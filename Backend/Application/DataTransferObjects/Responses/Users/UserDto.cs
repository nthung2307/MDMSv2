using Domain.Entities;

namespace Application.DataTransferObjects.Responses.Users;

public class UserDto
{
    public long Id { get; set; }
    public string UserName { get; set; }
    public string RoleName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CustomerCode { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Status { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
}