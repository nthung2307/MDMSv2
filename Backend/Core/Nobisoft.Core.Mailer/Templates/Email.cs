using System.Net.Mail;

namespace Nobisoft.Core.Mailer.Templates;

public class Email
{
    public ICollection<MailAddress> To { get; set; }
    public ICollection<MailAddress>? Cc { get; set; }
    public ICollection<MailAddress>? Bcc { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public ICollection<Attachment>? Attachments { get; set; }
}