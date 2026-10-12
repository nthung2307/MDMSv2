using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class MeasurementPointDailyEnergyReportEntity : IEntity<long>
{
    public long Id { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiaoUnit { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiaoUnit { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiaoUnit { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPTongKenhGiao { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPTongKenhGiaoUnit { get; set; }
    public string? ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao { get; set; }
    public string? ChiSoTichLuyDienNangPhanKhangAQTongKenhGiaoUnit { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhanUnit { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhanUnit { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhanUnit { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPTongKenhNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPTongKenhNhanUnit { get; set; }
    public string? ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan { get; set; }
    public string? ChiSoTichLuyDienNangPhanKhangAQTongKenhNhanUnit { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity? MeasurementPoint { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
}