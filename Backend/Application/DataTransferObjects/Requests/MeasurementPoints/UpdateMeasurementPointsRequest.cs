namespace Application.DataTransferObjects.Requests.MeasurementPoints;

public class UpdateMeasurementPointsRequest
{
    public long Id { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public long CustomerId { get; set; }
    public long? GcsId { get; set; }
    public string Status { get; set; }
    public decimal MultiplierFactor { get; set; } = 1.0m;
    public string? MeterSerial { get; set; }
    public long? MeterProfileId { get; set; }
    public string? MappingMode { get; set; } = "HYBRID";
}
