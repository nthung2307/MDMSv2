using System;

namespace Application.DataTransferObjects.Responses.MeterReplacement;

public class MeterReplacementDto
{
    public long Id { get; set; }
    public long MeasurementPointId { get; set; }
    public string MeasurementPointCode { get; set; } = string.Empty;
    public string MeasurementPointDescription { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTimeOffset ReplacementDate { get; set; }
    public string Reason { get; set; } = string.Empty;

    // Chỉ số công tơ tháo (Cũ)
    public string OldMeterSerial { get; set; } = string.Empty;
    public decimal OldMeterMultiplier { get; set; }
    public decimal OldBt { get; set; }
    public decimal OldCd { get; set; }
    public decimal OldTd { get; set; }
    public decimal OldSg { get; set; }
    public decimal OldVc { get; set; }
    public decimal OldPmax { get; set; }

    // Chỉ số công tơ treo (Mới)
    public string NewMeterSerial { get; set; } = string.Empty;
    public decimal NewMeterMultiplier { get; set; }
    public decimal NewBt { get; set; }
    public decimal NewCd { get; set; }
    public decimal NewTd { get; set; }
    public decimal NewSg { get; set; }
    public decimal NewVc { get; set; }
    public decimal NewPmax { get; set; }

    public string? DocumentUrl { get; set; }
    public string Status { get; set; } = "DRAFT"; // DRAFT, APPROVED, CANCELLED
    public DateTimeOffset CreatedTime { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedName { get; set; }
}

