namespace Application.DataTransferObjects.Requests.Report;

public class FinalizedOutputElectricityChartRequest
{
    public long MeasurementPointId { get; set; }
    public long CustomerId { get; set; }
    public DateTimeOffset Date { get; set; }
}