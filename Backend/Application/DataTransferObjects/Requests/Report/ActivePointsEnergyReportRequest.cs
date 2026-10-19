namespace Application.DataTransferObjects.Requests.Report;

public class ActivePointsEnergyReportRequest
{
    public long? CustomerId { get; set; }
    public DateTimeOffset SelectedTime { get; set; }
    public string? Search { get; set; }
}
