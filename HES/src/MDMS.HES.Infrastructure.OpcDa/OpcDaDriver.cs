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

    // Maps OPC clientHandle -> TagPath (shared template tag or meter-specific tag)
    private readonly ConcurrentDictionary<int, string> _clientHandleToTagMap = new();
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

                    // Add items and trigger initial AsyncRead
                    var serverHandles = RegisterTemplateItems();

                    _opcGroup.AsyncReadComplete += OnOpcGroupAsyncReadComplete;
                    _opcGroup.DataChange += OnOpcGroupDataChange;
                    _opcServer.ServerShutDown += OnOpcServerShutdown;

                    if (serverHandles.Count > 0)
                    {
                        int numItems = serverHandles.Count;
                        int[] sHandles = new int[numItems + 1];
                        sHandles[0] = 0;
                        for (int i = 0; i < numItems; i++) sHandles[i + 1] = serverHandles[i];

                        Array sHandlesArray = sHandles;
                        _opcGroup.AsyncRead(numItems, ref sHandlesArray, out _, 0, out _);
                    }
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

    private List<int> RegisterTemplateItems()
    {
        var serverHandles = new List<int>();
        if (_opcGroup == null) return serverHandles;
        var opcItems = _opcGroup.OPCItems;
        if (opcItems == null) return serverHandles;

        _clientHandleToTagMap.Clear();
        _handleCounter = 0;

        var templateTags = GetCanonicalTemplateTags();
        foreach (var tagPath in templateTags)
        {
            int handle = Interlocked.Increment(ref _handleCounter);
            _clientHandleToTagMap[handle] = tagPath;

            try
            {
                var item = opcItems.AddItem(tagPath, handle);
                if (item != null)
                {
                    serverHandles.Add(item.ServerHandle);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("[OPC-DA] AddItem failed for {Tag}: {Msg}", tagPath, ex.Message);
            }
        }

        _logger?.LogInformation("[OPC-DA] Subscribed to {Subscribed}/{Total} template tags for {MeterCount} meters.",
            serverHandles.Count, templateTags.Count, _meters.Count);
        return serverHandles;
    }

    private static List<string> GetCanonicalTemplateTags()
    {
        var list = new List<string>
        {
            // Electrical parameters (16 tags)
            "_AdvancedTags.Template.Meas.1_7_0",
            "_AdvancedTags.Template.Meas.13_7_0",
            "_AdvancedTags.Template.Meas.14_7_0",
            "_AdvancedTags.Template.Meas.21_7_0",
            "_AdvancedTags.Template.Meas.23_7_0",
            "_AdvancedTags.Template.Meas.3_7_0",
            "_AdvancedTags.Template.Meas.31_7_0",
            "_AdvancedTags.Template.Meas.32_7_0",
            "_AdvancedTags.Template.Meas.41_7_0",
            "_AdvancedTags.Template.Meas.43_7_0",
            "_AdvancedTags.Template.Meas.51_7_0",
            "_AdvancedTags.Template.Meas.52_7_0",
            "_AdvancedTags.Template.Meas.61_7_0",
            "_AdvancedTags.Template.Meas.63_7_0",
            "_AdvancedTags.Template.Meas.71_7_0",
            "_AdvancedTags.Template.Meas.72_7_0",

            // Energy cumulative & billing reset (10 tags)
            "_AdvancedTags.Template.Energy.1_8_0",
            "_AdvancedTags.Template.Energy.1_8_0_1",
            "_AdvancedTags.Template.Energy.1_8_1",
            "_AdvancedTags.Template.Energy.1_8_1_1",
            "_AdvancedTags.Template.Energy.1_8_2",
            "_AdvancedTags.Template.Energy.1_8_2_1",
            "_AdvancedTags.Template.Energy.1_8_3",
            "_AdvancedTags.Template.Energy.1_8_3_1",
            "_AdvancedTags.Template.Energy.3_8_0",
            "_AdvancedTags.Template.Energy.3_8_0_1",

            // Max Demand (2 tags)
            "_AdvancedTags.Template.MD.1_6_0",
            "_AdvancedTags.Template.MD.1_6_0_1",

            // Alarms / Status (8 tags)
            "_AdvancedTags.Template.Status.96_5_0",
            "_AdvancedTags.Template.Status.97_97_1",
            "_AdvancedTags.Template.Status.97_97_2",
            "_AdvancedTags.Template.Status.97_97_3",
            "_AdvancedTags.Template.Status.97_97_4",
            "_AdvancedTags.Template.Status.97_97_5",
            "_AdvancedTags.Template.Status.97_97_6",
            "_AdvancedTags.Template.Status.97_97_7"
        };

        // 24 Short DateTime parts
        string[] groups = { "Current", "MDC", "MDP", "Reset" };
        string[] parts = { "Year", "Month", "Day", "Hour", "Minute", "Second" };
        foreach (var g in groups)
        {
            foreach (var p in parts)
            {
                list.Add($"_AdvancedTags.Template.DT.{g}.{p}");
            }
        }

        return list;
    }

    private void OnOpcGroupAsyncReadComplete(
        int transactionId,
        int numItems,
        ref Array clientHandles,
        ref Array itemValues,
        ref Array qualities,
        ref Array timeStamps,
        ref Array errors)
    {
        ProcessOpcValues(numItems, ref clientHandles, ref itemValues, ref qualities, ref timeStamps);
    }

    private void OnOpcGroupDataChange(
        int transactionId,
        int numItems,
        ref Array clientHandles,
        ref Array itemValues,
        ref Array qualities,
        ref Array timeStamps)
    {
        ProcessOpcValues(numItems, ref clientHandles, ref itemValues, ref qualities, ref timeStamps);
    }

    private volatile bool _loggedInitialSample;

    private void ProcessOpcValues(
        int numItems,
        ref Array clientHandles,
        ref Array itemValues,
        ref Array qualities,
        ref Array timeStamps)
    {
        _lastDataReceivedAt = DateTime.UtcNow;
        int goodCount = 0;
        int badCount = 0;
        string? sampleInfo = null;

        for (int i = 1; i <= numItems; i++)
        {
            try
            {
                int handle = Convert.ToInt32(clientHandles.GetValue(i));
                if (_clientHandleToTagMap.TryGetValue(handle, out var tagId))
                {
                    object? val = itemValues.GetValue(i);
                    int qInt = Convert.ToInt32(qualities.GetValue(i));
                    DateTime rawDt = Convert.ToDateTime(timeStamps.GetValue(i));

                    // OPC-DA timestamps per OPC DA 2.05a specification are UTC FILETIMEs marshaled as Unspecified
                    DateTime utcDt = rawDt.Kind == DateTimeKind.Local
                        ? rawDt.ToUniversalTime()
                        : DateTime.SpecifyKind(rawDt, DateTimeKind.Utc);

                    var quality = (qInt & 0xC0) == 0xC0 ? TagQuality.Good : TagQuality.Bad;
                    if (quality == TagQuality.Good) goodCount++; else badCount++;

                    sampleInfo ??= $"{tagId}={val} (rawQ={qInt}, q={quality}, utcDt={utcDt:yyyy-MM-dd HH:mm:ss})";

                    var tag = new TagValue(tagId, val, quality, utcDt);

                    // Fan out shared template tag update to all enabled meters
                    foreach (var meter in _meters)
                    {
                        if (!meter.Enabled) continue;

                        var dict = _latestTags.GetOrAdd(meter.MeterId, _ => new ConcurrentDictionary<string, TagValue>(StringComparer.OrdinalIgnoreCase));
                        dict[tagId] = tag;

                        OnTagChanged?.Invoke(meter.MeterId, tag);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogTrace(ex, "[OPC-DA] Error parsing DataChange item.");
            }
        }

        if (!_loggedInitialSample && (goodCount > 0 || badCount > 0))
        {
            _loggedInitialSample = true;
            _logger?.LogInformation("[OPC-DA] First batch received ({NumItems} items): Good={Good}, Bad={Bad}. Sample: {Sample}",
                numItems, goodCount, badCount, sampleInfo);
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
                try { _opcGroup.AsyncReadComplete -= OnOpcGroupAsyncReadComplete; } catch { }
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
