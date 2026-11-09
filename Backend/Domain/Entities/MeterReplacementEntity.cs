using System;
using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class MeterReplacementEntity : IEntity<long>
{
    public long Id { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public DateTimeOffset ReplacementDate { get; set; }
    public string Reason { get; set; } = string.Empty; // Kiểm định định kỳ, Hỏng màn hình, Nâng công suất...
    
    // Chỉ số công tơ tháo
    public string OldMeterSerial { get; set; } = string.Empty;
    public decimal OldMeterMultiplier { get; set; } = 1.0m;
    public decimal OldBt { get; set; }
    public decimal OldCd { get; set; }
    public decimal OldTd { get; set; }
    public decimal OldSg { get; set; }
    public decimal OldVc { get; set; }
    public decimal OldPmax { get; set; }
    
    // Chỉ số công tơ treo mới
    public string NewMeterSerial { get; set; } = string.Empty;
    public decimal NewMeterMultiplier { get; set; } = 1.0m;
    public decimal NewBt { get; set; }
    public decimal NewCd { get; set; }
    public decimal NewTd { get; set; }
    public decimal NewSg { get; set; }
    public decimal NewVc { get; set; }
    public decimal NewPmax { get; set; }
    
    public string? DocumentUrl { get; set; } // Ảnh scan hoặc PDF biên bản treo tháo hiện trường
    public string Status { get; set; } = "APPROVED"; // DRAFT, APPROVED, CANCELLED
    public DateTimeOffset CreatedTime { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedName { get; set; }
}
