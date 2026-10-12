namespace Application.DataTransferObjects.Requests.TagName;

public class CreateListTagNameRequest
{
    public List<CreateTagNameRequest> TagNames { get; set; }
    public long MeasurementPointId { get; set; }
}

public class CreateTagNameRequest
{
    public string Description { get; set; }
    public string? TagName { get; set; }
    public string? Unit { get; set; }
    public string? CustomName { get; set; }
    public string Type { get; set; }
}