using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class GcsMonthlyDetailEntity : IEntity<long>
{
    public long Id { get; set; }
    public long GcsCutoffCycleId { get; set; }
    public GcsCutoffCycleEntity GcsCutoffCycle { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public string MeterReadingSet { get; set; }
    public double? PreviousReading { get; set; }
    public double? CurrentReading { get; set; }
    public double MultiplierFactor { get; set; }
    public double? Consumption { get; set; }
    public bool IsChange { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedName { get; set; }
}