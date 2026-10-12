using Nobisoft.Core.Lifetimes;
using Nobisoft.Core.Mailer.Templates;

namespace Nobisoft.Core.Mailer.Interfaces;

public interface IMailingService: ISingletonDependency
{
    /// <summary>
    /// Send email to users who are defined in the email template
    /// </summary>
    /// <param name="emailTemplate">Email template which need to be sent</param>
    /// <param name="cancellationToken"></param>
    /// <returns>SendEmailResult</returns>
    public Task SendEmailAsync(Email emailTemplate, CancellationToken cancellationToken);
}

