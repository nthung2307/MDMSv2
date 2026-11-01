using System;

namespace Application.DataTransferObjects.Templates;

public class NotificationTemplateDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyHtmlTemplate { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? ModifiedTime { get; set; }
}

public class CreateNotificationTemplateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyHtmlTemplate { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateNotificationTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyHtmlTemplate { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PreviewNotificationTemplateRequest
{
    public string? Code { get; set; }
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyHtmlTemplate { get; set; } = string.Empty;
    public Dictionary<string, string>? CustomVariables { get; set; }
}

public class PreviewNotificationTemplateResponse
{
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
}
