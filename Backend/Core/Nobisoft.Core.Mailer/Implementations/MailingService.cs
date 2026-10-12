using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Nobisoft.Core.Configuration;
using Nobisoft.Core.Logger.Interfaces;
using Nobisoft.Core.Mailer.Interfaces;
using Nobisoft.Core.Mailer.Templates;

namespace Nobisoft.Core.Mailer.Implementations;

public class MailingService : IMailingService
{
    private readonly ILoggerService _loggerService;
    private readonly EmailConfiguration _emailConfiguration;

    public MailingService(ILoggerService loggerService, IOptions<EmailConfiguration> emailConfiguration)
    {
        _loggerService = loggerService;
        _emailConfiguration = emailConfiguration.Value;
    }

    public async Task SendEmailAsync(Email emailTemplate, CancellationToken cancellationToken)
    {
        try
        {
            using (MailMessage message = new MailMessage())
            {
                // Adding To, Cc, Bcc, and other email details
                foreach (MailAddress to in emailTemplate.To)
                    message.To.Add(to);
                if (emailTemplate.Cc != null && emailTemplate.Cc.Count > 0)
                {
                    foreach (MailAddress cc in emailTemplate.Cc)
                        message.CC.Add(cc);
                }
                if (emailTemplate.Bcc != null && emailTemplate.Bcc.Count > 0)
                {
                    foreach (MailAddress bcc in emailTemplate.Bcc)
                        message.Bcc.Add(bcc);
                }

                message.From = new MailAddress(this._emailConfiguration.Sender, this._emailConfiguration.DisplayName);
                message.Subject = emailTemplate.Subject;
                message.Body = emailTemplate.Body;
                message.IsBodyHtml = this._emailConfiguration.IsBodyHtml;

                if (emailTemplate.Attachments != null && emailTemplate.Attachments.Count > 0)
                {
                    foreach (Attachment attachment in emailTemplate.Attachments)
                        message.Attachments.Add(attachment);
                }

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = this._emailConfiguration.Host;
                    smtp.Port = this._emailConfiguration.Port;
                    smtp.EnableSsl = this._emailConfiguration.EnableSsl;
                    smtp.Credentials = new NetworkCredential(this._emailConfiguration.Sender, this._emailConfiguration.Password);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                    Console.WriteLine("Sending email...");
                    try
                    {
                        await smtp.SendMailAsync(message, cancellationToken);
                    }
                    catch (SmtpFailedRecipientException ex)
                    {
                        _loggerService.LogError(ex, "Failed to send email to recipient: " + ex.FailedRecipient);
                        throw;
                    }

                    Console.WriteLine("Email sent successfully.");
                }
            }
        }
        catch (SmtpFailedRecipientsException ex)
        {
            // This will catch multiple failed recipient errors
            foreach (var recipientException in ex.InnerExceptions)
            {
                if (recipientException is SmtpFailedRecipientException failedRecipientEx)
                {
                    _loggerService.LogError(failedRecipientEx, "Failed to send email to recipient: " + failedRecipientEx.FailedRecipient, nameof(SendEmailAsync));
                }
            }
            throw;
        }
        catch (SmtpFailedRecipientException ex)
        {
            // This is thrown when sending to a specific recipient fails
            _loggerService.LogError(ex, "Failed to send email to recipient: " + ex.FailedRecipient, nameof(SendEmailAsync));
            throw;
        }
        catch (SmtpException ex)
        {
            // This handles SMTP protocol issues, such as network problems or authentication errors
            _loggerService.LogError(ex, "SMTP error: " + ex.Message, nameof(SendEmailAsync));
            throw;
        }
        catch (InvalidOperationException ex)
        {
            // Handles errors like sending while already in progress
            _loggerService.LogError(ex, "Operation error while sending email: " + ex.Message, nameof(SendEmailAsync));
            throw;
        }
        catch (Exception ex)
        {
            // This catches any unexpected errors
            _loggerService.LogError(ex, "Unexpected error occurred: " + ex.Message, nameof(SendEmailAsync));
            throw;
        }
    }

}