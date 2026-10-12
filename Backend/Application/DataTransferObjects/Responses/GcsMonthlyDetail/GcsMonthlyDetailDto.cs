
using Domain.Entities;

namespace Application.DataTransferObjects.Responses.GcsMonthlyDetail;

public class GcsMonthlyDetailDto
{
    public long Id { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public string MeterReadingSet { get; set; }
    public double PreviousReading { get; set; }
    public double CurrentReading { get; set; }
    public double MultiplierFactor { get; set; }
    public double Consumption { get; set; }
    public bool IsChange { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
}