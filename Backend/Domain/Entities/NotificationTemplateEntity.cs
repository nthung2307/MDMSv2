using System;
using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class NotificationTemplateEntity : IEntity<long>
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty; // USER_CREDENTIAL, FORGOT_PASSWORD, STATEMENT_NOTIFICATION
    public string Name { get; set; } = string.Empty;
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyHtmlTemplate { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
}
