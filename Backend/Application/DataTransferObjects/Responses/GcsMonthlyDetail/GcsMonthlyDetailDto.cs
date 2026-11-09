using Domain.Entities;

namespace Application.DataTransferObjects.Responses.GcsMonthlyDetail;

public class GcsMonthlyDetailDto
{
    public long Id { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public string MeterReadingSet { get; set; }
    public decimal? PreviousReading { get; set; }
    public decimal? CurrentReading { get; set; }
    public decimal MultiplierFactor { get; set; }
    public decimal? Consumption { get; set; }
    public decimal? CosPhi { get; set; }
    public DateTimeOffset? ReadingTime { get; set; }
    public DateTimeOffset? PmaxTime { get; set; }
    public bool IsChange { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
}
