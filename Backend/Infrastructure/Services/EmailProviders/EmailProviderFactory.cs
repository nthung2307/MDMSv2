using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
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

public static class EmailSecretCryptoHelper
{
    private const string Prefix = "ENC:v1:";
    private const string DefaultKeyMaterial = "MDMSv2-EmailSecret-DefaultKey-2026!@#";

    private static byte[] DeriveKey(string? secretKey)
    {
        var raw = string.IsNullOrWhiteSpace(secretKey) ? DefaultKeyMaterial : secretKey;
        return SHA256.HashData(Encoding.UTF8.GetBytes(raw));
    }

    public static string Encrypt(string? plainText, string? secretKey = null)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return string.Empty;

        if (plainText.StartsWith(Prefix, StringComparison.Ordinal))
            return plainText;

        using var aes = Aes.Create();
        aes.Key = DeriveKey(secretKey);
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var combined = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, combined, aes.IV.Length, cipherBytes.Length);

        return Prefix + Convert.ToBase64String(combined);
    }

    public static string Decrypt(string? storedValue, string? secretKey = null)
    {
        if (string.IsNullOrWhiteSpace(storedValue))
            return string.Empty;

        if (!storedValue.StartsWith(Prefix, StringComparison.Ordinal))
            return storedValue; // Backward-compatible fallback for legacy plaintext values

        try
        {
            var payloadBase64 = storedValue.Substring(Prefix.Length);
            var combined = Convert.FromBase64String(payloadBase64);
            if (combined.Length <= 16)
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = DeriveKey(secretKey);

            var iv = new byte[16];
            var cipherBytes = new byte[combined.Length - 16];
            Buffer.BlockCopy(combined, 0, iv, 0, 16);
            Buffer.BlockCopy(combined, 16, cipherBytes, 0, cipherBytes.Length);
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            return storedValue;
        }
    }
}

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
        var cryptoKey = _configuration["JwtConfigurations:SymmetricSecurityKey"];
        if (string.Equals(config.ProviderType, Constants.EmailProvider.MicrosoftGraph, StringComparison.OrdinalIgnoreCase))
        {
            var client = _httpClientFactory.CreateClient("MicrosoftGraphClient");
            var logger = _loggerFactory.CreateLogger<MicrosoftGraphEmailProvider>();
            return new MicrosoftGraphEmailProvider(client, _memoryCache, logger)
            {
                TenantId = config.TenantId ?? string.Empty,
                ClientId = config.ClientId ?? string.Empty,
                ClientSecret = EmailSecretCryptoHelper.Decrypt(config.EncryptedClientSecret, cryptoKey),
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
            SmtpPassword = EmailSecretCryptoHelper.Decrypt(config.EncryptedSmtpPassword, cryptoKey),
            SenderEmail = config.SenderEmail ?? string.Empty,
            SenderDisplayName = config.SenderDisplayName ?? "MDMSv2 System"
        };
    }

    public IEmailProviderService CreateProviderFromTestRequest(TestEmailConfigRequest request, Domain.Entities.SystemEmailConfigEntity? existingConfig)
    {
        var cryptoKey = _configuration["JwtConfigurations:SymmetricSecurityKey"];
        var providerType = request.ProviderType ?? existingConfig?.ProviderType ?? Constants.EmailProvider.MicrosoftGraph;

        if (string.Equals(providerType, Constants.EmailProvider.MicrosoftGraph, StringComparison.OrdinalIgnoreCase))
        {
            var client = _httpClientFactory.CreateClient("MicrosoftGraphClient");
            var logger = _loggerFactory.CreateLogger<MicrosoftGraphEmailProvider>();
            return new MicrosoftGraphEmailProvider(client, _memoryCache, logger)
            {
                TenantId = request.TenantId ?? existingConfig?.TenantId ?? string.Empty,
                ClientId = request.ClientId ?? existingConfig?.ClientId ?? string.Empty,
                ClientSecret = !string.IsNullOrWhiteSpace(request.ClientSecret)
                    ? request.ClientSecret
                    : EmailSecretCryptoHelper.Decrypt(existingConfig?.EncryptedClientSecret, cryptoKey),
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
            SmtpPassword = !string.IsNullOrWhiteSpace(request.SmtpPassword)
                ? request.SmtpPassword
                : EmailSecretCryptoHelper.Decrypt(existingConfig?.EncryptedSmtpPassword, cryptoKey),
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
