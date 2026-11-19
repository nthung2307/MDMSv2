using System.Collections.Concurrent;
using MDMS.HES.Core.Common;
using MDMS.HES.Core.Interfaces;
using MDMS.HES.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDMS.HES.Infrastructure.Simulation;

public class SimulationDriver : IProtocolDriver, IDisposable
{
    private readonly ILogger<SimulationDriver>? _logger;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, TagValue>> _latestTags = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<MeterConfig> _meters = new();

    private CancellationTokenSource? _cts;
    private Task? _simulationLoopTask;
    private volatile bool _isConnected;

    public string DriverName => "BuiltInSimulator";
    public bool IsConnected => _isConnected;

    public event Action<string, TagValue>? OnTagChanged;

    public SimulationDriver(ILogger<SimulationDriver>? logger = null)
    {
        _logger = logger;
    }

    public Task InitializeAsync(IEnumerable<MeterConfig> meters, CancellationToken cancellationToken)
    {
        _meters.Clear();
        _meters.AddRange(meters);

        // Pre-populate initial state
        var now = DateTime.UtcNow;
        var rnd = new Random();

        foreach (var m in _meters)
        {
            var dict = _latestTags.GetOrAdd(m.MeterId, _ => new ConcurrentDictionary<string, TagValue>(StringComparer.OrdinalIgnoreCase));

            // Initial tariffs
            double baseKwh = 100000.0 + rnd.Next(1000, 50000);
            dict[ObisConstants.EnergyTotalImport] = new TagValue(ObisConstants.EnergyTotalImport, baseKwh, TagQuality.Good, now);
            dict[ObisConstants.EnergyNormalTariff] = new TagValue(ObisConstants.EnergyNormalTariff, baseKwh * 0.5, TagQuality.Good, now);
            dict[ObisConstants.EnergyLowTariff] = new TagValue(ObisConstants.EnergyLowTariff, baseKwh * 0.2, TagQuality.Good, now);
            dict[ObisConstants.EnergyPeakTariff] = new TagValue(ObisConstants.EnergyPeakTariff, baseKwh * 0.3, TagQuality.Good, now);

            double baseKvarh = baseKwh * 0.4;
            dict[ObisConstants.ReactiveTotalImport] = new TagValue(ObisConstants.ReactiveTotalImport, baseKvarh, TagQuality.Good, now);
            dict[ObisConstants.ReactiveNormalTariff] = new TagValue(ObisConstants.ReactiveNormalTariff, baseKvarh * 0.5, TagQuality.Good, now);
            dict[ObisConstants.ReactiveLowTariff] = new TagValue(ObisConstants.ReactiveLowTariff, baseKvarh * 0.2, TagQuality.Good, now);
            dict[ObisConstants.ReactivePeakTariff] = new TagValue(ObisConstants.ReactivePeakTariff, baseKvarh * 0.3, TagQuality.Good, now);

            // Alarms
            dict[ObisConstants.StatusInternalError] = new TagValue(ObisConstants.StatusInternalError, false, TagQuality.Good, now);
            dict[ObisConstants.AlarmPhase1Loss] = new TagValue(ObisConstants.AlarmPhase1Loss, false, TagQuality.Good, now);
            dict[ObisConstants.AlarmPhase2Loss] = new TagValue(ObisConstants.AlarmPhase2Loss, false, TagQuality.Good, now);
            dict[ObisConstants.AlarmPhase3Loss] = new TagValue(ObisConstants.AlarmPhase3Loss, false, TagQuality.Good, now);
        }

        _logger?.LogInformation("[Simulator] Initialized with {Count} meters.", _meters.Count);
        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_simulationLoopTask != null)
            return Task.CompletedTask;

