using Domain.Entities;

namespace Application.DataTransferObjects.Responses.GcsDetail;

public class GcsDetailDto
{
    public long Id { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public string Address { get; set; }
    public string Description { get; set; }
    public long? UserId { get; set; }
    public UserEntity User { get; set; }
    public string? Email { get; set; }
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
}