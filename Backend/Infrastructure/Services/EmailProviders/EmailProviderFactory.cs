using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DataTransferObjects.Requests.EmailConfig;
using Application.Repositories.ReadOnly;
using Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.EmailProviders;

public class EmailProviderFactory : IEmailProviderFactory
{
    private readonly ISystemEmailConfigReadOnlyRepository _emailConfigRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<EmailProviderFactory> _logger;

    public EmailProviderFactory(
        ISystemEmailConfigReadOnlyRepository emailConfigRepo,
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _emailConfigRepo = emailConfigRepo;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<EmailProviderFactory>();
    }

    public async Task<IEmailProviderService> GetActiveProviderAsync(CancellationToken cancellationToken = default)
    {
        var dbConfig = await _emailConfigRepo.GetActiveConfigAsync(cancellationToken);
        if (dbConfig != null)
        {
            _logger.LogInformation("Khởi tạo Email Provider '{ProviderType}' từ cấu hình cơ sở dữ liệu...", dbConfig.ProviderType);
            return CreateProviderFromEntity(dbConfig);
        }

        _logger.LogInformation("Chưa có cấu hình email trong database. Áp dụng cấu hình mặc định từ appsettings.json...");
        return CreateFallbackProvider();
    }

    public IEmailProviderService CreateProviderFromEntity(Domain.Entities.SystemEmailConfigEntity config)
    {
        if (string.Equals(config.ProviderType, Constants.EmailProvider.MicrosoftGraph, StringComparison.OrdinalIgnoreCase))
        {
            var client = _httpClientFactory.CreateClient("MicrosoftGraphClient");
            var logger = _loggerFactory.CreateLogger<MicrosoftGraphEmailProvider>();
            return new MicrosoftGraphEmailProvider(client, _memoryCache, logger)
            {
                TenantId = config.TenantId ?? string.Empty,
                ClientId = config.ClientId ?? string.Empty,
                ClientSecret = config.EncryptedClientSecret ?? string.Empty,
                SenderEmail = config.SenderEmail ?? string.Empty,
                SenderDisplayName = config.SenderDisplayName ?? "MDMSv2 System"
            };
        }

        // Custom SMTP hoặc Gmail
        var smtpLogger = _loggerFactory.CreateLogger<SmtpEmailProvider>();
        return new SmtpEmailProvider(smtpLogger)
        {
            ProviderType = config.ProviderType ?? Constants.EmailProvider.CustomSmtp,
            SmtpHost = config.SmtpHost ?? "smtp.gmail.com",
            SmtpPort = config.SmtpPort ?? 587,
            SecurityMode = config.SecurityMode ?? "StartTls",
            RequireAuth = config.RequireAuth,
            SmtpUsername = config.SmtpUsername ?? string.Empty,
            SmtpPassword = config.EncryptedSmtpPassword ?? string.Empty,
            SenderEmail = config.SenderEmail ?? string.Empty,
            SenderDisplayName = config.SenderDisplayName ?? "MDMSv2 System"
        };
    }

    public IEmailProviderService CreateProviderFromTestRequest(TestEmailConfigRequest request, Domain.Entities.SystemEmailConfigEntity? existingConfig)
    {
        var providerType = request.ProviderType ?? existingConfig?.ProviderType ?? Constants.EmailProvider.MicrosoftGraph;

        if (string.Equals(providerType, Constants.EmailProvider.MicrosoftGraph, StringComparison.OrdinalIgnoreCase))
        {
            var client = _httpClientFactory.CreateClient("MicrosoftGraphClient");
            var logger = _loggerFactory.CreateLogger<MicrosoftGraphEmailProvider>();
            return new MicrosoftGraphEmailProvider(client, _memoryCache, logger)
            {
                TenantId = request.TenantId ?? existingConfig?.TenantId ?? string.Empty,
                ClientId = request.ClientId ?? existingConfig?.ClientId ?? string.Empty,
                ClientSecret = !string.IsNullOrWhiteSpace(request.ClientSecret) ? request.ClientSecret : existingConfig?.EncryptedClientSecret ?? string.Empty,
                SenderEmail = request.SenderEmail ?? existingConfig?.SenderEmail ?? string.Empty,
                SenderDisplayName = request.SenderDisplayName ?? existingConfig?.SenderDisplayName ?? "MDMSv2 Test"
            };
        }

        var smtpLogger = _loggerFactory.CreateLogger<SmtpEmailProvider>();
        return new SmtpEmailProvider(smtpLogger)
        {
            ProviderType = providerType,
            SmtpHost = request.SmtpHost ?? existingConfig?.SmtpHost ?? "smtp.gmail.com",
            SmtpPort = request.SmtpPort ?? existingConfig?.SmtpPort ?? 587,
            SecurityMode = request.SecurityMode ?? existingConfig?.SecurityMode ?? "StartTls",
            RequireAuth = request.RequireAuth ?? existingConfig?.RequireAuth ?? true,
            SmtpUsername = request.SmtpUsername ?? existingConfig?.SmtpUsername ?? string.Empty,
            SmtpPassword = !string.IsNullOrWhiteSpace(request.SmtpPassword) ? request.SmtpPassword : existingConfig?.EncryptedSmtpPassword ?? string.Empty,
            SenderEmail = request.SenderEmail ?? existingConfig?.SenderEmail ?? string.Empty,
            SenderDisplayName = request.SenderDisplayName ?? existingConfig?.SenderDisplayName ?? "MDMSv2 Test"
        };
    }

    private IEmailProviderService CreateFallbackProvider()
    {
        var smtpLogger = _loggerFactory.CreateLogger<SmtpEmailProvider>();
        var host = _configuration["Email:Host"] ?? _configuration["SendMail:EmailHost"] ?? "smtp.gmail.com";
        var portStr = _configuration["Email:Port"] ?? "587";
        int.TryParse(portStr, out int port);
        if (port <= 0) port = 587;

        var sender = _configuration["Email:Sender"] ?? _configuration["SendMail:EmailUsername"] ?? "noreply@mdms.vn";
        var password = _configuration["Email:Password"] ?? _configuration["SendMail:EmailPassword"] ?? string.Empty;
        var displayName = _configuration["Email:DisplayName"] ?? "MDMSv2 System";

        return new SmtpEmailProvider(smtpLogger)
        {
            ProviderType = Constants.EmailProvider.Gmail,
            SmtpHost = host,
            SmtpPort = port,
            SecurityMode = "StartTls",
            RequireAuth = true,
            SmtpUsername = sender,
            SmtpPassword = password,
            SenderEmail = sender,
            SenderDisplayName = displayName
        };
    }
}
