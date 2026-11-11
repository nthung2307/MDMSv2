using System;
using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

/// <summary>
/// Monthly RANGE-partitioned wide-table time-series storing numeric telemetry buckets (12-month retention).
/// </summary>
public class MeasurementPointTimeSeriesEntity : IEntity<long>
{
    public long Id { get; set; }
    public long MeasurementPointId { get; set; }
    public int PartitionYm { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public DateTimeOffset IngestedAt { get; set; }
    public bool QualityFlag { get; set; } = true;
    public byte SourcePacketType { get; set; } = 1;

    // Operational Parameters
    public decimal? DongDienPhaA { get; set; }
    public decimal? DongDienPhaB { get; set; }
    public decimal? DongDienPhaC { get; set; }
    public decimal? DienApPhaA { get; set; }
    public decimal? DienApPhaB { get; set; }
    public decimal? DienApPhaC { get; set; }
    public decimal? CongSuatTacDungPhaA { get; set; }
    public decimal? CongSuatTacDungPhaB { get; set; }
    public decimal? CongSuatTacDungPhaC { get; set; }
    public decimal? CongSuatTacDungTong3Pha { get; set; }
    public decimal? CongSuatPhanKhangPhaA { get; set; }
    public decimal? CongSuatPhanKhangPhaB { get; set; }
    public decimal? CongSuatPhanKhangPhaC { get; set; }
    public decimal? CongSuatPhanKhangTong3Pha { get; set; }
    public decimal? HeSoCongSuatPFPhaA { get; set; }
    public decimal? HeSoCongSuatPFPhaB { get; set; }
    public decimal? HeSoCongSuatPFPhaC { get; set; }
    public decimal? HeSoCongSuatPFTrungBinh3Pha { get; set; }
    public decimal? TanSoHeThong { get; set; }

    // Cumulative Energy Registers - Export & Import
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioBT { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioCD { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioTD { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPTong { get; set; }
    public decimal? ChiSoTichLuyDienNangPhanKhangAQTong { get; set; }

    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPTongChieuNhan { get; set; }
    public decimal? ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan { get; set; }

    // Latch Energy Registers & Pmax
    public decimal? ChiSoChotDienNangAPGioBT { get; set; }
    public decimal? ChiSoChotDienNangAPGioCD { get; set; }
    public decimal? ChiSoChotDienNangAPGioTD { get; set; }
    public decimal? ChiSoChotDienNangAPTong { get; set; }
    public decimal? ChiSoChotDienNangAQTong { get; set; }

    public decimal? ChiSoChotDienNangAPGioBTChieuNhan { get; set; }
    public decimal? ChiSoChotDienNangAPGioCDChieuNhan { get; set; }
    public decimal? ChiSoChotDienNangAPGioTDChieuNhan { get; set; }
    public decimal? ChiSoChotDienNangAPTongChieuNhan { get; set; }
    public decimal? ChiSoChotDienNangAQTongChieuNhan { get; set; }

    public decimal? PmaxKyHienTai { get; set; }
    public decimal? PmaxKyTruoc { get; set; }
}

