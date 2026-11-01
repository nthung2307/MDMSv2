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
    public double OldMeterMultiplier { get; set; } = 1.0;
    public double OldBt { get; set; }
    public double OldCd { get; set; }
    public double OldTd { get; set; }
    public double OldSg { get; set; }
    public double OldVc { get; set; }
    public double OldPmax { get; set; }
    
    // Chỉ số công tơ treo mới
    public string NewMeterSerial { get; set; } = string.Empty;
    public double NewMeterMultiplier { get; set; } = 1.0;
    public double NewBt { get; set; }
    public double NewCd { get; set; }
    public double NewTd { get; set; }
    public double NewSg { get; set; }
    public double NewVc { get; set; }
    public double NewPmax { get; set; }
    
    public string? DocumentUrl { get; set; } // Ảnh scan hoặc PDF biên bản treo tháo hiện trường
    public string Status { get; set; } = "APPROVED"; // DRAFT, APPROVED, CANCELLED
    public DateTimeOffset CreatedTime { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedName { get; set; }
}
