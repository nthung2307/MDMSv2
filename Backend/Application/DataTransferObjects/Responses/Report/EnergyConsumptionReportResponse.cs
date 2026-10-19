namespace Application.DataTransferObjects.Responses.Report;

public class EnergyConsumptionReportResponse
{
    public List<EnergyConsumptionItemResponse> Items { get; set; } = new();
    public EnergyConsumptionItemResponse? Total { get; set; }
    public EnergyConsumptionChartData Chart { get; set; } = new();
}

public class EnergyConsumptionItemResponse
{
    public string TimeLabel { get; set; } = string.Empty;
    public DateTimeOffset? CreatedTime { get; set; }
    public double MultiplierFactor { get; set; } = 1.0;

    public bool IsSuspected { get; set; }

    // Chiều giao (kWh / kVArh)
    public double? SanLuongTacDungTongKenhGiao { get; set; }
    public double? SanLuongTacDungBTKenhGiao { get; set; }
    public double? SanLuongTacDungCDKenhGiao { get; set; }
    public double? SanLuongTacDungTDKenhGiao { get; set; }
    public double? SanLuongPhanKhangTongKenhGiao { get; set; }

    // Chiều nhận (kWh / kVArh)
    public double? SanLuongTacDungTongKenhNhan { get; set; }
    public double? SanLuongTacDungBTKenhNhan { get; set; }
    public double? SanLuongTacDungCDKenhNhan { get; set; }
    public double? SanLuongTacDungTDKenhNhan { get; set; }
    public double? SanLuongPhanKhangTongKenhNhan { get; set; }

    // So sánh với mốc trước gần nhất
    public double? PreviousSanLuongTacDungTongKenhGiao { get; set; }
    public double? PreviousSanLuongTacDungBTKenhGiao { get; set; }
    public double? PreviousSanLuongTacDungCDKenhGiao { get; set; }
    public double? PreviousSanLuongTacDungTDKenhGiao { get; set; }
    public double? PreviousSanLuongPhanKhangTongKenhGiao { get; set; }

    public double? PreviousSanLuongTacDungTongKenhNhan { get; set; }
    public double? PreviousSanLuongTacDungBTKenhNhan { get; set; }
    public double? PreviousSanLuongTacDungCDKenhNhan { get; set; }
    public double? PreviousSanLuongTacDungTDKenhNhan { get; set; }
    public double? PreviousSanLuongPhanKhangTongKenhNhan { get; set; }

    public double? DeltaKenhGiao { get; set; }
    public double? DeltaPercentageKenhGiao { get; set; }
    public double? DeltaKenhNhan { get; set; }
    public double? DeltaPercentageKenhNhan { get; set; }
}

public class EnergyConsumptionChartData
{
    public List<string> Labels { get; set; } = new();
    public List<double> CurrentPeriodGiao { get; set; } = new();
    public List<double> PreviousPeriodGiao { get; set; } = new();
    public List<double> CurrentPeriodNhan { get; set; } = new();
    public List<double> PreviousPeriodNhan { get; set; } = new();
}