        _cts = new CancellationTokenSource();
        _isConnected = true;
        _simulationLoopTask = Task.Run(() => RunSimulationLoopAsync(_cts.Token), CancellationToken.None);
        _logger?.LogInformation("[Simulator] Simulation loop started.");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _isConnected = false;
        if (_cts != null)
        {
            _cts.Cancel();
            if (_simulationLoopTask != null)
            {
                await Task.WhenAny(_simulationLoopTask, Task.Delay(2000, cancellationToken));
            }
            _cts.Dispose();
            _cts = null;
            _simulationLoopTask = null;
        }
        _logger?.LogInformation("[Simulator] Simulation loop stopped.");
    }

    public IReadOnlyDictionary<string, TagValue> GetLatestTags(string meterId)
    {
        if (_latestTags.TryGetValue(meterId, out var dict))
            return dict;
        return new Dictionary<string, TagValue>();
    }

    private async Task RunSimulationLoopAsync(CancellationToken token)
    {
        var rnd = new Random();
        double tick = 0;

        while (!token.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now; // Use local system time for realistic matching
                tick += 0.1;

                foreach (var meter in _meters)
                {
                    if (!meter.Enabled) continue;

                    var dict = _latestTags.GetOrAdd(meter.MeterId, _ => new ConcurrentDictionary<string, TagValue>(StringComparer.OrdinalIgnoreCase));

                    // 1. Simulate Electrical parameters
                    double uBase = 220.0 + Math.Sin(tick + meter.MeterId.GetHashCode() % 10) * 2.5; // ~217.5 - 222.5 kV
                    double iBase = 65.0 + Math.Cos(tick * 0.7 + meter.MeterId.GetHashCode() % 5) * 20.0; // ~45 - 85 A
                    double pf = 0.90 + (rnd.NextDouble() - 0.5) * 0.04;
                    double freq = 50.0 + (rnd.NextDouble() - 0.5) * 0.08;

                    double p = Math.Sqrt(3) * uBase * iBase * pf / 1000.0; // MW
                    double q = Math.Sqrt(3) * uBase * iBase * Math.Sin(Math.Acos(pf)) / 1000.0; // Mvar

                    UpdateAndEmit(meter.MeterId, dict, ObisConstants.VoltageL1, Math.Round(uBase, 2), now);
                    UpdateAndEmit(meter.MeterId, dict, ObisConstants.CurrentL1, Math.Round(iBase, 2), now);
                    UpdateAndEmit(meter.MeterId, dict, ObisConstants.ActivePowerTotal, Math.Round(p, 4), now);
                    UpdateAndEmit(meter.MeterId, dict, ObisConstants.ReactivePowerTotal, Math.Round(q, 4), now);
                    UpdateAndEmit(meter.MeterId, dict, ObisConstants.PowerFactorTotal, Math.Round(pf, 4), now);
                    UpdateAndEmit(meter.MeterId, dict, ObisConstants.Frequency, Math.Round(freq, 2), now);

                    // Also emit Kepware template alias formats if needed
                    UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.13_7_0", Math.Round(uBase, 2), now);
                    UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.14_7_0", Math.Round(iBase, 2), now);
                    UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.21_7_0", Math.Round(p, 4), now);
                    UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.1_7_0", Math.Round(p, 4), now);
                    UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.3_7_0", Math.Round(q, 4), now);

                    // 2. Increment Accumulative Energy (kWh)
                    if (dict.TryGetValue(ObisConstants.EnergyTotalImport, out var oldTotal) && oldTotal.Value is double totVal)
                    {
                        double newTotal = totVal + (p / 3600.0) * 5.0; // increment for 5s
                        UpdateAndEmit(meter.MeterId, dict, ObisConstants.EnergyTotalImport, Math.Round(newTotal, 2), now);
                        UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.1_8_0", Math.Round(newTotal, 2), now);
                    }

                    // 3. Emit 24 Short DateTime parts (Current, MDC, MDP, Reset)
                    EmitDateTimeParts(meter.MeterId, dict, "Current", now);
                    EmitDateTimeParts(meter.MeterId, dict, "MDC", now);
                    EmitDateTimeParts(meter.MeterId, dict, "MDP", now.AddMinutes(-15));
                    EmitDateTimeParts(meter.MeterId, dict, "Reset", new DateTime(now.Year, now.Month, 1, 0, 0, 0));
                }

                await Task.Delay(5000, token); // update cycle
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[Simulator] Exception in simulation loop.");
                await Task.Delay(2000, token);
            }
        }
    }

    private void EmitDateTimeParts(string meterId, ConcurrentDictionary<string, TagValue> dict, string group, DateTime dt)
    {
        UpdateAndEmit(meterId, dict, $"_AdvancedTags.Template.DT.{group}.Year", (short)dt.Year, dt);
        UpdateAndEmit(meterId, dict, $"_AdvancedTags.Template.DT.{group}.Month", (short)dt.Month, dt);
        UpdateAndEmit(meterId, dict, $"_AdvancedTags.Template.DT.{group}.Day", (short)dt.Day, dt);
        UpdateAndEmit(meterId, dict, $"_AdvancedTags.Template.DT.{group}.Hour", (short)dt.Hour, dt);
        UpdateAndEmit(meterId, dict, $"_AdvancedTags.Template.DT.{group}.Minute", (short)dt.Minute, dt);
        UpdateAndEmit(meterId, dict, $"_AdvancedTags.Template.DT.{group}.Second", (short)dt.Second, dt);
    }

    private void UpdateAndEmit(string meterId, ConcurrentDictionary<string, TagValue> dict, string tagId, object val, DateTime now)
    {
        var tag = new TagValue(tagId, val, TagQuality.Good, now);
        dict[tagId] = tag;
        OnTagChanged?.Invoke(meterId, tag);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
