namespace Application.DataTransferObjects.Requests.Auth;

public class ConfirmForgotPasswordRequest
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
}