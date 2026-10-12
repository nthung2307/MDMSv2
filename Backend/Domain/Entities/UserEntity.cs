using Nobisoft.Core.Constants;
using Nobisoft.Core.Domain.Entities;
using VF.Core.Domain.Attributes;

namespace Domain.Entities;

public class UserEntity : IEntity<long>
{
    public long Id { get; set; }
    public string UserName { get; set; }
    public string? Password { get; set; }
    [Searchable] public string? FullName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    [Searchable] public string? CustomerCode { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = EntityStatus.Active;
    public int CountLocked { get; set; }
    public DateTimeOffset TimeLocked { get; set; }
    public bool IsLocked { get; set; }
    [Searchable] public string? RoleName { get; set; }
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
    public ICollection<UserTokenEntity> UserTokens { get; set; } = new List<UserTokenEntity>();
}