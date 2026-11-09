namespace Application.DataTransferObjects.Requests.MeasurementPoints;

public class CreateMeasurementPointsRequest
{
    public string Code { get; set; }
    public string Type { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public long CustomerId { get; set; }
    public long? GcsId { get; set; }
    public decimal MultiplierFactor { get; set; } = 1.0m;
    public string Status { get; set; }
}
