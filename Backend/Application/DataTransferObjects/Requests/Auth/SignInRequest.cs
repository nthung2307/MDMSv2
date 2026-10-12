namespace Application.DataTransferObjects.Requests.Auth;

public class SignInRequest
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}