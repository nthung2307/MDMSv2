using Application.Services;
using Domain.Entities;
using Nobisoft.Core.Constants;

namespace Infrastructure.Helpers;

internal enum SnapshotFieldKind
{
    NumericMetric,
    StatusBool,
    DateTimePart
}

internal sealed record SnapshotFieldSpec(
    string Description,
    SnapshotFieldKind Kind,
    string ValueColumn,
    string? UnitColumn,
    string QualityColumn,
    string? UpdatedAtColumn,
    string? TimeSeriesColumn,
    Func<MeasurementPointCurrentSnapshotEntity, object?> GetValue,
    Func<MeasurementPointCurrentSnapshotEntity, string?>? GetUnit,
    Func<MeasurementPointCurrentSnapshotEntity, bool?> GetQuality,
    Func<MeasurementPointCurrentSnapshotEntity, DateTimeOffset?>? GetUpdatedAt,
    Action<MeasurementPointCurrentSnapshotEntity, TelemetryTagSample> ApplyToSnapshot,
    Action<MeasurementPointTimeSeriesEntity, TelemetryTagSample>? ApplyToTimeSeries
);

/// <summary>
/// Single compile-time registry mapping canonical parameter descriptions to Snapshot and TimeSeries columns.
/// Guarantees parameterized SQL builders only emit fixed compile-time column identifiers.
/// </summary>
internal static class TelemetryMetadataRegistry
{
    public static readonly IReadOnlyList<SnapshotFieldSpec> AllSpecs = BuildSpecs();

    public static readonly IReadOnlyDictionary<string, SnapshotFieldSpec> ByDescription =
        AllSpecs.ToDictionary(s => s.Description, s => s, StringComparer.OrdinalIgnoreCase);

    private static SnapshotFieldSpec Num(
        string desc,
        string valCol,
        string? unitCol,
        string qCol,
        string updCol,
        string tsCol,
        Func<MeasurementPointCurrentSnapshotEntity, decimal?> getVal,
        Action<MeasurementPointCurrentSnapshotEntity, decimal?> setVal,
        Func<MeasurementPointCurrentSnapshotEntity, string?>? getUnit,
        Action<MeasurementPointCurrentSnapshotEntity, string?>? setUnit,
        Func<MeasurementPointCurrentSnapshotEntity, bool?> getQ,
        Action<MeasurementPointCurrentSnapshotEntity, bool?> setQ,
        Func<MeasurementPointCurrentSnapshotEntity, DateTimeOffset?> getUpd,
        Action<MeasurementPointCurrentSnapshotEntity, DateTimeOffset?> setUpd,
        Action<MeasurementPointTimeSeriesEntity, decimal?> setTs)
    {
        return new SnapshotFieldSpec(
            desc,
            SnapshotFieldKind.NumericMetric,
            valCol,
            unitCol,
            qCol,
            updCol,
            tsCol,
            s => getVal(s),
            getUnit,
            getQ,
            getUpd,
            (s, sample) =>
            {
                setVal(s, sample.NumericValue);
                if (setUnit != null && !string.IsNullOrWhiteSpace(sample.Unit)) setUnit(s, sample.Unit);
                setQ(s, sample.Quality);
                setUpd(s, sample.Timestamp);
            },
            (ts, sample) => setTs(ts, sample.NumericValue)
        );
    }

    private static SnapshotFieldSpec BoolFlag(
        string desc,
        string valCol,
        string qCol,
        string updCol,
        Func<MeasurementPointCurrentSnapshotEntity, bool?> getVal,
        Action<MeasurementPointCurrentSnapshotEntity, bool?> setVal,
        Func<MeasurementPointCurrentSnapshotEntity, bool?> getQ,
        Action<MeasurementPointCurrentSnapshotEntity, bool?> setQ,
        Func<MeasurementPointCurrentSnapshotEntity, DateTimeOffset?> getUpd,
        Action<MeasurementPointCurrentSnapshotEntity, DateTimeOffset?> setUpd)
    {
        return new SnapshotFieldSpec(
            desc,
            SnapshotFieldKind.StatusBool,
            valCol,
            null,
            qCol,
            updCol,
            null,
            s => getVal(s),
            null,
            getQ,
            getUpd,
            (s, sample) =>
            {
                setVal(s, sample.BoolValue);
                setQ(s, sample.Quality);
                setUpd(s, sample.Timestamp);
            },
            null
        );
    }

    private static SnapshotFieldSpec DatePart(
        string desc,
        string valCol,
        string qCol,
        Func<MeasurementPointCurrentSnapshotEntity, short?> getVal,
        Action<MeasurementPointCurrentSnapshotEntity, short?> setVal,
        Func<MeasurementPointCurrentSnapshotEntity, bool?> getQ,
        Action<MeasurementPointCurrentSnapshotEntity, bool?> setQ)
    {
        return new SnapshotFieldSpec(
            desc,
            SnapshotFieldKind.DateTimePart,
            valCol,
            null,
            qCol,
            null,
            null,
            s => getVal(s),
            null,
            getQ,
            null,
            (s, sample) =>
            {
                setVal(s, sample.SmallIntValue);
                setQ(s, sample.Quality);
            },
            null
        );
    }

