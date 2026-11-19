using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using MDMS.HES.Core.Interfaces;
using MDMS.HES.Core.Models;
using Microsoft.Extensions.Logging;
using OPCAutomation;

namespace MDMS.HES.Infrastructure.OpcDa;

public class OpcDaDriver : IProtocolDriver, IDisposable
{
    private readonly ILogger<OpcDaDriver>? _logger;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TagValue>> _latestTags = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<MeterConfig> _meters = new();

    private OPCServer? _opcServer;
    private OPCGroups? _opcGroups;
    private OPCGroup? _opcGroup;

    private readonly ConcurrentDictionary<int, (string MeterId, string TagId)> _clientHandleMap = new();
    private int _handleCounter = 0;

    private Thread? _workerThread;
    private volatile bool _running;
    private volatile bool _connected;
    private DateTime _lastDataReceivedAt = DateTime.MinValue;

    public string DriverName => "OPC-DA";
    public bool IsConnected => _connected;

    public string ProgId { get; set; } = "Kepware.KEPServerEX.V6";
    public string GroupName { get; set; } = "MDMS_HES_Group";
    public int UpdateRateMs { get; set; } = 1000;

    public event Action<string, TagValue>? OnTagChanged;

    public OpcDaDriver(ILogger<OpcDaDriver>? logger = null)
    {
        _logger = logger;
    }

    public Task InitializeAsync(IEnumerable<MeterConfig> meters, CancellationToken cancellationToken)
    {
        _meters.Clear();
        _meters.AddRange(meters);
        _logger?.LogInformation("[OPC-DA] Initialized with {Count} meters. Target ProgID: {ProgId}", _meters.Count, ProgId);
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_running)
            return Task.CompletedTask;

        _running = true;
        _workerThread = new Thread(RunWorkerLoop)
        {
            IsBackground = true,
            Name = "MDMS.HES.OpcDaWorker"
        };
        _workerThread.Start();
        _logger?.LogInformation("[OPC-DA] Worker thread started.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_running)
            return Task.CompletedTask;

        _running = false;
        if (_workerThread != null && _workerThread.IsAlive)
        {
            _workerThread.Join(3000);
        }
        CleanupComObjects();
        _connected = false;
        _logger?.LogInformation("[OPC-DA] Driver stopped.");
        return Task.CompletedTask;
    }

    public IReadOnlyDictionary<string, TagValue> GetLatestTags(string meterId)
    {
        if (_latestTags.TryGetValue(meterId, out var dict))
            return dict;
        return new Dictionary<string, TagValue>();
    }

    private void RunWorkerLoop()
    {
        while (_running)
        {
            if (!_connected)
            {
                try
                {
                    _logger?.LogInformation("[OPC-DA] Connecting to {ProgId}...", ProgId);
                    _opcServer = new OPCServer();
                    _opcServer.Connect(ProgId, "");

                    _opcGroups = _opcServer.OPCGroups;
                    _opcGroups.DefaultGroupIsActive = true;
                    _opcGroups.DefaultGroupUpdateRate = UpdateRateMs;

                    _opcGroup = _opcGroups.Add(GroupName);
                    _opcGroup.IsActive = true;
                    _opcGroup.IsSubscribed = true;

                    _connected = true;
                    _lastDataReceivedAt = DateTime.UtcNow;
                    _logger?.LogInformation("[OPC-DA] Connected successfully to {ProgId}.", ProgId);

                    // Add items for meters
                    RegisterMeterItems();

                    // Hook events
                    _opcGroup.DataChange += OnOpcGroupDataChange;
                    _opcServer.ServerShutDown += OnOpcServerShutdown;
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning("[OPC-DA] Connection attempt failed: {Message}. Retrying in 5s...", ex.Message);
                    CleanupComObjects();
                    for (int i = 0; i < 5 && _running; i++) Thread.Sleep(1000);
                    continue;
                }
            }

            // Watchdog loop
            Thread.Sleep(1000);
            if (_connected && _opcServer != null)
            {
                try
                {
                    if (_opcServer.ServerState != (int)OPCServerState.OPCRunning)
                    {
                        _logger?.LogWarning("[OPC-DA] Watchdog detected server state != OPCRunning ({State}). Reconnecting...", _opcServer.ServerState);
                        CleanupComObjects();
                    }
                }
                catch
                {
                    CleanupComObjects();
                }
            }
        }

        CleanupComObjects();
    }

    private void RegisterMeterItems()
    {
        if (_opcGroup == null) return;
        var opcItems = _opcGroup.OPCItems;
        if (opcItems == null) return;

        _clientHandleMap.Clear();
        _handleCounter = 0;

        foreach (var meter in _meters)
        {
            if (!meter.Enabled) continue;

            // List of tags to subscribe for each meter
            var tagsToRegister = GetTagNamesForMeter(meter.MeterId);
            foreach (var tagPath in tagsToRegister)
            {
                int handle = Interlocked.Increment(ref _handleCounter);
                _clientHandleMap[handle] = (meter.MeterId, tagPath);

                try
                {
                    opcItems.AddItem(tagPath, handle);
                }
                catch (Exception ex)
                {
                    _logger?.LogTrace("[OPC-DA] AddItem failed for {Tag}: {Msg}", tagPath, ex.Message);
                }
            }
        }
        _logger?.LogInformation("[OPC-DA] Registered {Count} items across {MeterCount} meters.", _clientHandleMap.Count, _meters.Count);
    }

