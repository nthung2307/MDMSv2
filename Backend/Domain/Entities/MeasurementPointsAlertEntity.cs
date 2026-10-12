using Nobisoft.Core.Domain.Entities;
using VF.Core.Domain.Attributes;

namespace Domain.Entities;

public class MeasurementPointsAlertEntity : IEntity<long>
{
    public long Id { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public long TagNameId { get; set; }
    public TagNameEntity TagName { get; set; }
    public string EventName { get; set; }
    public string Status { get; set; }
    public string Value { get; set; }
    public bool? Quanlity { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}