using Nobisoft.Core.Domain.Entities;
using VF.Core.Domain.Attributes;

namespace Domain.Entities;

public class GcsEntity : IEntity<long>
{
    public long Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int PeriodNumber { get; set; }
    public string? Description { get; set; }
    public List<int> DayPeriod { get; set; } = new List<int>();
    public string Status { get; set; }
    public bool IsNewChange { get; set; }
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
    public List<MeasurementPointsEntity> MeasurementPoints { get; set; } = new List<MeasurementPointsEntity>();
}