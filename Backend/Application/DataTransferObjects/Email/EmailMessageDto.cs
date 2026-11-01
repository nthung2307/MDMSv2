using System.Collections.Generic;

namespace Application.DataTransferObjects.Email;

public class EmailMessageDto
{
    public List<string> To { get; set; } = new List<string>();
    public List<string>? Cc { get; set; } = new List<string>();
    public List<string>? Bcc { get; set; } = new List<string>();
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public List<EmailAttachmentDto>? Attachments { get; set; } = new List<EmailAttachmentDto>();

    public EmailMessageDto()
    {
    }

    public EmailMessageDto(string to, string subject, string bodyHtml)
    {
        To = new List<string> { to };
        Subject = subject;
        BodyHtml = bodyHtml;
    }
}
