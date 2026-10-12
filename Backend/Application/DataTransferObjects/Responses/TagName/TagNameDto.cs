using Domain.Entities;

namespace Application.DataTransferObjects.Responses.TagName;

public class TagNameDto
{
    public long Id { get; set; }
    public string Description { get; set; }
    public string? TagName { get; set; }
    public string? Unit { get; set; }
    public string Type { get; set; }
    public string? CustomName { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
}