using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class MeasurementPointMonitoringHourlyEntity : IEntity<long>
{
    public long Id { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public string TrungBinhCongSuatPhanKhangTong3Pha { get; set; }
    public string CongSuatPhanKhangTong3PhaCucDai { get; set; }
    public string CongSuatPhanKhangTong3PhaCucTieu { get; set; }
    public string TrungBinhCongSuatPhanKhangTong3PhaUnit { get; set; }
    public string TrungBinhCongSuatTacDungTong3Pha { get; set; }
    public string CongSuatTacDungTong3PhaCucDai { get; set; }
    public string CongSuatTacDungTong3PhaCucTieu { get; set; }
    public string CongSuatTacDungTong3PhaUnit { get; set; }
    public string TrungBinhDienApPhaA { get; set; }
    public string TrungBinhDienApPhaAUnit { get; set; }
    public string TrungBinhDienApPhaB { get; set; }
    public string TrungBinhDienApPhaBUnit { get; set; }
    public string TrungBinhDienApPhaC { get; set; }
    public string TrungBinhDienApPhaCUnit { get; set; }
    public string TrungBinhDongDienPhaA { get; set; }
    public string TrungBinhDongDienPhaAUnit { get; set; }
    public string TrungBinhDongDienPhaB { get; set; }
    public string TrungBinhDongDienPhaBUnit { get; set; }
    public string TrungBinhDongDienPhaC { get; set; }
    public string TrungBinhDongDienPhaCUnit { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioBT { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioCD { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioTD { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPTong { get; set; }

}