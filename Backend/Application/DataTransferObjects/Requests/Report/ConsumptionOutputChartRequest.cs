namespace Application.DataTransferObjects.Requests.Report;

public class ConsumptionOutputChartRequest
{
    public string TimeSelector { get; set; }
    public long MeasurementPointId { get; set; }
    public long CustomerId { get; set; }
    public DateTimeOffset Date { get; set; }
}