    private static List<string> GetTagNamesForMeter(string meterId)
    {
        var list = new List<string>();
        // Electrical parameters
        list.Add($"{meterId}._AdvancedTags.Template.Meas.13_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.14_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.21_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.1_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.3_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.32_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.41_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.43_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.51_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.52_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.61_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.63_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.71_7_0");
        list.Add($"{meterId}._AdvancedTags.Template.Meas.72_7_0");

        // Energy cumulative
        list.Add($"{meterId}._AdvancedTags.Template.Energy.1_8_0");
        list.Add($"{meterId}._AdvancedTags.Template.Energy.1_8_0_1");
        list.Add($"{meterId}._AdvancedTags.Template.Energy.1_8_1");
        list.Add($"{meterId}._AdvancedTags.Template.Energy.1_8_1_1");
        list.Add($"{meterId}._AdvancedTags.Template.Energy.1_8_2");
        list.Add($"{meterId}._AdvancedTags.Template.Energy.1_8_2_1");
        list.Add($"{meterId}._AdvancedTags.Template.Energy.1_8_3");
        list.Add($"{meterId}._AdvancedTags.Template.Energy.1_8_3_1");
        list.Add($"{meterId}._AdvancedTags.Template.Energy.3_8_0");
        list.Add($"{meterId}._AdvancedTags.Template.Energy.3_8_0_1");

        // Alarms
        list.Add($"{meterId}._AdvancedTags.Template.Status.96_5_0");
        list.Add($"{meterId}._AdvancedTags.Template.Status.97_97_1");
        list.Add($"{meterId}._AdvancedTags.Template.Status.97_97_2");
        list.Add($"{meterId}._AdvancedTags.Template.Status.97_97_3");
        list.Add($"{meterId}._AdvancedTags.Template.Status.97_97_4");
        list.Add($"{meterId}._AdvancedTags.Template.Status.97_97_5");
        list.Add($"{meterId}._AdvancedTags.Template.Status.97_97_6");
        list.Add($"{meterId}._AdvancedTags.Template.Status.97_97_7");

        // 24 Short DateTime parts
        string[] groups = { "Current", "MDC", "MDP", "Reset" };
        string[] parts = { "Year", "Month", "Day", "Hour", "Minute", "Second" };
        foreach (var g in groups)
        {
            foreach (var p in parts)
            {
                list.Add($"{meterId}._AdvancedTags.Template.DT.{g}.{p}");
            }
        }

        return list;
    }

    private void OnOpcGroupDataChange(int transactionId, int numItems, ref Array clientHandles, ref Array itemValues, ref Array qualities, ref Array timeStamps)
    {
        _lastDataReceivedAt = DateTime.UtcNow;

        for (int i = 1; i <= numItems; i++)
        {
            try
            {
                int handle = (int)clientHandles.GetValue(i)!;
                if (_clientHandleMap.TryGetValue(handle, out var tuple))
                {
                    object? val = itemValues.GetValue(i);
                    int qInt = (int)qualities.GetValue(i)!;
                    DateTime dt = (DateTime)timeStamps.GetValue(i)!;

                    var quality = (qInt & 0xC0) == 0xC0 ? TagQuality.Good : TagQuality.Bad;
                    var tag = new TagValue(tuple.TagId, val, quality, dt);

                    var dict = _latestTags.GetOrAdd(tuple.MeterId, _ => new ConcurrentDictionary<string, TagValue>(StringComparer.OrdinalIgnoreCase));
                    dict[tuple.TagId] = tag;

                    OnTagChanged?.Invoke(tuple.MeterId, tag);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogTrace(ex, "[OPC-DA] Error parsing DataChange item.");
            }
        }
    }

    private void OnOpcServerShutdown(string reason)
    {
        _logger?.LogWarning("[OPC-DA] Server signaled shutdown: {Reason}", reason);
        CleanupComObjects();
    }

    private void CleanupComObjects()
    {
        _connected = false;
        try
        {
            if (_opcGroup != null)
            {
                try { _opcGroup.DataChange -= OnOpcGroupDataChange; } catch { }
                _opcGroup = null;
            }
            if (_opcGroups != null)
            {
                _opcGroups = null;
            }
            if (_opcServer != null)
            {
                try { _opcServer.ServerShutDown -= OnOpcServerShutdown; } catch { }
                try { _opcServer.Disconnect(); } catch { }
                try { Marshal.ReleaseComObject(_opcServer); } catch { }
                _opcServer = null;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogTrace(ex, "[OPC-DA] Error during COM cleanup.");
        }
    }

    public void Dispose()
    {
        _running = false;
        CleanupComObjects();
    }
}