    private static IReadOnlyList<SnapshotFieldSpec> BuildSpecs()
    {
        return new List<SnapshotFieldSpec>
        {
            // Currents
            Num(ListDataMeasurementPoint.DongDienPhaA, nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaA), nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaAUnit), nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaAQuality), nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaAUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.DongDienPhaA),
                s => s.DongDienPhaA, (s, v) => s.DongDienPhaA = v, s => s.DongDienPhaAUnit, (s, u) => s.DongDienPhaAUnit = u, s => s.DongDienPhaAQuality, (s, q) => s.DongDienPhaAQuality = q, s => s.DongDienPhaAUpdatedAt, (s, t) => s.DongDienPhaAUpdatedAt = t, (ts, v) => ts.DongDienPhaA = v),
            Num(ListDataMeasurementPoint.DongDienPhaB, nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaB), nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaBUnit), nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaBQuality), nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaBUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.DongDienPhaB),
                s => s.DongDienPhaB, (s, v) => s.DongDienPhaB = v, s => s.DongDienPhaBUnit, (s, u) => s.DongDienPhaBUnit = u, s => s.DongDienPhaBQuality, (s, q) => s.DongDienPhaBQuality = q, s => s.DongDienPhaBUpdatedAt, (s, t) => s.DongDienPhaBUpdatedAt = t, (ts, v) => ts.DongDienPhaB = v),
            Num(ListDataMeasurementPoint.DongDienPhaC, nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaC), nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaCUnit), nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaCQuality), nameof(MeasurementPointCurrentSnapshotEntity.DongDienPhaCUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.DongDienPhaC),
                s => s.DongDienPhaC, (s, v) => s.DongDienPhaC = v, s => s.DongDienPhaCUnit, (s, u) => s.DongDienPhaCUnit = u, s => s.DongDienPhaCQuality, (s, q) => s.DongDienPhaCQuality = q, s => s.DongDienPhaCUpdatedAt, (s, t) => s.DongDienPhaCUpdatedAt = t, (ts, v) => ts.DongDienPhaC = v),

            // Voltages
            Num(ListDataMeasurementPoint.DienApPhaA, nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaA), nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaAUnit), nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaAQuality), nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaAUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.DienApPhaA),
                s => s.DienApPhaA, (s, v) => s.DienApPhaA = v, s => s.DienApPhaAUnit, (s, u) => s.DienApPhaAUnit = u, s => s.DienApPhaAQuality, (s, q) => s.DienApPhaAQuality = q, s => s.DienApPhaAUpdatedAt, (s, t) => s.DienApPhaAUpdatedAt = t, (ts, v) => ts.DienApPhaA = v),
            Num(ListDataMeasurementPoint.DienApPhaB, nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaB), nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaBUnit), nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaBQuality), nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaBUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.DienApPhaB),
                s => s.DienApPhaB, (s, v) => s.DienApPhaB = v, s => s.DienApPhaBUnit, (s, u) => s.DienApPhaBUnit = u, s => s.DienApPhaBQuality, (s, q) => s.DienApPhaBQuality = q, s => s.DienApPhaBUpdatedAt, (s, t) => s.DienApPhaBUpdatedAt = t, (ts, v) => ts.DienApPhaB = v),
            Num(ListDataMeasurementPoint.DienApPhaC, nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaC), nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaCUnit), nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaCQuality), nameof(MeasurementPointCurrentSnapshotEntity.DienApPhaCUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.DienApPhaC),
                s => s.DienApPhaC, (s, v) => s.DienApPhaC = v, s => s.DienApPhaCUnit, (s, u) => s.DienApPhaCUnit = u, s => s.DienApPhaCQuality, (s, q) => s.DienApPhaCQuality = q, s => s.DienApPhaCUpdatedAt, (s, t) => s.DienApPhaCUpdatedAt = t, (ts, v) => ts.DienApPhaC = v),

            // Active Power
            Num(ListDataMeasurementPoint.CongSuatTacDungPhaA, nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaA), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaAUnit), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaAQuality), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaAUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.CongSuatTacDungPhaA),
                s => s.CongSuatTacDungPhaA, (s, v) => s.CongSuatTacDungPhaA = v, s => s.CongSuatTacDungPhaAUnit, (s, u) => s.CongSuatTacDungPhaAUnit = u, s => s.CongSuatTacDungPhaAQuality, (s, q) => s.CongSuatTacDungPhaAQuality = q, s => s.CongSuatTacDungPhaAUpdatedAt, (s, t) => s.CongSuatTacDungPhaAUpdatedAt = t, (ts, v) => ts.CongSuatTacDungPhaA = v),
            Num(ListDataMeasurementPoint.CongSuatTacDungPhaB, nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaB), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaBUnit), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaBQuality), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaBUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.CongSuatTacDungPhaB),
                s => s.CongSuatTacDungPhaB, (s, v) => s.CongSuatTacDungPhaB = v, s => s.CongSuatTacDungPhaBUnit, (s, u) => s.CongSuatTacDungPhaBUnit = u, s => s.CongSuatTacDungPhaBQuality, (s, q) => s.CongSuatTacDungPhaBQuality = q, s => s.CongSuatTacDungPhaBUpdatedAt, (s, t) => s.CongSuatTacDungPhaBUpdatedAt = t, (ts, v) => ts.CongSuatTacDungPhaB = v),
            Num(ListDataMeasurementPoint.CongSuatTacDungPhaC, nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaC), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaCUnit), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaCQuality), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungPhaCUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.CongSuatTacDungPhaC),
                s => s.CongSuatTacDungPhaC, (s, v) => s.CongSuatTacDungPhaC = v, s => s.CongSuatTacDungPhaCUnit, (s, u) => s.CongSuatTacDungPhaCUnit = u, s => s.CongSuatTacDungPhaCQuality, (s, q) => s.CongSuatTacDungPhaCQuality = q, s => s.CongSuatTacDungPhaCUpdatedAt, (s, t) => s.CongSuatTacDungPhaCUpdatedAt = t, (ts, v) => ts.CongSuatTacDungPhaC = v),
            Num(ListDataMeasurementPoint.CongSuatTacDungTong3Pha, nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungTong3Pha), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungTong3PhaUnit), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungTong3PhaQuality), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatTacDungTong3PhaUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.CongSuatTacDungTong3Pha),
                s => s.CongSuatTacDungTong3Pha, (s, v) => s.CongSuatTacDungTong3Pha = v, s => s.CongSuatTacDungTong3PhaUnit, (s, u) => s.CongSuatTacDungTong3PhaUnit = u, s => s.CongSuatTacDungTong3PhaQuality, (s, q) => s.CongSuatTacDungTong3PhaQuality = q, s => s.CongSuatTacDungTong3PhaUpdatedAt, (s, t) => s.CongSuatTacDungTong3PhaUpdatedAt = t, (ts, v) => ts.CongSuatTacDungTong3Pha = v),

            // Reactive Power
            Num(ListDataMeasurementPoint.CongSuatPhanKhangPhaA, nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaA), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaAUnit), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaAQuality), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaAUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.CongSuatPhanKhangPhaA),
                s => s.CongSuatPhanKhangPhaA, (s, v) => s.CongSuatPhanKhangPhaA = v, s => s.CongSuatPhanKhangPhaAUnit, (s, u) => s.CongSuatPhanKhangPhaAUnit = u, s => s.CongSuatPhanKhangPhaAQuality, (s, q) => s.CongSuatPhanKhangPhaAQuality = q, s => s.CongSuatPhanKhangPhaAUpdatedAt, (s, t) => s.CongSuatPhanKhangPhaAUpdatedAt = t, (ts, v) => ts.CongSuatPhanKhangPhaA = v),
            Num(ListDataMeasurementPoint.CongSuatPhanKhangPhaB, nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaB), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaBUnit), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaBQuality), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaBUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.CongSuatPhanKhangPhaB),
                s => s.CongSuatPhanKhangPhaB, (s, v) => s.CongSuatPhanKhangPhaB = v, s => s.CongSuatPhanKhangPhaBUnit, (s, u) => s.CongSuatPhanKhangPhaBUnit = u, s => s.CongSuatPhanKhangPhaBQuality, (s, q) => s.CongSuatPhanKhangPhaBQuality = q, s => s.CongSuatPhanKhangPhaBUpdatedAt, (s, t) => s.CongSuatPhanKhangPhaBUpdatedAt = t, (ts, v) => ts.CongSuatPhanKhangPhaB = v),
            Num(ListDataMeasurementPoint.CongSuatPhanKhangPhaC, nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaC), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaCUnit), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaCQuality), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangPhaCUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.CongSuatPhanKhangPhaC),
                s => s.CongSuatPhanKhangPhaC, (s, v) => s.CongSuatPhanKhangPhaC = v, s => s.CongSuatPhanKhangPhaCUnit, (s, u) => s.CongSuatPhanKhangPhaCUnit = u, s => s.CongSuatPhanKhangPhaCQuality, (s, q) => s.CongSuatPhanKhangPhaCQuality = q, s => s.CongSuatPhanKhangPhaCUpdatedAt, (s, t) => s.CongSuatPhanKhangPhaCUpdatedAt = t, (ts, v) => ts.CongSuatPhanKhangPhaC = v),
            Num(ListDataMeasurementPoint.CongSuatPhanKhangTong3Pha, nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangTong3Pha), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangTong3PhaUnit), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangTong3PhaQuality), nameof(MeasurementPointCurrentSnapshotEntity.CongSuatPhanKhangTong3PhaUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.CongSuatPhanKhangTong3Pha),
                s => s.CongSuatPhanKhangTong3Pha, (s, v) => s.CongSuatPhanKhangTong3Pha = v, s => s.CongSuatPhanKhangTong3PhaUnit, (s, u) => s.CongSuatPhanKhangTong3PhaUnit = u, s => s.CongSuatPhanKhangTong3PhaQuality, (s, q) => s.CongSuatPhanKhangTong3PhaQuality = q, s => s.CongSuatPhanKhangTong3PhaUpdatedAt, (s, t) => s.CongSuatPhanKhangTong3PhaUpdatedAt = t, (ts, v) => ts.CongSuatPhanKhangTong3Pha = v),

            // Power Factor & Frequency
            Num(ListDataMeasurementPoint.HeSoCongSuatPFPhaA, nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFPhaA), null, nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFPhaAQuality), nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFPhaAUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.HeSoCongSuatPFPhaA),
                s => s.HeSoCongSuatPFPhaA, (s, v) => s.HeSoCongSuatPFPhaA = v, null, null, s => s.HeSoCongSuatPFPhaAQuality, (s, q) => s.HeSoCongSuatPFPhaAQuality = q, s => s.HeSoCongSuatPFPhaAUpdatedAt, (s, t) => s.HeSoCongSuatPFPhaAUpdatedAt = t, (ts, v) => ts.HeSoCongSuatPFPhaA = v),
            Num(ListDataMeasurementPoint.HeSoCongSuatPFPhaB, nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFPhaB), null, nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFPhaBQuality), nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFPhaBUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.HeSoCongSuatPFPhaB),
                s => s.HeSoCongSuatPFPhaB, (s, v) => s.HeSoCongSuatPFPhaB = v, null, null, s => s.HeSoCongSuatPFPhaBQuality, (s, q) => s.HeSoCongSuatPFPhaBQuality = q, s => s.HeSoCongSuatPFPhaBUpdatedAt, (s, t) => s.HeSoCongSuatPFPhaBUpdatedAt = t, (ts, v) => ts.HeSoCongSuatPFPhaB = v),
            Num(ListDataMeasurementPoint.HeSoCongSuatPFPhaC, nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFPhaC), null, nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFPhaCQuality), nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFPhaCUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.HeSoCongSuatPFPhaC),
                s => s.HeSoCongSuatPFPhaC, (s, v) => s.HeSoCongSuatPFPhaC = v, null, null, s => s.HeSoCongSuatPFPhaCQuality, (s, q) => s.HeSoCongSuatPFPhaCQuality = q, s => s.HeSoCongSuatPFPhaCUpdatedAt, (s, t) => s.HeSoCongSuatPFPhaCUpdatedAt = t, (ts, v) => ts.HeSoCongSuatPFPhaC = v),
            Num(ListDataMeasurementPoint.HeSoCongSuatPFTrungBinh3Pha, nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFTrungBinh3Pha), null, nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFTrungBinh3PhaQuality), nameof(MeasurementPointCurrentSnapshotEntity.HeSoCongSuatPFTrungBinh3PhaUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.HeSoCongSuatPFTrungBinh3Pha),
                s => s.HeSoCongSuatPFTrungBinh3Pha, (s, v) => s.HeSoCongSuatPFTrungBinh3Pha = v, null, null, s => s.HeSoCongSuatPFTrungBinh3PhaQuality, (s, q) => s.HeSoCongSuatPFTrungBinh3PhaQuality = q, s => s.HeSoCongSuatPFTrungBinh3PhaUpdatedAt, (s, t) => s.HeSoCongSuatPFTrungBinh3PhaUpdatedAt = t, (ts, v) => ts.HeSoCongSuatPFTrungBinh3Pha = v),
            Num(ListDataMeasurementPoint.TanSoHeThong, nameof(MeasurementPointCurrentSnapshotEntity.TanSoHeThong), nameof(MeasurementPointCurrentSnapshotEntity.TanSoHeThongUnit), nameof(MeasurementPointCurrentSnapshotEntity.TanSoHeThongQuality), nameof(MeasurementPointCurrentSnapshotEntity.TanSoHeThongUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.TanSoHeThong),
                s => s.TanSoHeThong, (s, v) => s.TanSoHeThong = v, s => s.TanSoHeThongUnit, (s, u) => s.TanSoHeThongUnit = u, s => s.TanSoHeThongQuality, (s, q) => s.TanSoHeThongQuality = q, s => s.TanSoHeThongUpdatedAt, (s, t) => s.TanSoHeThongUpdatedAt = t, (ts, v) => ts.TanSoHeThong = v),

            // Cumulative Energy - Export
            Num(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioBT, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBT), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBT),
                s => s.ChiSoTichLuyDienNangTacDungAPBieuGioBT, (s, v) => s.ChiSoTichLuyDienNangTacDungAPBieuGioBT = v, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit, (s, u) => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit = u, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTQuality, (s, q) => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTQuality = q, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTUpdatedAt, (s, t) => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTUpdatedAt = t, (ts, v) => ts.ChiSoTichLuyDienNangTacDungAPBieuGioBT = v),
            Num(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioCD, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCD), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCD),
                s => s.ChiSoTichLuyDienNangTacDungAPBieuGioCD, (s, v) => s.ChiSoTichLuyDienNangTacDungAPBieuGioCD = v, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit, (s, u) => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit = u, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDQuality, (s, q) => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDQuality = q, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDUpdatedAt, (s, t) => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDUpdatedAt = t, (ts, v) => ts.ChiSoTichLuyDienNangTacDungAPBieuGioCD = v),
            Num(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioTD, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTD), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTD),
                s => s.ChiSoTichLuyDienNangTacDungAPBieuGioTD, (s, v) => s.ChiSoTichLuyDienNangTacDungAPBieuGioTD = v, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit, (s, u) => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit = u, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDQuality, (s, q) => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDQuality = q, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDUpdatedAt, (s, t) => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDUpdatedAt = t, (ts, v) => ts.ChiSoTichLuyDienNangTacDungAPBieuGioTD = v),
            Num(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTong, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPTong), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPTongUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPTongQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPTongUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoTichLuyDienNangTacDungAPTong),
                s => s.ChiSoTichLuyDienNangTacDungAPTong, (s, v) => s.ChiSoTichLuyDienNangTacDungAPTong = v, s => s.ChiSoTichLuyDienNangTacDungAPTongUnit, (s, u) => s.ChiSoTichLuyDienNangTacDungAPTongUnit = u, s => s.ChiSoTichLuyDienNangTacDungAPTongQuality, (s, q) => s.ChiSoTichLuyDienNangTacDungAPTongQuality = q, s => s.ChiSoTichLuyDienNangTacDungAPTongUpdatedAt, (s, t) => s.ChiSoTichLuyDienNangTacDungAPTongUpdatedAt = t, (ts, v) => ts.ChiSoTichLuyDienNangTacDungAPTong = v),
            Num(ListDataMeasurementPoint.ChiSoTichLuyDienNangPhanKhangAQTong, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangPhanKhangAQTong), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangPhanKhangAQTongUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangPhanKhangAQTongQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangPhanKhangAQTongUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoTichLuyDienNangPhanKhangAQTong),
                s => s.ChiSoTichLuyDienNangPhanKhangAQTong, (s, v) => s.ChiSoTichLuyDienNangPhanKhangAQTong = v, s => s.ChiSoTichLuyDienNangPhanKhangAQTongUnit, (s, u) => s.ChiSoTichLuyDienNangPhanKhangAQTongUnit = u, s => s.ChiSoTichLuyDienNangPhanKhangAQTongQuality, (s, q) => s.ChiSoTichLuyDienNangPhanKhangAQTongQuality = q, s => s.ChiSoTichLuyDienNangPhanKhangAQTongUpdatedAt, (s, t) => s.ChiSoTichLuyDienNangPhanKhangAQTongUpdatedAt = t, (ts, v) => ts.ChiSoTichLuyDienNangPhanKhangAQTong = v),

            // Cumulative Energy - Import
            Num(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan),
                s => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan, (s, v) => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan = v, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUnit, (s, u) => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUnit = u, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuality, (s, q) => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuality = q, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUpdatedAt, (s, t) => s.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUpdatedAt = t, (ts, v) => ts.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan = v),
            Num(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan),
                s => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan, (s, v) => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan = v, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUnit, (s, u) => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUnit = u, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuality, (s, q) => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuality = q, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUpdatedAt, (s, t) => s.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUpdatedAt = t, (ts, v) => ts.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan = v),
            Num(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan),
                s => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan, (s, v) => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan = v, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUnit, (s, u) => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUnit = u, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuality, (s, q) => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuality = q, s => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUpdatedAt, (s, t) => s.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUpdatedAt = t, (ts, v) => ts.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan = v),
            Num(ListDataMeasurementPoint.ChiSoTichLuyDienNangTacDungAPTongChieuNhan, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPTongChieuNhan), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPTongChieuNhanUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPTongChieuNhanQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangTacDungAPTongChieuNhanUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoTichLuyDienNangTacDungAPTongChieuNhan),
                s => s.ChiSoTichLuyDienNangTacDungAPTongChieuNhan, (s, v) => s.ChiSoTichLuyDienNangTacDungAPTongChieuNhan = v, s => s.ChiSoTichLuyDienNangTacDungAPTongChieuNhanUnit, (s, u) => s.ChiSoTichLuyDienNangTacDungAPTongChieuNhanUnit = u, s => s.ChiSoTichLuyDienNangTacDungAPTongChieuNhanQuality, (s, q) => s.ChiSoTichLuyDienNangTacDungAPTongChieuNhanQuality = q, s => s.ChiSoTichLuyDienNangTacDungAPTongChieuNhanUpdatedAt, (s, t) => s.ChiSoTichLuyDienNangTacDungAPTongChieuNhanUpdatedAt = t, (ts, v) => ts.ChiSoTichLuyDienNangTacDungAPTongChieuNhan = v),
            Num(ListDataMeasurementPoint.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan),
                s => s.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan, (s, v) => s.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan = v, s => s.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUnit, (s, u) => s.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUnit = u, s => s.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuality, (s, q) => s.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuality = q, s => s.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUpdatedAt, (s, t) => s.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUpdatedAt = t, (ts, v) => ts.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan = v),

            // Latch Energy - Export & Import
            Num(ListDataMeasurementPoint.ChiSoChotDienNangAPGioBT, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioBT), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioBTUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioBTQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioBTUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoChotDienNangAPGioBT),
                s => s.ChiSoChotDienNangAPGioBT, (s, v) => s.ChiSoChotDienNangAPGioBT = v, s => s.ChiSoChotDienNangAPGioBTUnit, (s, u) => s.ChiSoChotDienNangAPGioBTUnit = u, s => s.ChiSoChotDienNangAPGioBTQuality, (s, q) => s.ChiSoChotDienNangAPGioBTQuality = q, s => s.ChiSoChotDienNangAPGioBTUpdatedAt, (s, t) => s.ChiSoChotDienNangAPGioBTUpdatedAt = t, (ts, v) => ts.ChiSoChotDienNangAPGioBT = v),
            Num(ListDataMeasurementPoint.ChiSoChotDienNangAPGioCD, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioCD), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioCDUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioCDQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioCDUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoChotDienNangAPGioCD),
                s => s.ChiSoChotDienNangAPGioCD, (s, v) => s.ChiSoChotDienNangAPGioCD = v, s => s.ChiSoChotDienNangAPGioCDUnit, (s, u) => s.ChiSoChotDienNangAPGioCDUnit = u, s => s.ChiSoChotDienNangAPGioCDQuality, (s, q) => s.ChiSoChotDienNangAPGioCDQuality = q, s => s.ChiSoChotDienNangAPGioCDUpdatedAt, (s, t) => s.ChiSoChotDienNangAPGioCDUpdatedAt = t, (ts, v) => ts.ChiSoChotDienNangAPGioCD = v),
            Num(ListDataMeasurementPoint.ChiSoChotDienNangAPGioTD, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioTD), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioTDUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioTDQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioTDUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoChotDienNangAPGioTD),
                s => s.ChiSoChotDienNangAPGioTD, (s, v) => s.ChiSoChotDienNangAPGioTD = v, s => s.ChiSoChotDienNangAPGioTDUnit, (s, u) => s.ChiSoChotDienNangAPGioTDUnit = u, s => s.ChiSoChotDienNangAPGioTDQuality, (s, q) => s.ChiSoChotDienNangAPGioTDQuality = q, s => s.ChiSoChotDienNangAPGioTDUpdatedAt, (s, t) => s.ChiSoChotDienNangAPGioTDUpdatedAt = t, (ts, v) => ts.ChiSoChotDienNangAPGioTD = v),
            Num(ListDataMeasurementPoint.ChiSoChotDienNangAPTong, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPTong), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPTongUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPTongQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPTongUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoChotDienNangAPTong),
                s => s.ChiSoChotDienNangAPTong, (s, v) => s.ChiSoChotDienNangAPTong = v, s => s.ChiSoChotDienNangAPTongUnit, (s, u) => s.ChiSoChotDienNangAPTongUnit = u, s => s.ChiSoChotDienNangAPTongQuality, (s, q) => s.ChiSoChotDienNangAPTongQuality = q, s => s.ChiSoChotDienNangAPTongUpdatedAt, (s, t) => s.ChiSoChotDienNangAPTongUpdatedAt = t, (ts, v) => ts.ChiSoChotDienNangAPTong = v),
            Num(ListDataMeasurementPoint.ChiSoChotDienNangAQTong, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAQTong), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAQTongUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAQTongQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAQTongUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoChotDienNangAQTong),
                s => s.ChiSoChotDienNangAQTong, (s, v) => s.ChiSoChotDienNangAQTong = v, s => s.ChiSoChotDienNangAQTongUnit, (s, u) => s.ChiSoChotDienNangAQTongUnit = u, s => s.ChiSoChotDienNangAQTongQuality, (s, q) => s.ChiSoChotDienNangAQTongQuality = q, s => s.ChiSoChotDienNangAQTongUpdatedAt, (s, t) => s.ChiSoChotDienNangAQTongUpdatedAt = t, (ts, v) => ts.ChiSoChotDienNangAQTong = v),

            Num(ListDataMeasurementPoint.ChiSoChotDienNangAPGioBTChieuNhan, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioBTChieuNhan), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioBTChieuNhanUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioBTChieuNhanQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioBTChieuNhanUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoChotDienNangAPGioBTChieuNhan),
                s => s.ChiSoChotDienNangAPGioBTChieuNhan, (s, v) => s.ChiSoChotDienNangAPGioBTChieuNhan = v, s => s.ChiSoChotDienNangAPGioBTChieuNhanUnit, (s, u) => s.ChiSoChotDienNangAPGioBTChieuNhanUnit = u, s => s.ChiSoChotDienNangAPGioBTChieuNhanQuality, (s, q) => s.ChiSoChotDienNangAPGioBTChieuNhanQuality = q, s => s.ChiSoChotDienNangAPGioBTChieuNhanUpdatedAt, (s, t) => s.ChiSoChotDienNangAPGioBTChieuNhanUpdatedAt = t, (ts, v) => ts.ChiSoChotDienNangAPGioBTChieuNhan = v),
            Num(ListDataMeasurementPoint.ChiSoChotDienNangAPGioCDChieuNhan, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioCDChieuNhan), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioCDChieuNhanUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioCDChieuNhanQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioCDChieuNhanUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoChotDienNangAPGioCDChieuNhan),
                s => s.ChiSoChotDienNangAPGioCDChieuNhan, (s, v) => s.ChiSoChotDienNangAPGioCDChieuNhan = v, s => s.ChiSoChotDienNangAPGioCDChieuNhanUnit, (s, u) => s.ChiSoChotDienNangAPGioCDChieuNhanUnit = u, s => s.ChiSoChotDienNangAPGioCDChieuNhanQuality, (s, q) => s.ChiSoChotDienNangAPGioCDChieuNhanQuality = q, s => s.ChiSoChotDienNangAPGioCDChieuNhanUpdatedAt, (s, t) => s.ChiSoChotDienNangAPGioCDChieuNhanUpdatedAt = t, (ts, v) => ts.ChiSoChotDienNangAPGioCDChieuNhan = v),
            Num(ListDataMeasurementPoint.ChiSoChotDienNangAPGioTDChieuNhan, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioTDChieuNhan), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioTDChieuNhanUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioTDChieuNhanQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPGioTDChieuNhanUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoChotDienNangAPGioTDChieuNhan),
                s => s.ChiSoChotDienNangAPGioTDChieuNhan, (s, v) => s.ChiSoChotDienNangAPGioTDChieuNhan = v, s => s.ChiSoChotDienNangAPGioTDChieuNhanUnit, (s, u) => s.ChiSoChotDienNangAPGioTDChieuNhanUnit = u, s => s.ChiSoChotDienNangAPGioTDChieuNhanQuality, (s, q) => s.ChiSoChotDienNangAPGioTDChieuNhanQuality = q, s => s.ChiSoChotDienNangAPGioTDChieuNhanUpdatedAt, (s, t) => s.ChiSoChotDienNangAPGioTDChieuNhanUpdatedAt = t, (ts, v) => ts.ChiSoChotDienNangAPGioTDChieuNhan = v),
            Num(ListDataMeasurementPoint.ChiSoChotDienNangAPTongChieuNhan, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPTongChieuNhan), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPTongChieuNhanUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPTongChieuNhanQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAPTongChieuNhanUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoChotDienNangAPTongChieuNhan),
                s => s.ChiSoChotDienNangAPTongChieuNhan, (s, v) => s.ChiSoChotDienNangAPTongChieuNhan = v, s => s.ChiSoChotDienNangAPTongChieuNhanUnit, (s, u) => s.ChiSoChotDienNangAPTongChieuNhanUnit = u, s => s.ChiSoChotDienNangAPTongChieuNhanQuality, (s, q) => s.ChiSoChotDienNangAPTongChieuNhanQuality = q, s => s.ChiSoChotDienNangAPTongChieuNhanUpdatedAt, (s, t) => s.ChiSoChotDienNangAPTongChieuNhanUpdatedAt = t, (ts, v) => ts.ChiSoChotDienNangAPTongChieuNhan = v),
            Num(ListDataMeasurementPoint.ChiSoChotDienNangAQTongChieuNhan, nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAQTongChieuNhan), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAQTongChieuNhanUnit), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAQTongChieuNhanQuality), nameof(MeasurementPointCurrentSnapshotEntity.ChiSoChotDienNangAQTongChieuNhanUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.ChiSoChotDienNangAQTongChieuNhan),
                s => s.ChiSoChotDienNangAQTongChieuNhan, (s, v) => s.ChiSoChotDienNangAQTongChieuNhan = v, s => s.ChiSoChotDienNangAQTongChieuNhanUnit, (s, u) => s.ChiSoChotDienNangAQTongChieuNhanUnit = u, s => s.ChiSoChotDienNangAQTongChieuNhanQuality, (s, q) => s.ChiSoChotDienNangAQTongChieuNhanQuality = q, s => s.ChiSoChotDienNangAQTongChieuNhanUpdatedAt, (s, t) => s.ChiSoChotDienNangAQTongChieuNhanUpdatedAt = t, (ts, v) => ts.ChiSoChotDienNangAQTongChieuNhan = v),

            // Pmax
            Num(ListDataMeasurementPoint.PmaxKyHienTai, nameof(MeasurementPointCurrentSnapshotEntity.PmaxKyHienTai), nameof(MeasurementPointCurrentSnapshotEntity.PmaxKyHienTaiUnit), nameof(MeasurementPointCurrentSnapshotEntity.PmaxKyHienTaiQuality), nameof(MeasurementPointCurrentSnapshotEntity.PmaxKyHienTaiUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.PmaxKyHienTai),
                s => s.PmaxKyHienTai, (s, v) => s.PmaxKyHienTai = v, s => s.PmaxKyHienTaiUnit, (s, u) => s.PmaxKyHienTaiUnit = u, s => s.PmaxKyHienTaiQuality, (s, q) => s.PmaxKyHienTaiQuality = q, s => s.PmaxKyHienTaiUpdatedAt, (s, t) => s.PmaxKyHienTaiUpdatedAt = t, (ts, v) => ts.PmaxKyHienTai = v),
            Num(ListDataMeasurementPoint.PmaxKyTruoc, nameof(MeasurementPointCurrentSnapshotEntity.PmaxKyTruoc), nameof(MeasurementPointCurrentSnapshotEntity.PmaxKyTruocUnit), nameof(MeasurementPointCurrentSnapshotEntity.PmaxKyTruocQuality), nameof(MeasurementPointCurrentSnapshotEntity.PmaxKyTruocUpdatedAt), nameof(MeasurementPointTimeSeriesEntity.PmaxKyTruoc),
                s => s.PmaxKyTruoc, (s, v) => s.PmaxKyTruoc = v, s => s.PmaxKyTruocUnit, (s, u) => s.PmaxKyTruocUnit = u, s => s.PmaxKyTruocQuality, (s, q) => s.PmaxKyTruocQuality = q, s => s.PmaxKyTruocUpdatedAt, (s, t) => s.PmaxKyTruocUpdatedAt = t, (ts, v) => ts.PmaxKyTruoc = v),

            // Status & Warnings
            BoolFlag(ListDataMeasurementPoint.MatKetNoi, nameof(MeasurementPointCurrentSnapshotEntity.MatKetNoi), nameof(MeasurementPointCurrentSnapshotEntity.MatKetNoiQuality), nameof(MeasurementPointCurrentSnapshotEntity.MatKetNoiUpdatedAt),
                s => s.MatKetNoi, (s, v) => s.MatKetNoi = v, s => s.MatKetNoiQuality, (s, q) => s.MatKetNoiQuality = q, s => s.MatKetNoiUpdatedAt, (s, t) => s.MatKetNoiUpdatedAt = t),
            BoolFlag(ListDataMeasurementPoint.CanhBao01, nameof(MeasurementPointCurrentSnapshotEntity.CanhBao01), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao01Quality), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao01UpdatedAt),
                s => s.CanhBao01, (s, v) => s.CanhBao01 = v, s => s.CanhBao01Quality, (s, q) => s.CanhBao01Quality = q, s => s.CanhBao01UpdatedAt, (s, t) => s.CanhBao01UpdatedAt = t),
            BoolFlag(ListDataMeasurementPoint.CanhBao02, nameof(MeasurementPointCurrentSnapshotEntity.CanhBao02), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao02Quality), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao02UpdatedAt),
                s => s.CanhBao02, (s, v) => s.CanhBao02 = v, s => s.CanhBao02Quality, (s, q) => s.CanhBao02Quality = q, s => s.CanhBao02UpdatedAt, (s, t) => s.CanhBao02UpdatedAt = t),
            BoolFlag(ListDataMeasurementPoint.CanhBao03, nameof(MeasurementPointCurrentSnapshotEntity.CanhBao03), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao03Quality), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao03UpdatedAt),
                s => s.CanhBao03, (s, v) => s.CanhBao03 = v, s => s.CanhBao03Quality, (s, q) => s.CanhBao03Quality = q, s => s.CanhBao03UpdatedAt, (s, t) => s.CanhBao03UpdatedAt = t),
            BoolFlag(ListDataMeasurementPoint.CanhBao04, nameof(MeasurementPointCurrentSnapshotEntity.CanhBao04), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao04Quality), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao04UpdatedAt),
                s => s.CanhBao04, (s, v) => s.CanhBao04 = v, s => s.CanhBao04Quality, (s, q) => s.CanhBao04Quality = q, s => s.CanhBao04UpdatedAt, (s, t) => s.CanhBao04UpdatedAt = t),
            BoolFlag(ListDataMeasurementPoint.CanhBao05, nameof(MeasurementPointCurrentSnapshotEntity.CanhBao05), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao05Quality), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao05UpdatedAt),
                s => s.CanhBao05, (s, v) => s.CanhBao05 = v, s => s.CanhBao05Quality, (s, q) => s.CanhBao05Quality = q, s => s.CanhBao05UpdatedAt, (s, t) => s.CanhBao05UpdatedAt = t),
            BoolFlag(ListDataMeasurementPoint.CanhBao06, nameof(MeasurementPointCurrentSnapshotEntity.CanhBao06), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao06Quality), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao06UpdatedAt),
                s => s.CanhBao06, (s, v) => s.CanhBao06 = v, s => s.CanhBao06Quality, (s, q) => s.CanhBao06Quality = q, s => s.CanhBao06UpdatedAt, (s, t) => s.CanhBao06UpdatedAt = t),
            BoolFlag(ListDataMeasurementPoint.CanhBao07, nameof(MeasurementPointCurrentSnapshotEntity.CanhBao07), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao07Quality), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao07UpdatedAt),
                s => s.CanhBao07, (s, v) => s.CanhBao07 = v, s => s.CanhBao07Quality, (s, q) => s.CanhBao07Quality = q, s => s.CanhBao07UpdatedAt, (s, t) => s.CanhBao07UpdatedAt = t),
            BoolFlag(ListDataMeasurementPoint.CanhBao08, nameof(MeasurementPointCurrentSnapshotEntity.CanhBao08), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao08Quality), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao08UpdatedAt),
                s => s.CanhBao08, (s, v) => s.CanhBao08 = v, s => s.CanhBao08Quality, (s, q) => s.CanhBao08Quality = q, s => s.CanhBao08UpdatedAt, (s, t) => s.CanhBao08UpdatedAt = t),
            BoolFlag(ListDataMeasurementPoint.CanhBao09, nameof(MeasurementPointCurrentSnapshotEntity.CanhBao09), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao09Quality), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao09UpdatedAt),
                s => s.CanhBao09, (s, v) => s.CanhBao09 = v, s => s.CanhBao09Quality, (s, q) => s.CanhBao09Quality = q, s => s.CanhBao09UpdatedAt, (s, t) => s.CanhBao09UpdatedAt = t),
            BoolFlag(ListDataMeasurementPoint.CanhBao10, nameof(MeasurementPointCurrentSnapshotEntity.CanhBao10), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao10Quality), nameof(MeasurementPointCurrentSnapshotEntity.CanhBao10UpdatedAt),
                s => s.CanhBao10, (s, v) => s.CanhBao10 = v, s => s.CanhBao10Quality, (s, q) => s.CanhBao10Quality = q, s => s.CanhBao10UpdatedAt, (s, t) => s.CanhBao10UpdatedAt = t),

            // DateTime Parts - Meter Clock
            DatePart(ListDataMeasurementPoint.NamHienTai, nameof(MeasurementPointCurrentSnapshotEntity.MeterYear), nameof(MeasurementPointCurrentSnapshotEntity.MeterYearQuality), s => s.MeterYear, (s, v) => s.MeterYear = v, s => s.MeterYearQuality, (s, q) => s.MeterYearQuality = q),
            DatePart(ListDataMeasurementPoint.ThangHienTai, nameof(MeasurementPointCurrentSnapshotEntity.MeterMonth), nameof(MeasurementPointCurrentSnapshotEntity.MeterMonthQuality), s => s.MeterMonth, (s, v) => s.MeterMonth = v, s => s.MeterMonthQuality, (s, q) => s.MeterMonthQuality = q),
            DatePart(ListDataMeasurementPoint.NgayHienTai, nameof(MeasurementPointCurrentSnapshotEntity.MeterDay), nameof(MeasurementPointCurrentSnapshotEntity.MeterDayQuality), s => s.MeterDay, (s, v) => s.MeterDay = v, s => s.MeterDayQuality, (s, q) => s.MeterDayQuality = q),
            DatePart(ListDataMeasurementPoint.GioHienTai, nameof(MeasurementPointCurrentSnapshotEntity.MeterHour), nameof(MeasurementPointCurrentSnapshotEntity.MeterHourQuality), s => s.MeterHour, (s, v) => s.MeterHour = v, s => s.MeterHourQuality, (s, q) => s.MeterHourQuality = q),
            DatePart(ListDataMeasurementPoint.PhutHienTai, nameof(MeasurementPointCurrentSnapshotEntity.MeterMinute), nameof(MeasurementPointCurrentSnapshotEntity.MeterMinuteQuality), s => s.MeterMinute, (s, v) => s.MeterMinute = v, s => s.MeterMinuteQuality, (s, q) => s.MeterMinuteQuality = q),
            DatePart(ListDataMeasurementPoint.GiayHienTai, nameof(MeasurementPointCurrentSnapshotEntity.MeterSecond), nameof(MeasurementPointCurrentSnapshotEntity.MeterSecondQuality), s => s.MeterSecond, (s, v) => s.MeterSecond = v, s => s.MeterSecondQuality, (s, q) => s.MeterSecondQuality = q),

            // DateTime Parts - Latest Closing
            DatePart(ListDataMeasurementPoint.NamChotGanNhat, nameof(MeasurementPointCurrentSnapshotEntity.ClosingYear), nameof(MeasurementPointCurrentSnapshotEntity.ClosingYearQuality), s => s.ClosingYear, (s, v) => s.ClosingYear = v, s => s.ClosingYearQuality, (s, q) => s.ClosingYearQuality = q),
            DatePart(ListDataMeasurementPoint.ThangChotGanNhat, nameof(MeasurementPointCurrentSnapshotEntity.ClosingMonth), nameof(MeasurementPointCurrentSnapshotEntity.ClosingMonthQuality), s => s.ClosingMonth, (s, v) => s.ClosingMonth = v, s => s.ClosingMonthQuality, (s, q) => s.ClosingMonthQuality = q),
            DatePart(ListDataMeasurementPoint.NgayChotGanNhat, nameof(MeasurementPointCurrentSnapshotEntity.ClosingDay), nameof(MeasurementPointCurrentSnapshotEntity.ClosingDayQuality), s => s.ClosingDay, (s, v) => s.ClosingDay = v, s => s.ClosingDayQuality, (s, q) => s.ClosingDayQuality = q),
            DatePart(ListDataMeasurementPoint.GioChotGanNhat, nameof(MeasurementPointCurrentSnapshotEntity.ClosingHour), nameof(MeasurementPointCurrentSnapshotEntity.ClosingHourQuality), s => s.ClosingHour, (s, v) => s.ClosingHour = v, s => s.ClosingHourQuality, (s, q) => s.ClosingHourQuality = q),
            DatePart(ListDataMeasurementPoint.PhutChotGanNhat, nameof(MeasurementPointCurrentSnapshotEntity.ClosingMinute), nameof(MeasurementPointCurrentSnapshotEntity.ClosingMinuteQuality), s => s.ClosingMinute, (s, v) => s.ClosingMinute = v, s => s.ClosingMinuteQuality, (s, q) => s.ClosingMinuteQuality = q),
            DatePart(ListDataMeasurementPoint.GiayChotGanNhat, nameof(MeasurementPointCurrentSnapshotEntity.ClosingSecond), nameof(MeasurementPointCurrentSnapshotEntity.ClosingSecondQuality), s => s.ClosingSecond, (s, v) => s.ClosingSecond = v, s => s.ClosingSecondQuality, (s, q) => s.ClosingSecondQuality = q),

            // DateTime Parts - Pmax Current
            DatePart(ListDataMeasurementPoint.NamPmaxKyHienTai, nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentYear), nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentYearQuality), s => s.PmaxCurrentYear, (s, v) => s.PmaxCurrentYear = v, s => s.PmaxCurrentYearQuality, (s, q) => s.PmaxCurrentYearQuality = q),
            DatePart(ListDataMeasurementPoint.ThangPmaxKyHienTai, nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentMonth), nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentMonthQuality), s => s.PmaxCurrentMonth, (s, v) => s.PmaxCurrentMonth = v, s => s.PmaxCurrentMonthQuality, (s, q) => s.PmaxCurrentMonthQuality = q),
            DatePart(ListDataMeasurementPoint.NgayPmaxKyHienTai, nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentDay), nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentDayQuality), s => s.PmaxCurrentDay, (s, v) => s.PmaxCurrentDay = v, s => s.PmaxCurrentDayQuality, (s, q) => s.PmaxCurrentDayQuality = q),
            DatePart(ListDataMeasurementPoint.GioPmaxKyHienTai, nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentHour), nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentHourQuality), s => s.PmaxCurrentHour, (s, v) => s.PmaxCurrentHour = v, s => s.PmaxCurrentHourQuality, (s, q) => s.PmaxCurrentHourQuality = q),
            DatePart(ListDataMeasurementPoint.PhutPmaxKyHienTai, nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentMinute), nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentMinuteQuality), s => s.PmaxCurrentMinute, (s, v) => s.PmaxCurrentMinute = v, s => s.PmaxCurrentMinuteQuality, (s, q) => s.PmaxCurrentMinuteQuality = q),
            DatePart(ListDataMeasurementPoint.GiayPmaxKyHienTai, nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentSecond), nameof(MeasurementPointCurrentSnapshotEntity.PmaxCurrentSecondQuality), s => s.PmaxCurrentSecond, (s, v) => s.PmaxCurrentSecond = v, s => s.PmaxCurrentSecondQuality, (s, q) => s.PmaxCurrentSecondQuality = q),

            // DateTime Parts - Pmax Previous
            DatePart(ListDataMeasurementPoint.NamPmaxKyTruoc, nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevYear), nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevYearQuality), s => s.PmaxPrevYear, (s, v) => s.PmaxPrevYear = v, s => s.PmaxPrevYearQuality, (s, q) => s.PmaxPrevYearQuality = q),
            DatePart(ListDataMeasurementPoint.ThangPmaxKyTruoc, nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevMonth), nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevMonthQuality), s => s.PmaxPrevMonth, (s, v) => s.PmaxPrevMonth = v, s => s.PmaxPrevMonthQuality, (s, q) => s.PmaxPrevMonthQuality = q),
            DatePart(ListDataMeasurementPoint.NgayPmaxKyTruoc, nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevDay), nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevDayQuality), s => s.PmaxPrevDay, (s, v) => s.PmaxPrevDay = v, s => s.PmaxPrevDayQuality, (s, q) => s.PmaxPrevDayQuality = q),
            DatePart(ListDataMeasurementPoint.GioPmaxKyTruoc, nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevHour), nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevHourQuality), s => s.PmaxPrevHour, (s, v) => s.PmaxPrevHour = v, s => s.PmaxPrevHourQuality, (s, q) => s.PmaxPrevHourQuality = q),
            DatePart(ListDataMeasurementPoint.PhutPmaxKyTruoc, nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevMinute), nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevMinuteQuality), s => s.PmaxPrevMinute, (s, v) => s.PmaxPrevMinute = v, s => s.PmaxPrevMinuteQuality, (s, q) => s.PmaxPrevMinuteQuality = q),
            DatePart(ListDataMeasurementPoint.GiayPmaxKyTruoc, nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevSecond), nameof(MeasurementPointCurrentSnapshotEntity.PmaxPrevSecondQuality), s => s.PmaxPrevSecond, (s, v) => s.PmaxPrevSecond = v, s => s.PmaxPrevSecondQuality, (s, q) => s.PmaxPrevSecondQuality = q),
        };
    }
}

