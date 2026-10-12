namespace Application.DataTransferObjects.Requests.Users;

public class UpdateUserRequest
{
    public long Id { get; set; }
    public string RoleName { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? CustomerCode { get; set; }
    public string? Description { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
}