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
    private readonly ConcurrentDictionary<string, double> _accumulatedKwh = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<MeterConfig> _meters = new();

    private CancellationTokenSource? _cts;
    private Task? _simulationLoopTask;
    private volatile bool _isConnected;
    private double _tick = 0;

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

        var rnd = new Random(42);
        foreach (var m in _meters)
        {
            _accumulatedKwh[m.MeterId] = 12500.0 + rnd.Next(500, 15000);
        }

        // Execute one initial tick immediately so buffers are pre-warmed before the first publish cycle
        SimulateSingleTick(new Random());

        _logger?.LogInformation("[Simulator] Initialized and pre-warmed {Count} virtual meters.", _meters.Count);
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

        while (!token.IsCancellationRequested)
        {
            try
            {
                SimulateSingleTick(rnd);
                await Task.Delay(5000, token);
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

    private void SimulateSingleTick(Random rnd)
    {
        var now = DateTime.Now;
        _tick += 0.15;

        int idx = 0;
        foreach (var meter in _meters)
        {
            idx++;
            if (!meter.Enabled) continue;

            var dict = _latestTags.GetOrAdd(meter.MeterId, _ => new ConcurrentDictionary<string, TagValue>(StringComparer.OrdinalIgnoreCase));

            double phaseOffset = idx * 0.37;

            // 1. Voltages (Ua, Ub, Uc ~ 219.0 - 223.5 kV)
            double ua = Math.Round(221.0 + Math.Sin(_tick + phaseOffset) * 2.2, 2);
            double ub = Math.Round(220.8 + Math.Sin(_tick + phaseOffset + 2.09) * 2.2, 2);
            double uc = Math.Round(221.2 + Math.Sin(_tick + phaseOffset + 4.18) * 2.2, 2);

            // 2. Currents (Ia, Ib, Ic ~ 15.0 - 105.0 A)
            double ia = Math.Round(60.0 + Math.Cos(_tick * 0.7 + phaseOffset) * 35.0, 2);
            double ib = Math.Round(58.0 + Math.Cos(_tick * 0.7 + phaseOffset + 1.1) * 34.0, 2);
            double ic = Math.Round(62.0 + Math.Cos(_tick * 0.7 + phaseOffset + 2.2) * 36.0, 2);

            // 3. Power Factor & Frequency
            double pfA = Math.Round(0.91 + Math.Sin(_tick * 0.3 + phaseOffset) * 0.03, 4);
            double pfB = Math.Round(0.90 + Math.Cos(_tick * 0.3 + phaseOffset) * 0.03, 4);
            double pfC = Math.Round(0.92 + Math.Sin(_tick * 0.4 + phaseOffset) * 0.03, 4);
            double pfAvg = Math.Round((pfA + pfB + pfC) / 3.0, 4);
            double freq = Math.Round(50.0 + (rnd.NextDouble() - 0.5) * 0.08, 2);

            // 4. Active Power (Pa, Pb, Pc, Pt in kW) & Reactive Power (Qa, Qb, Qc, Qt in kVAr)
            double pa = Math.Round(ua * ia * pfA / 1000.0 * 3.0, 4);
            double pb = Math.Round(ub * ib * pfB / 1000.0 * 3.0, 4);
            double pc = Math.Round(uc * ic * pfC / 1000.0 * 3.0, 4);
            double pt = Math.Round(pa + pb + pc, 4);

            double qa = Math.Round(pa * Math.Tan(Math.Acos(pfA)), 4);
            double qb = Math.Round(pb * Math.Tan(Math.Acos(pfB)), 4);
            double qc = Math.Round(pc * Math.Tan(Math.Acos(pfC)), 4);
            double qt = Math.Round(qa + qb + qc, 4);

            // Emit Electrical Measurements (exact Kepware tag names -> auto-mapped via OBIS tail)
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.32_7_0", ua, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.52_7_0", ub, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.72_7_0", uc, now);

            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.31_7_0", ia, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.51_7_0", ib, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.71_7_0", ic, now);

            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.21_7_0", pa, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.41_7_0", pb, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.61_7_0", pc, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.1_7_0", pt, now);

            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.23_7_0", qa, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.43_7_0", qb, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.63_7_0", qc, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.3_7_0", qt, now);

            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.13_7_0", pfAvg, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Meas.14_7_0", freq, now);

            // 5. Cumulative Energy & Billing Reset Energy
            double baseKwh = _accumulatedKwh.AddOrUpdate(
                meter.MeterId,
                15000.0,
                (_, prev) => Math.Round(prev + (pt / 3600.0) * 5.0, 2));

            double btKwh = Math.Round(baseKwh * 0.52, 2);
            double cdKwh = Math.Round(baseKwh * 0.28, 2);
            double tdKwh = Math.Round(baseKwh - btKwh - cdKwh, 2);
            double qTotal = Math.Round(baseKwh * 0.42, 2);

            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.1_8_0", baseKwh, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.1_8_1", btKwh, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.1_8_2", cdKwh, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.1_8_3", tdKwh, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.3_8_0", qTotal, now);

            // Billing reset snapshot (beginning of month)
            double resetKwh = Math.Round(baseKwh * 0.92, 2);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.1_8_0_1", resetKwh, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.1_8_1_1", Math.Round(resetKwh * 0.52, 2), now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.1_8_2_1", Math.Round(resetKwh * 0.28, 2), now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.1_8_3_1", Math.Round(resetKwh * 0.20, 2), now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Energy.3_8_0_1", Math.Round(resetKwh * 0.42, 2), now);

            // Maximum demand
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.MD.1_6_0", Math.Round(pt * 1.15, 4), now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.MD.1_6_0_1", Math.Round(pt * 1.12, 4), now);

            // 6. Status / Alarms (Healthy = false)
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Status.96_5_0", false, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Status.97_97_1", false, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Status.97_97_2", false, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Status.97_97_3", false, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Status.97_97_4", false, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Status.97_97_5", false, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Status.97_97_6", false, now);
            UpdateAndEmit(meter.MeterId, dict, "_AdvancedTags.Template.Status.97_97_7", false, now);

            // 7. Emit 24 Short DateTime parts (Current, MDC, MDP, Reset) -> converted to 0_9_1..0_9_8 by ObisDateTimeStringTagBuilder
            EmitDateTimeParts(meter.MeterId, dict, "Current", now);
            EmitDateTimeParts(meter.MeterId, dict, "MDC", now);
            EmitDateTimeParts(meter.MeterId, dict, "MDP", now.AddDays(-1));
            EmitDateTimeParts(meter.MeterId, dict, "Reset", new DateTime(now.Year, now.Month, 1, 0, 0, 0));
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
