using System.Collections.Concurrent;
using MDMS.HES.Core.Common;
using MDMS.HES.Core.Models;
using Microsoft.Extensions.Logging;

namespace MDMS.HES.Application.Services;

public class ObisDateTimeStringTagBuilder
{
    private readonly ILogger<ObisDateTimeStringTagBuilder>? _logger;

    // Structure holding the 4 datetime groups for a meter
    public class MeterDateTimeState
    {
        public DateTimeGroup Current { get; } = new(ObisConstants.ObisCurrentTime, ObisConstants.ObisCurrentDate);
        public DateTimeGroup Mdc { get; } = new(ObisConstants.ObisMdcTime, ObisConstants.ObisMdcDate);
        public DateTimeGroup Mdp { get; } = new(ObisConstants.ObisMdpTime, ObisConstants.ObisMdpDate);
        public DateTimeGroup Reset { get; } = new(ObisConstants.ObisResetTime, ObisConstants.ObisResetDate);
    }

    public class DateTimeGroup
    {
        public string TimeObisCode { get; }
        public string DateObisCode { get; }

        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
        public int? Hour { get; set; }
        public int? Minute { get; set; }
        public int? Second { get; set; }
        public TagQuality Quality { get; set; } = TagQuality.Good;

        public DateTimeGroup(string timeObisCode, string dateObisCode)
        {
            TimeObisCode = timeObisCode;
            DateObisCode = dateObisCode;
        }

        public bool TryCompose(out string timeStr, out string dateStr)
        {
            timeStr = string.Empty;
            dateStr = string.Empty;

            if (!Year.HasValue || !Month.HasValue || !Day.HasValue ||
                !Hour.HasValue || !Minute.HasValue || !Second.HasValue)
            {
                return false;
            }

            int y = Year.Value;
            if (y < 100) y += 2000; // 24 -> 2024

            if (y < 2000 || y > 2100 ||
                Month.Value < 1 || Month.Value > 12 ||
                Day.Value < 1 || Day.Value > 31 ||
                Hour.Value < 0 || Hour.Value > 23 ||
                Minute.Value < 0 || Minute.Value > 59 ||
                Second.Value < 0 || Second.Value > 59)
            {
                return false;
            }

            try
            {
                // Validate calendar validity
                var dt = new DateTime(y, Month.Value, Day.Value, Hour.Value, Minute.Value, Second.Value);
                timeStr = dt.ToString("HH:mm:ss");
                dateStr = dt.ToString("yyyy-MM-dd");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    private readonly ConcurrentDictionary<string, MeterDateTimeState> _meterStates = new();

    public event Action<string, TagValue>? OnObisTagSynthesized;

    public ObisDateTimeStringTagBuilder(ILogger<ObisDateTimeStringTagBuilder>? logger = null)
    {
        _logger = logger;
    }

    public bool TryProcessTag(string meterId, TagValue tag, out List<TagValue> synthesizedTags)
    {
        synthesizedTags = new List<TagValue>();
        var (groupType, partType) = MatchDateTimePart(tag.Id);
        if (groupType == null || partType == null)
            return false;

        var state = _meterStates.GetOrAdd(meterId, _ => new MeterDateTimeState());
        var group = groupType switch
        {
            "Current" => state.Current,
            "MDC" => state.Mdc,
            "MDP" => state.Mdp,
            "Reset" => state.Reset,
            _ => null
        };

        if (group == null)
            return false;

        int intVal = ConvertToInt(tag.Value);
        switch (partType)
        {
            case "Year": group.Year = intVal; break;
            case "Month": group.Month = intVal; break;
            case "Day": group.Day = intVal; break;
            case "Hour": group.Hour = intVal; break;
            case "Minute": group.Minute = intVal; break;
            case "Second": group.Second = intVal; break;
        }

        if (tag.Quality != TagQuality.Good)
            group.Quality = tag.Quality;

        if (group.TryCompose(out var timeStr, out var dateStr))
        {
            var now = tag.Timestamp;
            var timeTag = new TagValue(group.TimeObisCode, timeStr, group.Quality, now);
            var dateTag = new TagValue(group.DateObisCode, dateStr, group.Quality, now);

            synthesizedTags.Add(timeTag);
            synthesizedTags.Add(dateTag);

            OnObisTagSynthesized?.Invoke(meterId, timeTag);
            OnObisTagSynthesized?.Invoke(meterId, dateTag);
        }

        return true;
    }

    private static (string? Group, string? Part) MatchDateTimePart(string tagId)
    {
        // Matches patterns like:
        // _AdvancedTags.Template.DT.Current.Year
        // DT.Current.Hour
        // M001._AdvancedTags.Template.DT.MDC.Minute
        string normalized = tagId.Replace('/', '.').Replace('\\', '.');
        var parts = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);

        int dtIndex = -1;
        for (int i = 0; i < parts.Length; i++)
        {
            if (string.Equals(parts[i], "DT", StringComparison.OrdinalIgnoreCase))
            {
                dtIndex = i;
                break;
            }
        }

        if (dtIndex >= 0 && dtIndex + 2 < parts.Length)
        {
            string group = parts[dtIndex + 1];
            string part = parts[dtIndex + 2];

            string canonicalGroup = group.ToUpperInvariant() switch
            {
                "CURRENT" => "Current",
                "MDC" => "MDC",
                "MDP" => "MDP",
                "RESET" => "Reset",
                _ => group
            };

            string canonicalPart = part.ToUpperInvariant() switch
            {
                "YEAR" => "Year",
                "MONTH" => "Month",
                "DAY" => "Day",
                "HOUR" => "Hour",
                "MINUTE" => "Minute",
                "SECOND" => "Second",
                _ => part
            };

            return (canonicalGroup, canonicalPart);
        }

        return (null, null);
    }

    private static int ConvertToInt(object? value)
    {
        if (value == null) return 0;
        if (value is int i) return i;
        if (value is short s) return s;
        if (value is ushort us) return us;
        if (value is byte b) return b;
        if (value is long l) return (int)l;
        if (value is double d) return (int)Math.Round(d);
        if (value is float f) return (int)Math.Round(f);
        if (int.TryParse(value.ToString(), out var parsed)) return parsed;
        return 0;
    }
}

