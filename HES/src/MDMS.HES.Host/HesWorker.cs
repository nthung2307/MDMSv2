using System.Diagnostics;
using System.Text.Json;
using MDMS.HES.Application.Services;
using MDMS.HES.Core.Interfaces;
using MDMS.HES.Core.Models;
using MDMS.HES.Infrastructure.OpcDa;
using MDMS.HES.Infrastructure.Simulation;
using MDMS.HES.Infrastructure.Uplink;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MDMS.HES.Host;

public class HesWorker : BackgroundService
{
    private readonly ILogger<HesWorker> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _configuration;

    private readonly List<MeterConfig> _meters = new();
    private readonly Dictionary<string, DateTime> _lastPublishedTimes = new(StringComparer.OrdinalIgnoreCase);

    private IProtocolDriver? _driver;
    private ObisDateTimeStringTagBuilder? _obisDateTimeBuilder;
    private ISmartBufferManager? _bufferManager;
    private ICircuitBreaker? _circuitBreaker;
    private RestTelemetryPublisher? _publisher;

    private string _driverType = "Simulation";
    private string _targetUrl = "http://192.168.28.130:19089/IoT/telemetry";
    private string? _globalApiKey;
    private int _defaultScanRateMs = 60000;
    private int _maxEventsPerPublish = 150;
    private int _probeIntervalSeconds = 5;

    public HesWorker(
        ILogger<HesWorker> logger,
        ILoggerFactory loggerFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _configuration = configuration;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        LoadConfig();
        LoadMeters();

        // 1. Initialize Core Services
        _obisDateTimeBuilder = new ObisDateTimeStringTagBuilder(_loggerFactory.CreateLogger<ObisDateTimeStringTagBuilder>());
        _bufferManager = new SmartBufferManager(_loggerFactory.CreateLogger<SmartBufferManager>());

        var cb = new CircuitBreaker(_loggerFactory.CreateLogger<CircuitBreaker>())
        {
            FailureThreshold = 1,
            SuccessThreshold = 2,
            ProbeInterval = TimeSpan.FromSeconds(_probeIntervalSeconds)
        };
        _circuitBreaker = cb;

        _publisher = new RestTelemetryPublisher(
            _circuitBreaker,
            _targetUrl,
            _globalApiKey,
            _loggerFactory.CreateLogger<RestTelemetryPublisher>());

        // 2. Hook OBIS DateTime string synthesis
        _obisDateTimeBuilder.OnObisTagSynthesized += (meterId, tag) =>
        {
            _bufferManager.RecordTag(meterId, tag);
        };

        // 3. Instantiate Driver
        if (string.Equals(_driverType, "OPC-DA", StringComparison.OrdinalIgnoreCase))
        {
            var opcDriver = new OpcDaDriver(_loggerFactory.CreateLogger<OpcDaDriver>());
            var opcSection = _configuration.GetSection("HesConfig:OpcDa");
            if (opcSection.Exists())
            {
                opcDriver.ProgId = opcSection["ProgId"] ?? "Kepware.KEPServerEX.V6";
                opcDriver.GroupName = opcSection["GroupName"] ?? "MDMS_HES_OPCDA";
                if (int.TryParse(opcSection["UpdateRateMs"], out int rate))
                    opcDriver.UpdateRateMs = rate;
            }
            _driver = opcDriver;
            _logger.LogInformation("[HES Host] Selected Driver: OPC-DA Client (Target: {ProgId})", opcDriver.ProgId);
        }
        else
        {
            _driver = new SimulationDriver(_loggerFactory.CreateLogger<SimulationDriver>());
            _logger.LogInformation("[HES Host] Selected Driver: Built-in Simulation Driver");
        }

        // 4. Hook Driver events
        _driver.OnTagChanged += OnDriverTagChanged;

        return base.StartAsync(cancellationToken);
    }

