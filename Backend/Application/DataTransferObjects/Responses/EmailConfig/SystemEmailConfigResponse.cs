using System;

namespace Application.DataTransferObjects.Responses.EmailConfig;

public class SystemEmailConfigResponse
{
    public long Id { get; set; }
    public string ProviderType { get; set; } = "MICROSOFT_GRAPH";
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;

    // Microsoft Graph
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public bool HasClientSecret { get; set; }

    // SMTP / Gmail
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SecurityMode { get; set; }
    public bool RequireAuth { get; set; }
    public string? SmtpUsername { get; set; }
    public bool HasSmtpPassword { get; set; }

    public bool IsActive { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
}
