using Domain.Entities;

namespace Application.DataTransferObjects.Responses.MeasurementPoints;

public class MeasurementPointDto
{
    public long Id { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public string Address { get; set; }
    public string Description { get; set; }
    public long? CustomerId { get; set; }
    public CustomerEntity Customer { get; set; }
    public string Status { get; set; }
    public double MultiplierFactor { get; set; }
    public string? Email { get; set; }
    public string? Email2 { get; set; }
    public string? Email3 { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedName { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
    public long? ModifiedBy { get; set; }
    public string? ModifiedName { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedTime { get; set; }
    public long? DeletedBy { get; set; }
    public string? DeletedName { get; set; }
    public long? GcsId { get; set; }
    public GcsEntity Gcs { get; set; } = new GcsEntity();
    public long? OldGcsId { get; set; }
    public bool IsNewChange { get; set; }
    public List<TagNameEntity> TagNames { get; set; } = new List<TagNameEntity>();
}