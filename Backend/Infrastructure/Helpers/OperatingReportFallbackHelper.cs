using Application.DataTransferObjects.Responses.Report;
using Domain.Entities;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Constants;

namespace Infrastructure.Helpers;

internal sealed record ConfiguredOperatingUnits(
    string VoltageA,
    string VoltageB,
    string VoltageC,
    string CurrentA,
    string CurrentB,
    string CurrentC,
    string ActivePower,
    string ReactivePower,
    string Frequency);

/// <summary>
/// Shared helper resolving configured TagName units and computing 30-minute operating report fallbacks
/// from partitioned MeasurementPointTimeSeries when aggregated MeasurementPointHalfHourOperatingReports rows are missing.
/// </summary>
internal static class OperatingReportFallbackHelper
{
    internal static readonly ConfiguredOperatingUnits DefaultUnits = new(
        VoltageA: "kV",
        VoltageB: "kV",
        VoltageC: "kV",
        CurrentA: "A",
        CurrentB: "A",
        CurrentC: "A",
        ActivePower: "kW",
        ReactivePower: "kVAr",
        Frequency: "Hz");

    public static async Task<ConfiguredOperatingUnits> GetConfiguredUnitsAsync(
        DbContext context,
        long measurementPointId,
        CancellationToken cancellationToken)
    {
        try
        {
            var tags = await context.Set<TagNameEntity>().AsNoTracking()
                .Where(t => t.MeasurementPointId == measurementPointId &&
                            !t.Deleted &&
                            !string.IsNullOrEmpty(t.Unit))
                .Select(t => new { t.Description, t.Unit })
                .ToListAsync(cancellationToken);

            if (tags.Count == 0)
            {
                return DefaultUnits;
            }

            string? FindUnit(string description) =>
                tags.FirstOrDefault(t => string.Equals(t.Description, description, StringComparison.OrdinalIgnoreCase))?.Unit?.Trim();

            var fallbackVoltage = FindUnit(ListDataMeasurementPoint.DienApPhaA)
                                  ?? FindUnit(ListDataMeasurementPoint.DienApPhaB)
                                  ?? FindUnit(ListDataMeasurementPoint.DienApPhaC)
                                  ?? tags.FirstOrDefault(t =>
                                         t.Description != null &&
                                         (t.Description.Contains("DIEN_AP", StringComparison.OrdinalIgnoreCase) ||
                                          t.Description.Contains("Điện áp", StringComparison.OrdinalIgnoreCase) ||
                                          t.Description.Contains("dien ap", StringComparison.OrdinalIgnoreCase)))?.Unit?.Trim()
                                  ?? DefaultUnits.VoltageA;

            string ResolveOrDefault(string description, string fallback)
            {
                var value = FindUnit(description);
                return string.IsNullOrWhiteSpace(value) ? fallback : value;
            }

            return new ConfiguredOperatingUnits(
                VoltageA: ResolveOrDefault(ListDataMeasurementPoint.DienApPhaA, fallbackVoltage),
                VoltageB: ResolveOrDefault(ListDataMeasurementPoint.DienApPhaB, fallbackVoltage),
                VoltageC: ResolveOrDefault(ListDataMeasurementPoint.DienApPhaC, fallbackVoltage),
                CurrentA: ResolveOrDefault(ListDataMeasurementPoint.DongDienPhaA, DefaultUnits.CurrentA),
                CurrentB: ResolveOrDefault(ListDataMeasurementPoint.DongDienPhaB, DefaultUnits.CurrentB),
                CurrentC: ResolveOrDefault(ListDataMeasurementPoint.DongDienPhaC, DefaultUnits.CurrentC),
                ActivePower: ResolveOrDefault(ListDataMeasurementPoint.CongSuatTacDungTong3Pha, DefaultUnits.ActivePower),
                ReactivePower: ResolveOrDefault(ListDataMeasurementPoint.CongSuatPhanKhangTong3Pha, DefaultUnits.ReactivePower),
                Frequency: ResolveOrDefault(ListDataMeasurementPoint.TanSoHeThong, DefaultUnits.Frequency));
        }
        catch
        {
            return DefaultUnits;
        }
    }

    public static async Task<string> GetConfiguredVoltageUnitAsync(
        DbContext context,
        long measurementPointId,
        CancellationToken cancellationToken)
    {
        var units = await GetConfiguredUnitsAsync(context, measurementPointId, cancellationToken);
        return units.VoltageA;
    }

