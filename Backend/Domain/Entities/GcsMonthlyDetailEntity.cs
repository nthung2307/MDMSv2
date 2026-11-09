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
    public decimal? PreviousReading { get; set; }
    public decimal? CurrentReading { get; set; }
    public decimal MultiplierFactor { get; set; } = 1.0m;
    public decimal? Consumption { get; set; }
    public decimal? CosPhi { get; set; }
    public DateTimeOffset? ReadingTime { get; set; }
    public DateTimeOffset? PmaxTime { get; set; }
    public bool IsChange { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedName { get; set; }
}
