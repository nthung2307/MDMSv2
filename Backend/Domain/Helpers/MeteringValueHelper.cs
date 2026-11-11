using System;
using System.Globalization;
using Domain.Enums;

namespace Domain.Helpers;

/// <summary>
/// Shared helper for parsing numeric telemetry strings, packing/formatting suspected energy flags,
/// and calculating monthly partition keys (UTC+7).
/// </summary>
public static class MeteringValueHelper
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);
    private static readonly CultureInfo ViCulture = CultureInfo.GetCultureInfo("vi-VN");

    public static (decimal? Value, bool IsSuspected) ParseTelemetryDecimal(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return (null, false);
        }

        var isSuspected = raw.Contains('*') ||
                          raw.Contains("Not Topical", StringComparison.OrdinalIgnoreCase);

        var cleaned = raw
            .Replace("*", string.Empty, StringComparison.Ordinal)
            .Replace("(Not Topical)", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        if (string.IsNullOrEmpty(cleaned))
        {
            return (null, isSuspected);
        }

        cleaned = cleaned.Replace(" ", string.Empty, StringComparison.Ordinal);

        var dotIndex = cleaned.LastIndexOf('.');
        var commaIndex = cleaned.LastIndexOf(',');
        string normalized;

        if (dotIndex >= 0 && commaIndex >= 0)
        {
            // Both separators present: rightmost separator is the decimal point
            normalized = commaIndex > dotIndex
                ? cleaned.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.')
                : cleaned.Replace(",", string.Empty, StringComparison.Ordinal);
        }
        else if (commaIndex >= 0)
        {
            // Comma only: multiple commas indicate thousands grouping; single comma indicates decimal separator
            var commaCount = CountChar(cleaned, ',');
            normalized = commaCount > 1
                ? cleaned.Replace(",", string.Empty, StringComparison.Ordinal)
                : cleaned.Replace(',', '.');
        }
        else if (dotIndex >= 0)
        {
            // Dot only: multiple dots indicate vi-VN thousands grouping; single dot is decimal point
            var dotCount = CountChar(cleaned, '.');
            normalized = dotCount > 1
                ? cleaned.Replace(".", string.Empty, StringComparison.Ordinal)
                : cleaned;
        }
        else
        {
            normalized = cleaned;
        }

        const NumberStyles strictStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        if (decimal.TryParse(normalized, strictStyles, CultureInfo.InvariantCulture, out var parsedValue))
        {
            return (Math.Round(parsedValue, 4, MidpointRounding.AwayFromZero), isSuspected);
        }

        return (null, isSuspected);
    }

    private static int CountChar(string text, char target)
    {
        var count = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == target) count++;
        }
        return count;
    }

    public static decimal? ToNullableDecimal(double? value)
    {
        if (!value.HasValue || double.IsNaN(value.Value) || double.IsInfinity(value.Value))
        {
            return null;
        }

        return Math.Round((decimal)value.Value, 4, MidpointRounding.AwayFromZero);
    }

    public static short? ParseSmallInt(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return short.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    public static string? FormatEnergyReportValue(decimal? value, int suspectedMask, EnergySuspectedFlags channelFlag, string? nullFallback = null)
    {
        if (!value.HasValue)
        {
            return nullFallback;
        }

        var rounded = Math.Round(value.Value, 2, MidpointRounding.AwayFromZero);
        var formatted = rounded % 1m == 0m
            ? rounded.ToString("N0", ViCulture)
            : rounded.ToString("N2", ViCulture);

        var isSuspected = ((EnergySuspectedFlags)suspectedMask & channelFlag) == channelFlag;
        return isSuspected ? $"{formatted}*" : formatted;
    }

    public static int SetSuspectedFlag(int mask, EnergySuspectedFlags flag, bool isSuspected)
    {
        return isSuspected
            ? mask | (int)flag
            : mask & ~(int)flag;
    }

    public static bool HasSuspectedFlag(int mask, EnergySuspectedFlags flag)
    {
        return (mask & (int)flag) == (int)flag;
    }

    public static int ComputePartitionYm(DateTimeOffset timestamp)
    {
        var vnTime = timestamp.ToOffset(VietnamOffset);
        return vnTime.Year * 100 + vnTime.Month;
    }

    public static int GetRetentionCutoffPartitionYm(DateTimeOffset utcNow, int retentionMonths = 12)
    {
        var vnMonthStart = new DateTimeOffset(utcNow.ToOffset(VietnamOffset).Year, utcNow.ToOffset(VietnamOffset).Month, 1, 0, 0, 0, VietnamOffset);
        var cutoff = vnMonthStart.AddMonths(-(retentionMonths - 1));
        return cutoff.Year * 100 + cutoff.Month;
    }
}
