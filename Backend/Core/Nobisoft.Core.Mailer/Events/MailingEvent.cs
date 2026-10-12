using System.Net.Mail;

namespace Nobisoft.Core.Mailer.Events;

public class MailingEvent
{
    public ICollection<MailAddress> To { get; set; }
    public ICollection<MailAddress>? Cc { get; set; }
    public ICollection<MailAddress>? Bcc { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public ICollection<string>? AttachmentNames { get; set; }
    public string Signature { get; set; }

    public MailingEvent(ICollection<MailAddress> to, ICollection<MailAddress>? cc, ICollection<MailAddress>? bcc, string subject, string body, ICollection<string>? attachmentNames, string signature)
    {
        To = to;
        Cc = cc;
        Bcc = bcc;
        Subject = subject;
        Body = body;
        AttachmentNames = attachmentNames;
        Signature = signature;
    }
}