namespace Application.DataTransferObjects.Requests.EmailConfig;

public class UpdateSystemEmailConfigRequest
{
    public string? ProviderType { get; set; } = "MICROSOFT_GRAPH"; // MICROSOFT_GRAPH, CUSTOM_SMTP, GMAIL
    public string? SenderEmail { get; set; }
    public string? SenderDisplayName { get; set; }

    // Microsoft Graph API
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; } // Nếu để trống/null thì giữ nguyên secret cũ đã lưu

    // Custom SMTP / Gmail
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; } = 587;
    public string? SecurityMode { get; set; } = "StartTls"; // None, Auto, SslOnConnect, StartTls
    public bool RequireAuth { get; set; } = true;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; } // Nếu để trống/null thì giữ nguyên password cũ đã lưu
    
    public bool IsActive { get; set; } = true;
}
