using System.Threading;
using System.Threading.Tasks;
using Application.DataTransferObjects.Email;

namespace Application.Services;

public interface IEmailProviderService
{
    string ProviderType { get; }
    Task SendEmailAsync(EmailMessageDto message, CancellationToken cancellationToken = default);
    Task<bool> TestConnectionAsync(string testRecipientEmail, CancellationToken cancellationToken = default);
}
