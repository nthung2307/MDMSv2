namespace Application.DataTransferObjects.Requests.Report;

public class ListFinalizedOutputElectricityRequest
{
    public long? MeasurementPointId { get; set; }
    public long? CustomerId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
}