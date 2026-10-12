namespace Application.DataTransferObjects.Requests.Report;

public class VoltageChartRequest
{
    public string Type { get; set; }
    public string TimeSelector { get; set; }
    public long MeasurementPointId { get; set; }
    public long CustomerId { get; set; }
    public DateTimeOffset Date { get; set; }
}