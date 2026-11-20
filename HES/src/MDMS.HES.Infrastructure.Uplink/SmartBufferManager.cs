using System.Collections.Concurrent;
using MDMS.HES.Core.Interfaces;
using MDMS.HES.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDMS.HES.Infrastructure.Uplink;

public class SmartBufferManager : ISmartBufferManager
{
    private readonly ILogger<SmartBufferManager>? _logger;

    private class MeterBuffer
    {
        // Layer 1: In-place Latest Value Snapshot (Fixed memory per meter)
        public ConcurrentDictionary<string, TagValue> LatestSnapshot { get; } = new(StringComparer.OrdinalIgnoreCase);

        // Layer 2: State Transition Queue for Alarms / Status bits
        public ConcurrentQueue<TagValue> AlarmTransitionQueue { get; } = new();

        // Previous status cache to detect 0 <-> 1 transitions
        public ConcurrentDictionary<string, object?> LastStatusValues { get; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private readonly ConcurrentDictionary<string, MeterBuffer> _meterBuffers = new(StringComparer.OrdinalIgnoreCase);
    public int MaxQueuedAlarmsPerMeter { get; set; } = 500;

    public SmartBufferManager(ILogger<SmartBufferManager>? logger = null)
    {
        _logger = logger;
    }

    public void RecordTag(string meterId, TagValue tag)
    {
        if (string.IsNullOrWhiteSpace(meterId) || string.IsNullOrWhiteSpace(tag.Id))
            return;

        var buffer = _meterBuffers.GetOrAdd(meterId, _ => new MeterBuffer());

        // Always store latest snapshot for instantaneous values & energy
        buffer.LatestSnapshot[tag.Id] = tag;

        // Check if this tag represents a status/alarm bit (e.g., Status.*, 96_5_0, 97_97_*)
        if (IsStatusOrAlarmTag(tag.Id))
        {
            if (buffer.LastStatusValues.TryGetValue(tag.Id, out var prevVal))
            {
                if (!AreValuesEqual(prevVal, tag.Value))
                {
                    // State transitioned! Enqueue with timestamp
                    buffer.LastStatusValues[tag.Id] = tag.Value;
                    if (buffer.AlarmTransitionQueue.Count < MaxQueuedAlarmsPerMeter)
                    {
                        buffer.AlarmTransitionQueue.Enqueue(tag);
                    }
                    else
                    {
                        _logger?.LogWarning("[SmartBuffer] Alarm queue for meter {MeterId} reached cap ({Cap}). Oldest dropped.", meterId, MaxQueuedAlarmsPerMeter);
                        buffer.AlarmTransitionQueue.TryDequeue(out _);
                        buffer.AlarmTransitionQueue.Enqueue(tag);
                    }
                }
            }
            else
            {
                // First observation
                buffer.LastStatusValues[tag.Id] = tag.Value;
                // If it is an active alarm (1 / true), enqueue it
                if (IsActiveAlarm(tag.Value))
                {
                    buffer.AlarmTransitionQueue.Enqueue(tag);
                }
            }
        }
    }

    public TelemetryPayload? DrainPendingPayload(string meterId, string serial, int maxEvents = 150)
    {
        if (!_meterBuffers.TryGetValue(meterId, out var buffer))
            return null;

        var payload = new TelemetryPayload
        {
            Meter = meterId,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        // 1. Drain alarm transitions first (up to maxEvents / 2)
        int drainedAlarms = 0;
        while (drainedAlarms < maxEvents / 2 && buffer.AlarmTransitionQueue.TryDequeue(out var alarmTag))
        {
            payload.Values.Add(new TelemetryValueItem(
                alarmTag.Id,
                alarmTag.Value,
                alarmTag.QualityString,
                alarmTag.TimestampEpochMs
            ));
            drainedAlarms++;
        }

        // 2. Add latest snapshots (stamped with current scan timestamp so periodic scans advance LastTelemetryAt and snapshot UpdatedAt)
        long scanTimestampMs = payload.Timestamp;
        foreach (var kvp in buffer.LatestSnapshot)
        {
            if (payload.Values.Count >= maxEvents)
                break;

            var tag = kvp.Value;
            payload.Values.Add(new TelemetryValueItem(
                tag.Id,
                tag.Value,
                tag.QualityString,
                scanTimestampMs
            ));
        }

        return payload.Values.Count > 0 ? payload : null;
    }

    public int GetPendingAlarmCount(string meterId)
    {
        if (_meterBuffers.TryGetValue(meterId, out var buffer))
            return buffer.AlarmTransitionQueue.Count;
        return 0;
    }

    public int GetCachedMeterCount() => _meterBuffers.Count;

    public void Clear()
    {
        _meterBuffers.Clear();
    }

    private static bool IsStatusOrAlarmTag(string tagId)
    {
        string lower = tagId.ToLowerInvariant();
        return lower.Contains("status") ||
               lower.Contains("alarm") ||
               lower.Contains("96_5_0") ||
               lower.Contains("97_97_");
    }

    private static bool AreValuesEqual(object? v1, object? v2)
    {
        if (v1 == null && v2 == null) return true;
        if (v1 == null || v2 == null) return false;
        return v1.ToString()?.Trim() == v2.ToString()?.Trim();
    }

    private static bool IsActiveAlarm(object? val)
    {
        if (val == null) return false;
        if (val is bool b) return b;
        if (val is int i) return i != 0;
        if (val is short s) return s != 0;
        string str = val.ToString()?.Trim().ToLowerInvariant() ?? "";
        return str is "1" or "true" or "alarm";
    }
}

