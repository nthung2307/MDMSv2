using System;
using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

/// <summary>
/// Stores the latest known telemetry, status, warning flags, and clock registers per measurement point (1 row/point).
/// </summary>
public class MeasurementPointCurrentSnapshotEntity : IEntity<long>
{
    public long Id { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity? MeasurementPoint { get; set; }

    public DateTimeOffset? LastTelemetryAt { get; set; }
    public DateTimeOffset? LastStatusAt { get; set; }
    public DateTimeOffset? LastDateTimeAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Operational Currents
    public decimal? DongDienPhaA { get; set; }
    public string? DongDienPhaAUnit { get; set; }
    public bool? DongDienPhaAQuality { get; set; }
    public DateTimeOffset? DongDienPhaAUpdatedAt { get; set; }

    public decimal? DongDienPhaB { get; set; }
    public string? DongDienPhaBUnit { get; set; }
    public bool? DongDienPhaBQuality { get; set; }
    public DateTimeOffset? DongDienPhaBUpdatedAt { get; set; }

    public decimal? DongDienPhaC { get; set; }
    public string? DongDienPhaCUnit { get; set; }
    public bool? DongDienPhaCQuality { get; set; }
    public DateTimeOffset? DongDienPhaCUpdatedAt { get; set; }

    // Operational Voltages
    public decimal? DienApPhaA { get; set; }
    public string? DienApPhaAUnit { get; set; }
    public bool? DienApPhaAQuality { get; set; }
    public DateTimeOffset? DienApPhaAUpdatedAt { get; set; }

    public decimal? DienApPhaB { get; set; }
    public string? DienApPhaBUnit { get; set; }
    public bool? DienApPhaBQuality { get; set; }
    public DateTimeOffset? DienApPhaBUpdatedAt { get; set; }

    public decimal? DienApPhaC { get; set; }
    public string? DienApPhaCUnit { get; set; }
    public bool? DienApPhaCQuality { get; set; }
    public DateTimeOffset? DienApPhaCUpdatedAt { get; set; }

    // Active Power
    public decimal? CongSuatTacDungPhaA { get; set; }
    public string? CongSuatTacDungPhaAUnit { get; set; }
    public bool? CongSuatTacDungPhaAQuality { get; set; }
    public DateTimeOffset? CongSuatTacDungPhaAUpdatedAt { get; set; }

    public decimal? CongSuatTacDungPhaB { get; set; }
    public string? CongSuatTacDungPhaBUnit { get; set; }
    public bool? CongSuatTacDungPhaBQuality { get; set; }
    public DateTimeOffset? CongSuatTacDungPhaBUpdatedAt { get; set; }

    public decimal? CongSuatTacDungPhaC { get; set; }
    public string? CongSuatTacDungPhaCUnit { get; set; }
    public bool? CongSuatTacDungPhaCQuality { get; set; }
    public DateTimeOffset? CongSuatTacDungPhaCUpdatedAt { get; set; }

    public decimal? CongSuatTacDungTong3Pha { get; set; }
    public string? CongSuatTacDungTong3PhaUnit { get; set; }
    public bool? CongSuatTacDungTong3PhaQuality { get; set; }
    public DateTimeOffset? CongSuatTacDungTong3PhaUpdatedAt { get; set; }

    // Reactive Power
    public decimal? CongSuatPhanKhangPhaA { get; set; }
    public string? CongSuatPhanKhangPhaAUnit { get; set; }
    public bool? CongSuatPhanKhangPhaAQuality { get; set; }
    public DateTimeOffset? CongSuatPhanKhangPhaAUpdatedAt { get; set; }

    public decimal? CongSuatPhanKhangPhaB { get; set; }
    public string? CongSuatPhanKhangPhaBUnit { get; set; }
    public bool? CongSuatPhanKhangPhaBQuality { get; set; }
    public DateTimeOffset? CongSuatPhanKhangPhaBUpdatedAt { get; set; }

    public decimal? CongSuatPhanKhangPhaC { get; set; }
    public string? CongSuatPhanKhangPhaCUnit { get; set; }
    public bool? CongSuatPhanKhangPhaCQuality { get; set; }
    public DateTimeOffset? CongSuatPhanKhangPhaCUpdatedAt { get; set; }

    public decimal? CongSuatPhanKhangTong3Pha { get; set; }
    public string? CongSuatPhanKhangTong3PhaUnit { get; set; }
    public bool? CongSuatPhanKhangTong3PhaQuality { get; set; }
    public DateTimeOffset? CongSuatPhanKhangTong3PhaUpdatedAt { get; set; }

    // Power Factor
    public decimal? HeSoCongSuatPFPhaA { get; set; }
    public bool? HeSoCongSuatPFPhaAQuality { get; set; }
    public DateTimeOffset? HeSoCongSuatPFPhaAUpdatedAt { get; set; }

    public decimal? HeSoCongSuatPFPhaB { get; set; }
    public bool? HeSoCongSuatPFPhaBQuality { get; set; }
    public DateTimeOffset? HeSoCongSuatPFPhaBUpdatedAt { get; set; }

    public decimal? HeSoCongSuatPFPhaC { get; set; }
    public bool? HeSoCongSuatPFPhaCQuality { get; set; }
    public DateTimeOffset? HeSoCongSuatPFPhaCUpdatedAt { get; set; }

    public decimal? HeSoCongSuatPFTrungBinh3Pha { get; set; }
    public bool? HeSoCongSuatPFTrungBinh3PhaQuality { get; set; }
    public DateTimeOffset? HeSoCongSuatPFTrungBinh3PhaUpdatedAt { get; set; }

    // Frequency
    public decimal? TanSoHeThong { get; set; }
    public string? TanSoHeThongUnit { get; set; }
    public bool? TanSoHeThongQuality { get; set; }
    public DateTimeOffset? TanSoHeThongUpdatedAt { get; set; }

    // Cumulative Energy - Export (Kenh Giao)
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioBT { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioBTUnit { get; set; }
    public bool? ChiSoTichLuyDienNangTacDungAPBieuGioBTQuality { get; set; }
    public DateTimeOffset? ChiSoTichLuyDienNangTacDungAPBieuGioBTUpdatedAt { get; set; }

    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioCD { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioCDUnit { get; set; }
    public bool? ChiSoTichLuyDienNangTacDungAPBieuGioCDQuality { get; set; }
    public DateTimeOffset? ChiSoTichLuyDienNangTacDungAPBieuGioCDUpdatedAt { get; set; }

    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioTD { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioTDUnit { get; set; }
    public bool? ChiSoTichLuyDienNangTacDungAPBieuGioTDQuality { get; set; }
    public DateTimeOffset? ChiSoTichLuyDienNangTacDungAPBieuGioTDUpdatedAt { get; set; }

    public decimal? ChiSoTichLuyDienNangTacDungAPTong { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPTongUnit { get; set; }
    public bool? ChiSoTichLuyDienNangTacDungAPTongQuality { get; set; }
    public DateTimeOffset? ChiSoTichLuyDienNangTacDungAPTongUpdatedAt { get; set; }

    public decimal? ChiSoTichLuyDienNangPhanKhangAQTong { get; set; }
    public string? ChiSoTichLuyDienNangPhanKhangAQTongUnit { get; set; }
    public bool? ChiSoTichLuyDienNangPhanKhangAQTongQuality { get; set; }
    public DateTimeOffset? ChiSoTichLuyDienNangPhanKhangAQTongUpdatedAt { get; set; }

    // Cumulative Energy - Import (Kenh Nhan)
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUnit { get; set; }
    public bool? ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanQuality { get; set; }
    public DateTimeOffset? ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhanUpdatedAt { get; set; }

    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUnit { get; set; }
    public bool? ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanQuality { get; set; }
    public DateTimeOffset? ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhanUpdatedAt { get; set; }

    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUnit { get; set; }
    public bool? ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanQuality { get; set; }
    public DateTimeOffset? ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhanUpdatedAt { get; set; }

    public decimal? ChiSoTichLuyDienNangTacDungAPTongChieuNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPTongChieuNhanUnit { get; set; }
    public bool? ChiSoTichLuyDienNangTacDungAPTongChieuNhanQuality { get; set; }
    public DateTimeOffset? ChiSoTichLuyDienNangTacDungAPTongChieuNhanUpdatedAt { get; set; }

    public decimal? ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan { get; set; }
    public string? ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUnit { get; set; }
    public bool? ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanQuality { get; set; }
    public DateTimeOffset? ChiSoTichLuyDienNangPhanKhangAQTongChieuNhanUpdatedAt { get; set; }

    // Latch Energy - Export (Kenh Giao)
    public decimal? ChiSoChotDienNangAPGioBT { get; set; }
    public string? ChiSoChotDienNangAPGioBTUnit { get; set; }
    public bool? ChiSoChotDienNangAPGioBTQuality { get; set; }
    public DateTimeOffset? ChiSoChotDienNangAPGioBTUpdatedAt { get; set; }

    public decimal? ChiSoChotDienNangAPGioCD { get; set; }
    public string? ChiSoChotDienNangAPGioCDUnit { get; set; }
    public bool? ChiSoChotDienNangAPGioCDQuality { get; set; }
    public DateTimeOffset? ChiSoChotDienNangAPGioCDUpdatedAt { get; set; }

    public decimal? ChiSoChotDienNangAPGioTD { get; set; }
    public string? ChiSoChotDienNangAPGioTDUnit { get; set; }
    public bool? ChiSoChotDienNangAPGioTDQuality { get; set; }
    public DateTimeOffset? ChiSoChotDienNangAPGioTDUpdatedAt { get; set; }

    public decimal? ChiSoChotDienNangAPTong { get; set; }
    public string? ChiSoChotDienNangAPTongUnit { get; set; }
    public bool? ChiSoChotDienNangAPTongQuality { get; set; }
    public DateTimeOffset? ChiSoChotDienNangAPTongUpdatedAt { get; set; }

    public decimal? ChiSoChotDienNangAQTong { get; set; }
    public string? ChiSoChotDienNangAQTongUnit { get; set; }
    public bool? ChiSoChotDienNangAQTongQuality { get; set; }
    public DateTimeOffset? ChiSoChotDienNangAQTongUpdatedAt { get; set; }

    // Latch Energy - Import (Kenh Nhan)
    public decimal? ChiSoChotDienNangAPGioBTChieuNhan { get; set; }
    public string? ChiSoChotDienNangAPGioBTChieuNhanUnit { get; set; }
    public bool? ChiSoChotDienNangAPGioBTChieuNhanQuality { get; set; }
    public DateTimeOffset? ChiSoChotDienNangAPGioBTChieuNhanUpdatedAt { get; set; }

    public decimal? ChiSoChotDienNangAPGioCDChieuNhan { get; set; }
    public string? ChiSoChotDienNangAPGioCDChieuNhanUnit { get; set; }
    public bool? ChiSoChotDienNangAPGioCDChieuNhanQuality { get; set; }
    public DateTimeOffset? ChiSoChotDienNangAPGioCDChieuNhanUpdatedAt { get; set; }

    public decimal? ChiSoChotDienNangAPGioTDChieuNhan { get; set; }
    public string? ChiSoChotDienNangAPGioTDChieuNhanUnit { get; set; }
    public bool? ChiSoChotDienNangAPGioTDChieuNhanQuality { get; set; }
    public DateTimeOffset? ChiSoChotDienNangAPGioTDChieuNhanUpdatedAt { get; set; }

    public decimal? ChiSoChotDienNangAPTongChieuNhan { get; set; }
    public string? ChiSoChotDienNangAPTongChieuNhanUnit { get; set; }
    public bool? ChiSoChotDienNangAPTongChieuNhanQuality { get; set; }
    public DateTimeOffset? ChiSoChotDienNangAPTongChieuNhanUpdatedAt { get; set; }

    public decimal? ChiSoChotDienNangAQTongChieuNhan { get; set; }
    public string? ChiSoChotDienNangAQTongChieuNhanUnit { get; set; }
    public bool? ChiSoChotDienNangAQTongChieuNhanQuality { get; set; }
    public DateTimeOffset? ChiSoChotDienNangAQTongChieuNhanUpdatedAt { get; set; }

    // Pmax
    public decimal? PmaxKyHienTai { get; set; }
    public string? PmaxKyHienTaiUnit { get; set; }
    public bool? PmaxKyHienTaiQuality { get; set; }
    public DateTimeOffset? PmaxKyHienTaiUpdatedAt { get; set; }

    public decimal? PmaxKyTruoc { get; set; }
    public string? PmaxKyTruocUnit { get; set; }
    public bool? PmaxKyTruocQuality { get; set; }
    public DateTimeOffset? PmaxKyTruocUpdatedAt { get; set; }

    // Connection Status & Warning Flags
    public bool? MatKetNoi { get; set; }
    public bool? MatKetNoiQuality { get; set; }
    public DateTimeOffset? MatKetNoiUpdatedAt { get; set; }

    public bool? CanhBao01 { get; set; }
    public bool? CanhBao01Quality { get; set; }
    public DateTimeOffset? CanhBao01UpdatedAt { get; set; }

    public bool? CanhBao02 { get; set; }
    public bool? CanhBao02Quality { get; set; }
    public DateTimeOffset? CanhBao02UpdatedAt { get; set; }

    public bool? CanhBao03 { get; set; }
    public bool? CanhBao03Quality { get; set; }
    public DateTimeOffset? CanhBao03UpdatedAt { get; set; }

    public bool? CanhBao04 { get; set; }
    public bool? CanhBao04Quality { get; set; }
    public DateTimeOffset? CanhBao04UpdatedAt { get; set; }

    public bool? CanhBao05 { get; set; }
    public bool? CanhBao05Quality { get; set; }
    public DateTimeOffset? CanhBao05UpdatedAt { get; set; }

    public bool? CanhBao06 { get; set; }
    public bool? CanhBao06Quality { get; set; }
    public DateTimeOffset? CanhBao06UpdatedAt { get; set; }

    public bool? CanhBao07 { get; set; }
    public bool? CanhBao07Quality { get; set; }
    public DateTimeOffset? CanhBao07UpdatedAt { get; set; }

    public bool? CanhBao08 { get; set; }
    public bool? CanhBao08Quality { get; set; }
    public DateTimeOffset? CanhBao08UpdatedAt { get; set; }

    public bool? CanhBao09 { get; set; }
    public bool? CanhBao09Quality { get; set; }
    public DateTimeOffset? CanhBao09UpdatedAt { get; set; }

    public bool? CanhBao10 { get; set; }
    public bool? CanhBao10Quality { get; set; }
    public DateTimeOffset? CanhBao10UpdatedAt { get; set; }

    // Meter Clock / Latest Closing / Pmax Timestamps (DateTime Registers)
    public short? MeterYear { get; set; }
    public bool? MeterYearQuality { get; set; }
    public short? MeterMonth { get; set; }
    public bool? MeterMonthQuality { get; set; }
    public short? MeterDay { get; set; }
    public bool? MeterDayQuality { get; set; }
    public short? MeterHour { get; set; }
    public bool? MeterHourQuality { get; set; }
    public short? MeterMinute { get; set; }
    public bool? MeterMinuteQuality { get; set; }
    public short? MeterSecond { get; set; }
    public bool? MeterSecondQuality { get; set; }

    public short? ClosingYear { get; set; }
    public bool? ClosingYearQuality { get; set; }
    public short? ClosingMonth { get; set; }
    public bool? ClosingMonthQuality { get; set; }
    public short? ClosingDay { get; set; }
    public bool? ClosingDayQuality { get; set; }
    public short? ClosingHour { get; set; }
    public bool? ClosingHourQuality { get; set; }
    public short? ClosingMinute { get; set; }
    public bool? ClosingMinuteQuality { get; set; }
    public short? ClosingSecond { get; set; }
    public bool? ClosingSecondQuality { get; set; }

    public short? PmaxCurrentYear { get; set; }
    public bool? PmaxCurrentYearQuality { get; set; }
    public short? PmaxCurrentMonth { get; set; }
    public bool? PmaxCurrentMonthQuality { get; set; }
    public short? PmaxCurrentDay { get; set; }
    public bool? PmaxCurrentDayQuality { get; set; }
    public short? PmaxCurrentHour { get; set; }
    public bool? PmaxCurrentHourQuality { get; set; }
    public short? PmaxCurrentMinute { get; set; }
    public bool? PmaxCurrentMinuteQuality { get; set; }
    public short? PmaxCurrentSecond { get; set; }
    public bool? PmaxCurrentSecondQuality { get; set; }

    public short? PmaxPrevYear { get; set; }
    public bool? PmaxPrevYearQuality { get; set; }
    public short? PmaxPrevMonth { get; set; }
    public bool? PmaxPrevMonthQuality { get; set; }
    public short? PmaxPrevDay { get; set; }
    public bool? PmaxPrevDayQuality { get; set; }
    public short? PmaxPrevHour { get; set; }
    public bool? PmaxPrevHourQuality { get; set; }
    public short? PmaxPrevMinute { get; set; }
    public bool? PmaxPrevMinuteQuality { get; set; }
    public short? PmaxPrevSecond { get; set; }
    public bool? PmaxPrevSecondQuality { get; set; }
}

