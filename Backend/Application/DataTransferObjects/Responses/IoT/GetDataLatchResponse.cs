using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Domain.Entities;

namespace Application.DataTransferObjects.Responses.IoT;

public class GetDataLatchResponse
{
    public long Id { get; set; }
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
    public string? MeterReadingSet { get; set; }
    public decimal? PreviousReading { get; set; }
    public bool Quanlity { get; set; }
    public decimal? CurrentReading { get; set; }
    public decimal? MultiplierFactor { get; set; }
    public decimal? Consumption { get; set; }
    public decimal? CosPhi { get; set; }
    public decimal? NewCurrentReading { get; set; }
    public decimal? NewPreviousReading { get; set; }
    public bool IsSuspicious { get; set; }
    public string? SuspiciousReason { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
}
