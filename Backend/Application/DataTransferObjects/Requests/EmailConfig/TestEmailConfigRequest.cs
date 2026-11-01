namespace Application.DataTransferObjects.Requests.EmailConfig;

public class TestEmailConfigRequest
{
    public string TestRecipientEmail { get; set; } = string.Empty;
    public string? ProviderType { get; set; } // Nếu truyền thì test với thông số gửi lên, nếu rỗng thì test cấu hình hiện hành
    public string? SenderEmail { get; set; }
    public string? SenderDisplayName { get; set; }

    // Microsoft Graph
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }

    // SMTP
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SecurityMode { get; set; }
    public bool? RequireAuth { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
}
