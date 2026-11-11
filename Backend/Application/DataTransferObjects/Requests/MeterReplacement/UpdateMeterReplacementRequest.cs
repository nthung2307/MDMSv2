using System;

namespace Application.DataTransferObjects.Requests.MeterReplacement;

public class UpdateMeterReplacementRequest
{
    public long MeasurementPointId { get; set; }
    public DateTimeOffset ReplacementDate { get; set; }
    public string Reason { get; set; } = string.Empty;

    // Chỉ số công tơ tháo (Cũ)
    public string OldMeterSerial { get; set; } = string.Empty;
    public decimal OldMeterMultiplier { get; set; } = 1.0m;
    public decimal OldBt { get; set; }
    public decimal OldCd { get; set; }
    public decimal OldTd { get; set; }
    public decimal OldSg { get; set; }
    public decimal OldVc { get; set; }
    public decimal OldPmax { get; set; }

    // Chỉ số công tơ treo (Mới)
    public string NewMeterSerial { get; set; } = string.Empty;
    public decimal NewMeterMultiplier { get; set; } = 1.0m;
    public decimal NewBt { get; set; }
    public decimal NewCd { get; set; }
    public decimal NewTd { get; set; }
    public decimal NewSg { get; set; }
    public decimal NewVc { get; set; }
    public decimal NewPmax { get; set; }

    public string? DocumentUrl { get; set; }
    public string Status { get; set; } = "DRAFT"; // DRAFT, APPROVED
}

