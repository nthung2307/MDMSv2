namespace Application.DataTransferObjects.Requests.Report;

public class EnergyConsumptionReportRequest
{
    public long CustomerId { get; set; }
    public long MeasurementPointId { get; set; }
    public string CycleType { get; set; } = "HALF_HOUR"; // "HALF_HOUR", "MONTHLY", "YEARLY"
    public DateTimeOffset? Date { get; set; }
    public int? Month { get; set; }
    public int Year { get; set; }
    public bool CompareWithPrevious { get; set; } = true;
}
