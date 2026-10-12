using Newtonsoft.Json;
using Nobisoft.Core.Domain.Entities;
using VF.Core.Domain.Attributes;

namespace Domain.Entities;

public class MeasurementPointMonitoringEntity : IEntity<long>
{
    public long Id { get; set; }
    public long MeasurementPointsId { get; set; }
    public MeasurementPointsEntity MeasurementPoints { get; set; }
    public long? TagNameId { get; set; }
    public TagNameEntity TagName { get; set; }
    public string? TagNameDescription { get; set; }
    public string? Value { get; set; }
    public bool? Quanlity { get; set; }
    public long? GroupId { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedName { get; set; }
}