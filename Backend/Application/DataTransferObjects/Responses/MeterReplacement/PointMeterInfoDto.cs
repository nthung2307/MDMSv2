using System;

namespace Application.DataTransferObjects.Responses.MeterReplacement;

public class PointMeterInfoDto
{
    public long MeasurementPointId { get; set; }
    public string PointCode { get; set; } = string.Empty;
    public string PointName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal CurrentMultiplier { get; set; } = 1.0m;
    public string CurrentSerial { get; set; } = string.Empty;

    // Chỉ số đọc gần nhất (nếu có từ GCS hoặc chốt kỳ)
    public decimal LatestBt { get; set; }
    public decimal LatestCd { get; set; }
    public decimal LatestTd { get; set; }
    public decimal LatestSg { get; set; }
    public decimal LatestVc { get; set; }
    public decimal LatestPmax { get; set; }
    public DateTimeOffset? LatestReadingTime { get; set; }
}

