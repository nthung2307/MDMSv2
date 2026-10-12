namespace Application.DataTransferObjects.Requests.TagName;

public class UpdateListTagNameRequest
{
    public List<UpdateTagNameRequest> TagNames { get; set; }
    public long MeasurementPointId { get; set; }
    public string Type { get; set; }
}

public class UpdateTagNameRequest
{
    public long Id { get; set; }
    public string Description { get; set; }
    public string? TagName { get; set; }
    public string? Unit { get; set; }
    public string? CustomName { get; set; }
}