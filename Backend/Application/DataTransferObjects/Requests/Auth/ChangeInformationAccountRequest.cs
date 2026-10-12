namespace Application.DataTransferObjects.Requests.Auth;

public class ChangeInformationAccountRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Description { get; set; }
}
