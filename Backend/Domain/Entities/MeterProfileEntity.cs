using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

/// <summary>
/// Mẫu chuẩn ánh xạ dữ liệu công tơ theo mã OBIS IEC 62056-61 (Trung thế kV / Hạ thế V).
/// </summary>
public class MeterProfileEntity : IEntity<long>
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string VoltageLevel { get; set; } = "LV";
    public string DefaultVoltageUnit { get; set; } = "V";
    public string DefaultPowerUnit { get; set; } = "kW";
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; } = true;
    public DateTimeOffset? CreatedTime { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedName { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
    public long? ModifiedBy { get; set; }
    public string? ModifiedName { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedTime { get; set; }
    public long? DeletedBy { get; set; }
    public string? DeletedName { get; set; }

    public ICollection<MeterProfileTagMappingEntity> TagMappings { get; set; } = new List<MeterProfileTagMappingEntity>();
    public ICollection<MeasurementPointsEntity> MeasurementPoints { get; set; } = new List<MeasurementPointsEntity>();
}

