using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class MeasurementPointHalfHourEnergyReportEntity : IEntity<long>
{
    public long Id { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPTongKenhGiao { get; set; }
    public decimal? ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan { get; set; }
    public decimal? ChiSoTichLuyDienNangTacDungAPTongKenhNhan { get; set; }
    public decimal? ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan { get; set; }
    public int SuspectedMask { get; set; }
    public bool IsSuspected { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity? MeasurementPoint { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
}
