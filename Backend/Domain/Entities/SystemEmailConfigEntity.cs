using System;
using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class SystemEmailConfigEntity : IEntity<long>
{
    public long Id { get; set; }
    public string ProviderType { get; set; } = "MICROSOFT_GRAPH"; // MICROSOFT_GRAPH, CUSTOM_SMTP, GMAIL
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderDisplayName { get; set; } = string.Empty;
    
    // Microsoft Graph API (OAuth 2.0)
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? EncryptedClientSecret { get; set; }
    
    // Custom SMTP / Gmail
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SecurityMode { get; set; } = "StartTls"; // None, Auto, SslOnConnect, StartTls
    public bool RequireAuth { get; set; } = true;
    public string? SmtpUsername { get; set; }
    public string? EncryptedSmtpPassword { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
}
