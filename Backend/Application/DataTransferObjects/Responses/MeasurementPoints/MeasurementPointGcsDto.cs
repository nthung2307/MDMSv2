using Application.DataTransferObjects.Responses.TagName;
using Domain.Entities;

namespace Application.DataTransferObjects.Responses.MeasurementPoints;

public class MeasurementPointGcsDto
{
    public long Id { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
    public string? CustomerName { get; set; }
    public double MultiplierFactor { get; set; }
    public long? GcsId { get; set; }
    public long? OldGcsId { get; set; }
    public bool IsNewChange { get; set; }
    public List<TagNameDto> TagNames { get; set; } = new List<TagNameDto>();
}