using Domain.Entities;
using Newtonsoft.Json;

namespace Application.DataTransferObjects.Responses.MeasurementPointMonitoring;

public class ListMeasurementPointMonitoringDto
{
    public long Id { get; set; }
    public long MeasurementPointsId { get; set; }
    public string? MeasurementPointCode { get; set; }
    public string? MeasurementPointDescription { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public double? DongDienPhaA { get; set; }
    public string? DongDienPhaAUnit { get; set; }
    public bool? DongDienPhaAQuanlity { get; set; }
    public double? DienApPhaA { get; set; }
    public string? DienApPhaAUnit { get; set; }
    public bool? DienApPhaAQuanlity { get; set; }
    public double? CongSuatTacDungTong3Pha { get; set; }
    public string? CongSuatTacDungTong3PhaUnit { get; set; }
    public bool? CongSuatTacDungTong3PhaQuanlity { get; set; }
    public double? CongSuatPhanKhangTong3Pha { get; set; }
    public string? CongSuatPhanKhangTong3PhaUnit { get; set; }
    public bool? CongSuatPhanKhangTong3PhaQuanlity { get; set; }
    public double? HeSoCongSuatPFTrungBinh3Pha { get; set; }
    public bool? HeSoCongSuatPFTrungBinh3PhaQuanlity { get; set; }
    public double? TanSoHeThong { get; set; }
    public string? TanSoHeThongUnit { get; set; }
    public bool? TanSoHeThongQuanlity { get; set; }
    public bool? MatKetNoi { get; set; }
    public bool? MatKetNoiQuanlity { get; set; }
    public long GroupId { get; set; }
}