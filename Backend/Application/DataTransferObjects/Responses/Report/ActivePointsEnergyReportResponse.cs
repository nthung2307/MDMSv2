namespace Application.DataTransferObjects.Responses.Report;

public class ActivePointsEnergyReportResponse
{
    public long MeasurementPointId { get; set; }
    public string MeasurementPointCode { get; set; } = string.Empty;
    public string? MeasurementPointName { get; set; }
    public long CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public double MultiplierFactor { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }

    // Kênh Giao (Export)
    public string? ChiSoTichLuyDienNangTacDungAPTongKenhGiao { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao { get; set; }
    public string? ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao { get; set; }

    // Kênh Nhận (Import)
    public string? ChiSoTichLuyDienNangTacDungAPTongKenhNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan { get; set; }
    public string? ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan { get; set; }
    public string? ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan { get; set; }
}
