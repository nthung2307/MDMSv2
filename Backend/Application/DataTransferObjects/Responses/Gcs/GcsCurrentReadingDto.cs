using Domain.Entities;

namespace Application.DataTransferObjects.Responses.Gcs;

public class GcsCurrentReadingDto
{
    public int CurrentPeriodNumber { get; set; }
    public int CurrentDay { get; set; }
    public int CurrentMonth { get; set; }
    public int CurrentYear { get; set; }
    public long MeasurementPointId { get; set; }
    public double? CurrentReading { get; set; }
    public string MeterReadingSet { get; set; }
}