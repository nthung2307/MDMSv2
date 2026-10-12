using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class TagNameEntity : IEntity<long>
{
    public long Id { get; set; }
    public string Description { get; set; }
    public string? TagName { get; set; }
    public string? Unit { get; set; }
    public string? CustomName { get; set; }
    public string Type { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public string Status { get; set; }
    public bool? WarningValue { get; set; }
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