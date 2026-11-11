using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DataTransferObjects.Email;
using Application.Repositories.ReadOnly;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class EmailServiceService : IEmailService
{
    private readonly IEmailProviderFactory _emailProviderFactory;
    private readonly INotificationTemplateService _notificationTemplateService;
    private readonly IConfiguration _config;
    private readonly ISystemConfigReadOnlyRepository? _systemConfigRepository;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public EmailServiceService(
        IEmailProviderFactory emailProviderFactory,
        INotificationTemplateService notificationTemplateService,
        IConfiguration config,
        ISystemConfigReadOnlyRepository? systemConfigRepository = null,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _emailProviderFactory = emailProviderFactory;
        _notificationTemplateService = notificationTemplateService;
        _config = config;
        _systemConfigRepository = systemConfigRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Nhận diện đường dẫn công khai (Public Web Base URL) để sinh liên kết chính xác trong Email.
    /// Ưu tiên:
    /// 1. Cấu hình PUBLIC_BASE_URL trong CSDL do SuperAdmin cài đặt trên Web.
    /// 2. Nhận diện động từ Reverse Proxy / Cloudflare Tunnel (X-Forwarded-Proto, X-Forwarded-Host).
    /// 3. Cấu hình FrontEndUrl trong appsettings.json nếu không phải localhost / 127.0.0.1.
    /// 4. Fallback an toàn.
    /// </summary>
    public async Task<string> ResolveBaseUrlAsync(CancellationToken cancellationToken = default)
    {
        // 1. Kiểm tra CSDL
        if (_systemConfigRepository != null)
        {
            try
            {
                var configs = await _systemConfigRepository.GetListSystemConfigAsync(cancellationToken);
                var dbUrl = configs?.Find(x => x.Key == Constants.SystemConfig.PublicBaseUrl)?.Value;
                if (!string.IsNullOrWhiteSpace(dbUrl))
                {
                    return dbUrl.Trim().TrimEnd('/');
                }
            }
            catch
            {
                // Fallback nếu có lỗi đọc CSDL
            }
        }

        // 2. Nhận diện động từ HTTP Request hiện tại (Cloudflare Tunnel, Domain hoặc WAN IP)
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext != null && httpContext.Request != null)
        {
            var proto = httpContext.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(proto))
            {
                proto = httpContext.Request.Scheme;
            }

            var host = httpContext.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(host))
            {
                host = httpContext.Request.Host.Value;
            }

            if (!string.IsNullOrWhiteSpace(host) &&
                !host.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) &&
                !host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
            {
                return $"{proto}://{host}".TrimEnd('/');
            }
        }

        // 3. Fallback vào IConfiguration FrontEndUrl nếu hợp lệ
        var configUrl = _config["FrontEndUrl"];
        if (!string.IsNullOrWhiteSpace(configUrl) &&
            !configUrl.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) &&
            !configUrl.Contains("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return configUrl.TrimEnd('/');
        }

        // 4. Nếu đang ở môi trường nội bộ có HttpContext
        if (httpContext != null && httpContext.Request != null)
        {
            return $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}".TrimEnd('/');
        }

        return !string.IsNullOrWhiteSpace(configUrl) ? configUrl.TrimEnd('/') : "http://localhost:80";
    }

    public async Task SendEmailForgotPassword(string to, string subject, string token, string userName)
    {
        var frontendUrl = await ResolveBaseUrlAsync();
        var resetLink = $"{frontendUrl}/reset-password?token={token}&email={to}";

        var context = new Dictionary<string, object?>
        {
            ["Username"] = userName,
            ["CustomerName"] = userName,
            ["ResetLink"] = resetLink,
            ["Token"] = token,
            ["Email"] = to,
            ["PublicBaseUrl"] = frontendUrl,
            ["PortalUrl"] = frontendUrl,
            ["LoginUrl"] = $"{frontendUrl}/login"
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
        var frontendUrl = await ResolveBaseUrlAsync();
        var loginUrl = $"{frontendUrl}/login";

        var context = new Dictionary<string, object?>
        {
            ["FullName"] = fullName,
            ["CustomerName"] = fullName,
            ["Username"] = userName,
            ["Password"] = password,
            ["LoginUrl"] = loginUrl,
            ["Email"] = to,
            ["PublicBaseUrl"] = frontendUrl,
            ["PortalUrl"] = frontendUrl
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