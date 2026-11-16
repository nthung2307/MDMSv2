using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

/// <summary>
/// Chi tiết quy tắc ánh xạ từ mã OBIS chuẩn / alias Kepware sang tham số MDMS trong MeterProfile.
/// </summary>
public class MeterProfileTagMappingEntity : IEntity<long>
{
    public long Id { get; set; }
    public long MeterProfileId { get; set; }
    public MeterProfileEntity? MeterProfile { get; set; }
    public string ObisCode { get; set; } = string.Empty;
    public string ParameterCode { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? CustomName { get; set; }
    public string? LegacyAliases { get; set; }
    public string DataCategory { get; set; } = "ELECTRICAL";
    public int DisplayOrder { get; set; }
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
}

