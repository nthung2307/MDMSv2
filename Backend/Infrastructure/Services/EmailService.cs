using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Common;
using Application.DataTransferObjects.Email;
using Application.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class EmailServiceService : IEmailService
{
    private readonly IEmailProviderFactory _emailProviderFactory;
    private readonly INotificationTemplateService _notificationTemplateService;
    private readonly IConfiguration _config;

    public EmailServiceService(
        IEmailProviderFactory emailProviderFactory,
        INotificationTemplateService notificationTemplateService,
        IConfiguration config)
    {
        _emailProviderFactory = emailProviderFactory;
        _notificationTemplateService = notificationTemplateService;
        _config = config;
    }

    public async Task SendEmailForgotPassword(string to, string subject, string token, string userName)
    {
        var frontendUrl = _config["FrontEndUrl"] ?? "https://amr.nobisoft.vn";
        var resetLink = $"{frontendUrl.TrimEnd('/')}/reset-password?token={token}&email={to}";

        var context = new Dictionary<string, object?>
        {
            ["Username"] = userName,
            ["CustomerName"] = userName,
            ["ResetLink"] = resetLink,
            ["Token"] = token,
            ["Email"] = to
        };

        var (renderedSubject, renderedBodyHtml) = await _notificationTemplateService.RenderNotificationAsync(
            Constants.NotificationTemplateCode.ForgotPassword,
            context);

        var finalSubject = !string.IsNullOrWhiteSpace(renderedSubject) ? renderedSubject : subject;

        var provider = await _emailProviderFactory.GetActiveProviderAsync();
        await provider.SendEmailAsync(new EmailMessageDto
        {
            To = new List<string> { to },
            Subject = finalSubject,
            BodyHtml = renderedBodyHtml
        });
    }

    public async Task SendEmailGeneratePassword(string to, string subject, string password, string fullName, string userName)
    {
        var frontendUrl = _config["FrontEndUrl"] ?? "https://amr.nobisoft.vn";
        var loginUrl = $"{frontendUrl.TrimEnd('/')}/login";

        var context = new Dictionary<string, object?>
        {
            ["FullName"] = fullName,
            ["CustomerName"] = fullName,
            ["Username"] = userName,
            ["Password"] = password,
            ["LoginUrl"] = loginUrl,
            ["Email"] = to
        };

        var (renderedSubject, renderedBodyHtml) = await _notificationTemplateService.RenderNotificationAsync(
            Constants.NotificationTemplateCode.UserCredential,
            context);

        var finalSubject = !string.IsNullOrWhiteSpace(renderedSubject) ? renderedSubject : subject;

        var provider = await _emailProviderFactory.GetActiveProviderAsync();
        await provider.SendEmailAsync(new EmailMessageDto
        {
            To = new List<string> { to },
            Subject = finalSubject,
            BodyHtml = renderedBodyHtml
        });
    }
}