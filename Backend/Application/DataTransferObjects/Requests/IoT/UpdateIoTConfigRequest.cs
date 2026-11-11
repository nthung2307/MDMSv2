namespace Application.DataTransferObjects.Requests.IoT;

public class UpdateIoTConfigRequest
{
    public string ApiKey { get; set; } = string.Empty;
    public string AllowedIps { get; set; } = string.Empty;
    public bool RequireApiKey { get; set; } = false;
}

