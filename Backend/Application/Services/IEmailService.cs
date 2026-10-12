using Nobisoft.Core.Lifetimes;

namespace Application.Services;

public interface IEmailService : IScopedDependency
{
    Task SendEmailForgotPassword(string to, string subject, string token, string userName);
    Task SendEmailGeneratePassword(string to, string subject, string password, string fullName, string userName);
}