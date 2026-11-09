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
    public DateTimeOffset? PmaxTime { get; set; }
    public string MeterReadingSet { get; set; }
    public decimal? PreviousReading { get; set; }
    public decimal? CurrentReading { get; set; }
    public bool? Quanlity { get; set; }
    public decimal? NewCurrentReading { get; set; }
    public decimal? NewPreviousReading { get; set; }
    public decimal MultiplierFactor { get; set; } = 1.0m;
    public decimal? Consumption { get; set; }
    public decimal? CosPhi { get; set; }
    public bool IsSuspicious { get; set; } = false;
    public string? SuspiciousReason { get; set; }
    public bool IsChange { get; set; } = false;
    public DateTimeOffset? ModifiedTime { get; set; }
    public long? ModifiedBy { get; set; }
    public string? ModifiedName { get; set; }
}