    private void OnDriverTagChanged(string meterId, TagValue tag)
    {
        if (_bufferManager == null || _obisDateTimeBuilder == null) return;

        // Process 24 short datetime parts (Year, Month, Day, Hour, Minute, Second) -> synthesizes 0_9_1..0_9_8
        if (_obisDateTimeBuilder.TryProcessTag(meterId, tag, out _))
        {
            // Synthesized OBIS string tags are recorded via OnObisTagSynthesized
            return;
        }

        // Record the measurement / energy / status tag into SmartBuffer
        _bufferManager.RecordTag(meterId, tag);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[HES Host] Initializing and starting driver with {Count} meters...", _meters.Count);
        if (_driver != null)
        {
            await _driver.InitializeAsync(_meters, stoppingToken);
            await _driver.StartAsync(stoppingToken);
        }

        _logger.LogInformation("[HES Host] MDMS.HES Engine is RUNNING. Target URL: {Url}", _targetUrl);

        var nextStatsLogTime = DateTime.UtcNow.AddSeconds(30);
        var probeMeter = _meters.FirstOrDefault(m => m.Enabled && !string.IsNullOrWhiteSpace(m.Serial));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;

                // 1. Periodic Stats Log
                if (now >= nextStatsLogTime)
                {
                    _logger.LogInformation("[HES Stats] Circuit: {CircuitState} | Meters Cached: {CachedMeters} | Active Drivers: {DriverConnected}",
                        _circuitBreaker?.State, _bufferManager?.GetCachedMeterCount(), _driver?.IsConnected);
                    nextStatsLogTime = now.AddSeconds(60);
                }

                // 2. Handle Circuit Breaker Outage / Probe mode
                if (_circuitBreaker != null && (_circuitBreaker.State == CircuitState.Open || _circuitBreaker.State == CircuitState.HalfOpen))
                {
                    if (_circuitBreaker.CanProbe() && _publisher != null)
                    {
                        _logger.LogInformation("[HES Heartbeat] Circuit is {State}. Probing server {Url}...", _circuitBreaker.State, _targetUrl);
                        await _publisher.ProbeServerAsync(probeMeter?.Serial, probeMeter?.ApiKey, stoppingToken);
                    }

                    // While circuit is open/half-open, do NOT publish normal meters!
                    await Task.Delay(500, stoppingToken);
                    continue;
                }

                // 3. Normal Publish Loop (Circuit is CLOSED)
                if (_publisher != null && _bufferManager != null && _circuitBreaker?.CanPublish() == true)
                {
                    int publishedMetersInBatch = 0;
                    int totalTagsInBatch = 0;
                    var sw = Stopwatch.StartNew();

                    foreach (var meter in _meters)
                    {
                        if (stoppingToken.IsCancellationRequested) break;
                        if (!meter.Enabled) continue;

                        // Check if scan rate elapsed
                        if (!_lastPublishedTimes.TryGetValue(meter.MeterId, out var lastTime) ||
                            (now - lastTime).TotalMilliseconds >= meter.ScanRateMs)
                        {
                            var payload = _bufferManager.DrainPendingPayload(meter.MeterId, meter.Serial, _maxEventsPerPublish);
                            if (payload != null && payload.Values.Count > 0)
                            {
                                bool success = await _publisher.PublishTelemetryAsync(
                                    payload,
                                    meter.Serial,
                                    meter.ApiKey,
                                    stoppingToken);

                                if (success)
                                {
                                    _lastPublishedTimes[meter.MeterId] = now;
                                    publishedMetersInBatch++;
                                    totalTagsInBatch += payload.Values.Count;
                                }
                                else
                                {
                                    // If publish failed due to network error and tripped circuit breaker, break immediately!
                                    if (!_circuitBreaker.CanPublish())
                                    {
                                        _logger.LogWarning("[HES Host] Publish failed for {MeterId} ({Serial}). Circuit breaker TRIPPED! Suspending remaining meters.",
                                            meter.MeterId, meter.Serial);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    sw.Stop();
                    if (publishedMetersInBatch > 0)
                    {
                        _logger.LogInformation("[HES Uplink] Published {PublishedCount}/{TotalMeters} meters ({TagCount} tags) to {Url} in {ElapsedMs}ms.",
                            publishedMetersInBatch, _meters.Count, totalTagsInBatch, _targetUrl, sw.ElapsedMilliseconds);
                    }
                }

                await Task.Delay(250, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HES Host] Error in main execution loop.");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[HES Host] Stopping HES Engine...");
        if (_driver != null)
        {
            _driver.OnTagChanged -= OnDriverTagChanged;
            await _driver.StopAsync(cancellationToken);
        }
        _publisher?.Dispose();
        await base.StopAsync(cancellationToken);
    }

    private void LoadConfig()
    {
        var section = _configuration.GetSection("HesConfig");
        if (section.Exists())
        {
            _driverType = section["DriverType"] ?? _driverType;
            _targetUrl = section["TargetUrl"] ?? _targetUrl;
            _globalApiKey = section["GlobalApiKey"];
            if (int.TryParse(section["DefaultScanRateMs"], out int rate)) _defaultScanRateMs = rate;
            if (int.TryParse(section["MaxEventsPerPublish"], out int maxEv)) _maxEventsPerPublish = maxEv;
            if (int.TryParse(section["ProbeIntervalSeconds"], out int probeSec)) _probeIntervalSeconds = probeSec;
        }

        _logger.LogInformation("[HES Host] Config Loaded: Driver={Driver}, TargetUrl={Url}, DefaultRate={Rate}ms",
            _driverType, _targetUrl, _defaultScanRateMs);
    }

    private void LoadMeters()
    {
        _meters.Clear();
        string metersPath = Path.Combine(AppContext.BaseDirectory, "meters.json");
        if (!File.Exists(metersPath))
        {
            metersPath = Path.Combine(Directory.GetCurrentDirectory(), "meters.json");
        }

        if (File.Exists(metersPath))
        {
            try
            {
                string json = File.ReadAllText(metersPath);
                var loaded = JsonSerializer.Deserialize<List<MeterConfig>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loaded != null && loaded.Count > 0)
                {
                    _meters.AddRange(loaded);
                    _logger.LogInformation("[HES Host] Successfully loaded {Count} meters from {Path}", _meters.Count, metersPath);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[HES Host] Failed to load meters.json, falling back to defaults.");
            }
        }

        // Fallback default meters (162 meters)
        _logger.LogInformation("[HES Host] Generating 162 fallback meters...");
        for (int i = 1; i <= 162; i++)
        {
            string id = $"M{i:D3}";
            _meters.Add(new MeterConfig
            {
                MeterId = id,
                Serial = $"20955{i:D4}",
                Name = $"Meter {id}",
                Enabled = true,
                ScanRateMs = _defaultScanRateMs
            });
        }
    }
}