    public static void ApplyConfiguredUnits(
        IEnumerable<OperatingParameterReportResponse> items,
        ConfiguredOperatingUnits units)
    {
        foreach (var item in items)
        {
            item.DienApPhaAUnit = units.VoltageA;
            item.DienApPhaBUnit = units.VoltageB;
            item.DienApPhaCUnit = units.VoltageC;
            item.DongDienPhaAUnit = units.CurrentA;
            item.DongDienPhaBUnit = units.CurrentB;
            item.DongDienPhaCUnit = units.CurrentC;
            item.CongSuatTacDungTong3PhaUnit = units.ActivePower;
            item.CongSuatPhanKhangTong3PhaUnit = units.ReactivePower;
            item.TanSoHeThongUnit = units.Frequency;
        }
    }

    public static async Task<List<MeasurementPointHalfHourOperatingReportEntity>> GetFallbackFromTimeSeriesAsync(
        DbContext context,
        long measurementPointId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken)
    {
        var point = await context.Set<MeasurementPointsEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == measurementPointId, cancellationToken);

        var startYm = MeteringValueHelper.ComputePartitionYm(startTime);
        var endYm = MeteringValueHelper.ComputePartitionYm(endTime);

        var rawRows = await context.Set<MeasurementPointTimeSeriesEntity>()
            .AsNoTracking()
            .Where(x => x.MeasurementPointId == measurementPointId &&
                        x.PartitionYm >= startYm &&
                        x.PartitionYm <= endYm &&
                        x.RecordedAt >= startTime &&
                        x.RecordedAt <= endTime)
            .OrderBy(x => x.RecordedAt)
            .Select(x => new
            {
                x.Id,
                x.RecordedAt,
                x.DienApPhaA,
                x.DienApPhaB,
                x.DienApPhaC,
                x.DongDienPhaA,
                x.DongDienPhaB,
                x.DongDienPhaC,
                x.CongSuatTacDungTong3Pha,
                x.CongSuatPhanKhangTong3Pha,
                x.HeSoCongSuatPFTrungBinh3Pha,
                x.TanSoHeThong
            })
            .ToListAsync(cancellationToken);

        if (rawRows.Count == 0)
        {
            return new List<MeasurementPointHalfHourOperatingReportEntity>();
        }

        var grouped = rawRows
            .GroupBy(x =>
            {
                var t = x.RecordedAt;
                var min = t.Minute >= 30 ? 30 : 0;
                return new DateTimeOffset(t.Year, t.Month, t.Day, t.Hour, min, 0, t.Offset);
            })
            .OrderBy(g => g.Key);

        var result = new List<MeasurementPointHalfHourOperatingReportEntity>();
        foreach (var g in grouped)
        {
            static double? Avg(IEnumerable<decimal?> vals, int decimals = 2)
            {
                var valid = vals.Where(v => v.HasValue).Select(v => (double)v!.Value).ToList();
                return valid.Count > 0 ? Math.Round(valid.Average(), decimals) : null;
            }

            var p = Avg(g.Select(x => x.CongSuatTacDungTong3Pha));
            var q = Avg(g.Select(x => x.CongSuatPhanKhangTong3Pha));
            var pf = Avg(g.Select(x => x.HeSoCongSuatPFTrungBinh3Pha), 3);
            if (!pf.HasValue && p.HasValue && q.HasValue)
            {
                var s = Math.Sqrt(p.Value * p.Value + q.Value * q.Value);
                if (s > 0) pf = Math.Round(Math.Abs(p.Value) / s, 3);
            }

            result.Add(new MeasurementPointHalfHourOperatingReportEntity
            {
                Id = g.First().Id,
                MeasurementPointId = measurementPointId,
                MeasurementPoint = point,
                CreatedTime = g.Key,
                DienApPhaA = Avg(g.Select(x => x.DienApPhaA)),
                DienApPhaB = Avg(g.Select(x => x.DienApPhaB)),
                DienApPhaC = Avg(g.Select(x => x.DienApPhaC)),
                DongDienPhaA = Avg(g.Select(x => x.DongDienPhaA)),
                DongDienPhaB = Avg(g.Select(x => x.DongDienPhaB)),
                DongDienPhaC = Avg(g.Select(x => x.DongDienPhaC)),
                CongSuatTacDungTong3Pha = p,
                CongSuatPhanKhangTong3Pha = q,
                HeSoCongSuatPF = pf,
                TanSoHeThong = Avg(g.Select(x => x.TanSoHeThong)) ?? 50.0
            });
        }

        return result;
    }
}
