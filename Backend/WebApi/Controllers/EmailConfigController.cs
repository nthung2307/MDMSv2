using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Requests.EmailConfig;
using Application.DataTransferObjects.Responses.EmailConfig;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Infrastructure.Services.EmailProviders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

[ApiController]
[Area(Common.Url.Areas.EmailConfig)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.EmailConfig)]
public class EmailConfigController : ControllerBase
{
    private readonly ISystemEmailConfigReadWriteRepository _configReadWriteRepository;
    private readonly ISystemEmailConfigReadOnlyRepository _configReadOnlyRepository;
    private readonly IEmailProviderFactory _emailProviderFactory;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailConfigController> _logger;

    public EmailConfigController(
        ISystemEmailConfigReadWriteRepository configReadWriteRepository,
        ISystemEmailConfigReadOnlyRepository configReadOnlyRepository,
        IEmailProviderFactory emailProviderFactory,
        ISnowflakeIdService snowflakeIdService,
        IConfiguration configuration,
        ILogger<EmailConfigController> logger)
    {
        _configReadWriteRepository = configReadWriteRepository;
        _configReadOnlyRepository = configReadOnlyRepository;
        _emailProviderFactory = emailProviderFactory;
        _snowflakeIdService = snowflakeIdService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    [Route(Common.Url.Default.EmailConfig.GetActive)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<SystemEmailConfigResponse>> GetActiveEmailConfigAsync(CancellationToken cancellationToken)
    {
        try
        {
            Domain.Entities.SystemEmailConfigEntity? config = null;
            try
            {
                config = await _configReadOnlyRepository.GetActiveConfigAsync(cancellationToken);
            }
            catch (Exception dbEx)
            {
                _logger.LogWarning(dbEx, "Chưa thể truy vấn bảng cấu hình email trong CSDL (có thể chưa chạy script migration). Đang fallback sang appsettings.");
            }

            if (config == null)
            {
                // Trả về cấu hình mặc định từ appsettings
                var defaultHost = _configuration["Email:Host"] ?? _configuration["SendMail:EmailHost"] ?? "smtp.gmail.com";
                var defaultSender = _configuration["Email:Sender"] ?? _configuration["SendMail:EmailUsername"] ?? string.Empty;
                var defaultDisplayName = _configuration["Email:DisplayName"] ?? "MDMSv2 System";
                var hasPassword = !string.IsNullOrWhiteSpace(_configuration["Email:Password"] ?? _configuration["SendMail:EmailPassword"]);

                return RequestResult<SystemEmailConfigResponse>.Succeed(new SystemEmailConfigResponse
                {
                    Id = 0,
                    ProviderType = Constants.EmailProvider.Gmail,
                    SenderEmail = defaultSender,
                    SenderDisplayName = defaultDisplayName,
                    SmtpHost = defaultHost,
                    SmtpPort = 587,
                    SecurityMode = "StartTls",
                    RequireAuth = true,
                    SmtpUsername = defaultSender,
                    HasSmtpPassword = hasPassword,
                    IsActive = true,
                    CreatedTime = DateTimeOffset.UtcNow
                });
            }

            return RequestResult<SystemEmailConfigResponse>.Succeed(new SystemEmailConfigResponse
            {
                Id = config.Id,
                ProviderType = config.ProviderType,
                SenderEmail = config.SenderEmail,
                SenderDisplayName = config.SenderDisplayName,
                TenantId = config.TenantId,
                ClientId = config.ClientId,
                HasClientSecret = !string.IsNullOrWhiteSpace(config.EncryptedClientSecret),
                SmtpHost = config.SmtpHost,
                SmtpPort = config.SmtpPort,
                SecurityMode = config.SecurityMode,
                RequireAuth = config.RequireAuth,
                SmtpUsername = config.SmtpUsername,
                HasSmtpPassword = !string.IsNullOrWhiteSpace(config.EncryptedSmtpPassword),
                IsActive = config.IsActive,
                CreatedTime = config.CreatedTime,
                ModifiedTime = config.ModifiedTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy cấu hình email active");
            return RequestResult<SystemEmailConfigResponse>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "SystemEmailConfig",
                    Error = "Có lỗi xảy ra khi đọc cấu hình email từ hệ thống."
                }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.EmailConfig.Update)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<SystemEmailConfigResponse>> UpdateEmailConfigAsync(
        [FromBody] UpdateSystemEmailConfigRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _configReadWriteRepository.EnsureTableCreatedAsync(cancellationToken);
            var isNew = false;
            var existingConfig = await _configReadWriteRepository.GetActiveConfigAsync(cancellationToken);
            if (existingConfig == null)
            {
                isNew = true;
                var newId = await _snowflakeIdService.GenerateId(cancellationToken);
                existingConfig = new SystemEmailConfigEntity
                {
                    Id = newId,
                    CreatedTime = DateTimeOffset.UtcNow
                };
            }

            existingConfig.ProviderType = request.ProviderType ?? Constants.EmailProvider.MicrosoftGraph;
            existingConfig.SenderEmail = request.SenderEmail?.Trim() ?? string.Empty;
            existingConfig.SenderDisplayName = request.SenderDisplayName?.Trim() ?? string.Empty;

            // Microsoft Graph
            existingConfig.TenantId = request.TenantId?.Trim();
            existingConfig.ClientId = request.ClientId?.Trim();
            if (!string.IsNullOrWhiteSpace(request.ClientSecret))
            {
                existingConfig.EncryptedClientSecret = request.ClientSecret.Trim();
            }

            // Custom SMTP / Gmail
            existingConfig.SmtpHost = request.SmtpHost?.Trim();
            existingConfig.SmtpPort = request.SmtpPort;
            existingConfig.SecurityMode = request.SecurityMode?.Trim() ?? "StartTls";
            existingConfig.RequireAuth = request.RequireAuth;
            existingConfig.SmtpUsername = request.SmtpUsername?.Trim();
            if (!string.IsNullOrWhiteSpace(request.SmtpPassword))
            {
                existingConfig.EncryptedSmtpPassword = request.SmtpPassword.Trim();
            }

            existingConfig.IsActive = request.IsActive;
            existingConfig.ModifiedTime = DateTimeOffset.UtcNow;

            if (isNew)
            {
                await _configReadWriteRepository.AddAsync(existingConfig, cancellationToken);
            }
            else
            {
                await _configReadWriteRepository.UpdateAsync(existingConfig, cancellationToken);
            }

            await _configReadWriteRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cập nhật cấu hình email thành công: Provider={ProviderType}, Sender={SenderEmail}", 
                existingConfig.ProviderType, existingConfig.SenderEmail);

            return RequestResult<SystemEmailConfigResponse>.Succeed(new SystemEmailConfigResponse
            {
                Id = existingConfig.Id,
                ProviderType = existingConfig.ProviderType,
                SenderEmail = existingConfig.SenderEmail,
                SenderDisplayName = existingConfig.SenderDisplayName,
                TenantId = existingConfig.TenantId,
                ClientId = existingConfig.ClientId,
                HasClientSecret = !string.IsNullOrWhiteSpace(existingConfig.EncryptedClientSecret),
                SmtpHost = existingConfig.SmtpHost,
                SmtpPort = existingConfig.SmtpPort,
                SecurityMode = existingConfig.SecurityMode,
                RequireAuth = existingConfig.RequireAuth,
                SmtpUsername = existingConfig.SmtpUsername,
                HasSmtpPassword = !string.IsNullOrWhiteSpace(existingConfig.EncryptedSmtpPassword),
                IsActive = existingConfig.IsActive,
                CreatedTime = existingConfig.CreatedTime,
                ModifiedTime = existingConfig.ModifiedTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật cấu hình email: {Message}", ex.Message);
            return RequestResult<SystemEmailConfigResponse>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "SystemEmailConfig",
                    Error = $"Lưu cấu hình thất bại: {ex.Message}"
                }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.EmailConfig.TestConnection)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<bool>> TestEmailConnectionAsync(
        [FromBody] TestEmailConfigRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.TestRecipientEmail))
            {
                return RequestResult<bool>.Fail(new[]
                {
                    new ErrorItem
                    {
                        FieldName = nameof(request.TestRecipientEmail),
                        Error = "Vui lòng nhập địa chỉ email nhận thư kiểm tra."
                    }
                });
            }

            SystemEmailConfigEntity? existingConfig = null;
            try
            {
                existingConfig = await _configReadOnlyRepository.GetActiveConfigAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Không thể đọc cấu hình email từ CSDL khi kiểm tra kết nối.");
            }

            if (_emailProviderFactory is EmailProviderFactory concreteFactory)
            {
                var provider = concreteFactory.CreateProviderFromTestRequest(request, existingConfig);
                var isSuccess = await provider.TestConnectionAsync(request.TestRecipientEmail.Trim(), cancellationToken);
                return RequestResult<bool>.Succeed(isSuccess);
            }

            // Fallback nếu không ép kiểu được
            var activeProvider = await _emailProviderFactory.GetActiveProviderAsync(cancellationToken);
            var result = await activeProvider.TestConnectionAsync(request.TestRecipientEmail.Trim(), cancellationToken);
            return RequestResult<bool>.Succeed(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kiểm tra kết nối email thất bại: {Message}", ex.Message);
            return RequestResult<bool>.Fail(new[]
            {
                new ErrorItem
                {
                    FieldName = "Connection",
                    Error = $"Gửi email kiểm tra thất bại: {ex.Message}"
                }
            });
        }
    }
}
