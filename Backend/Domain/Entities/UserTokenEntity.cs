using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class UserTokenEntity : IEntity<long>
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public UserEntity User { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string? Type { get; set; }
    public DateTimeOffset? ExpiredTime { get; set; }
}