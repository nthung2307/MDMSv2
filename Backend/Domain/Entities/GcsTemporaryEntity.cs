using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class GcsTemporaryEntity : IEntity<long>
{
    public long Id { get; set; }
    public long GcsId { get; set; }
    public GcsEntity Gcs { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public int Day { get; set; }
    public int? PreviousDay { get; set; }
    public int Month { get; set; }
    public int? PreviousMonth { get; set; }
    public int Year { get; set; }
    public int? PreviousYear { get; set; }
    public int PeriodNumber { get; set; }
    public int? PreviousPeriodNumber { get; set; }
    public DateTimeOffset ReadingTime { get; set; }
    public string MeterReadingSet { get; set; }
    public double? PreviousReading { get; set; }
    public double? CurrentReading { get; set; }
    public bool? Quanlity { get; set; }
    public double? NewCurrentReading { get; set; }
    public double? NewPreviousReading { get; set; }
    public double MultiplierFactor { get; set; }
    public double? Consumption { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
    public long? ModifiedBy { get; set; }
    public string? ModifiedName { get; set; }
}