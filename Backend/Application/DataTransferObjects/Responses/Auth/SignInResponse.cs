namespace Application.DataTransferObjects.Responses.Auth;

public class SignInResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string UserName { get; set; }
    public string? Role { get; set; }
    public UserProfile UserProfile { get; set; }
}

public class UserProfile
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string? FullName { get;set; }
    public string? PhoneNumber { get; set; }
}