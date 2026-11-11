using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Templates;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

[ApiController]
[Area(Common.Url.Areas.NotificationTemplates)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.NotificationTemplates)]
public class NotificationTemplatesController : ControllerBase
{
    private readonly INotificationTemplateReadOnlyRepository _readOnlyRepository;
    private readonly INotificationTemplateReadWriteRepository _readWriteRepository;
    private readonly INotificationTemplateService _templateService;
    private readonly IEmailProviderFactory _emailProviderFactory;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationTemplatesController> _logger;

    public NotificationTemplatesController(
        INotificationTemplateReadOnlyRepository readOnlyRepository,
        INotificationTemplateReadWriteRepository readWriteRepository,
        INotificationTemplateService templateService,
        IEmailProviderFactory emailProviderFactory,
        ISnowflakeIdService snowflakeIdService,
        IMapper mapper,
        ILogger<NotificationTemplatesController> logger)
    {
        _readOnlyRepository = readOnlyRepository;
        _readWriteRepository = readWriteRepository;
        _templateService = templateService;
        _emailProviderFactory = emailProviderFactory;
        _snowflakeIdService = snowflakeIdService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [Route(Common.Url.Default.NotificationTemplates.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<List<NotificationTemplateDto>>> GetAllTemplatesAsync([FromQuery] string? code, CancellationToken cancellationToken)
    {
        try
        {
            var list = await _readOnlyRepository.GetAllTemplatesAsync(code, cancellationToken);
            if (list.Count == 0 && string.IsNullOrEmpty(code))
            {
                await SeedDefaultTemplatesAsync(cancellationToken);
                list = await _readOnlyRepository.GetAllTemplatesAsync(code, cancellationToken);
            }
            else if (list.GroupBy(x => (x.Code ?? string.Empty).Trim().ToUpperInvariant())
                .Any(g => g.Count(t => t.IsDefault) > 1 || g.GroupBy(t => (t.Name ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase).Any(ng => ng.Count() > 1)))
            {
                var removed = await _readWriteRepository.DeduplicateTemplatesAsync(cancellationToken);
                if (removed > 0)
                {
                    _logger.LogInformation("Đã tự động dọn {Count} mẫu email mặc định bị trùng lặp trong CSDL.", removed);
                    list = await _readOnlyRepository.GetAllTemplatesAsync(code, cancellationToken);
                }
            }

            foreach (var t in list)
            {
                t.BodyHtmlTemplate = Infrastructure.Services.Templates.NotificationTemplateService.CleanLegacyFields(t.BodyHtmlTemplate);
            }

            return RequestResult<List<NotificationTemplateDto>>.Succeed(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách mẫu thông báo: {Message}", ex.Message);
            return RequestResult<List<NotificationTemplateDto>>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.NotificationTemplates.ResetDefaults)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<bool>> ResetDefaultsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SeedDefaultTemplatesAsync(cancellationToken);
            return RequestResult<bool>.Succeed(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi khôi phục mẫu email chuẩn: {Message}", ex.Message);
            return RequestResult<bool>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    private async Task SeedDefaultTemplatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var prebuilts = Infrastructure.Services.Templates.NotificationTemplateService.GetPrebuiltDefaultTemplates();
            var existingTemplates = await _readOnlyRepository.GetAllTemplatesAsync(null, cancellationToken);
            foreach (var template in prebuilts)
            {
                var existing = existingTemplates.Find(x => x.Code == template.Code && x.IsDefault)
                               ?? existingTemplates.Find(x => x.Code == template.Code);
                if (existing != null)
                {
                    var entity = await _readOnlyRepository.GetByIdAsync(existing.Id, cancellationToken);
                    if (entity != null)
                    {
                        entity.Name = template.Name;
                        entity.SubjectTemplate = template.SubjectTemplate;
                        entity.BodyHtmlTemplate = template.BodyHtmlTemplate;
                        entity.Description = template.Description;
                        entity.ModifiedTime = DateTimeOffset.UtcNow;
                        await _readWriteRepository.UpdateAsync(entity, cancellationToken);
                        continue;
                    }
                }

                template.Id = await _snowflakeIdService.GenerateId(cancellationToken);
                template.CreatedTime = DateTimeOffset.UtcNow;
                if (template.IsDefault)
                {
                    await _readWriteRepository.SetDefaultAsync(template.Id, template.Code, cancellationToken);
                }
                await _readWriteRepository.AddAsync(template, cancellationToken);
            }
            await _readWriteRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Khởi tạo và đồng bộ thành công {Count} mẫu email chuẩn", prebuilts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo mẫu email chuẩn: {Message}", ex.Message);
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.NotificationTemplates.View)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<NotificationTemplateDto>> GetDetailAsync([FromRoute] long id, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _readOnlyRepository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                return RequestResult<NotificationTemplateDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy mẫu thông báo với mã yêu cầu." }
                });
            }

            return RequestResult<NotificationTemplateDto>.Succeed(_mapper.Map<NotificationTemplateDto>(entity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết mẫu thông báo {Id}: {Message}", id, ex.Message);
            return RequestResult<NotificationTemplateDto>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.NotificationTemplates.Create)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<NotificationTemplateDto>> CreateTemplateAsync([FromBody] CreateNotificationTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.BodyHtmlTemplate))
            {
                return RequestResult<NotificationTemplateDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Validation", Error = "Tên mẫu, mã nghiệp vụ và nội dung HTML không được để trống." }
                });
            }

            var entity = _mapper.Map<NotificationTemplateEntity>(request);
            entity.Id = await _snowflakeIdService.GenerateId(cancellationToken);
            entity.Code = entity.Code.Trim().ToUpperInvariant();
            entity.CreatedTime = DateTimeOffset.UtcNow;

            if (entity.IsDefault)
            {
                await _readWriteRepository.SetDefaultAsync(entity.Id, entity.Code, cancellationToken);
            }

            await _readWriteRepository.AddAsync(entity, cancellationToken);
            await _readWriteRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tạo mới thành công mẫu email {Code} (Id = {Id})", entity.Code, entity.Id);
            return RequestResult<NotificationTemplateDto>.Succeed(_mapper.Map<NotificationTemplateDto>(entity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo mới mẫu email: {Message}", ex.Message);
            return RequestResult<NotificationTemplateDto>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.NotificationTemplates.Update)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<NotificationTemplateDto>> UpdateTemplateAsync([FromRoute] long id, [FromBody] UpdateNotificationTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _readWriteRepository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                return RequestResult<NotificationTemplateDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy mẫu thông báo cần cập nhật." }
                });
            }

            entity.Name = request.Name;
            entity.SubjectTemplate = request.SubjectTemplate;
            entity.BodyHtmlTemplate = request.BodyHtmlTemplate;
            entity.Description = request.Description;
            entity.IsActive = request.IsActive;
            entity.ModifiedTime = DateTimeOffset.UtcNow;

            if (request.IsDefault)
            {
                await _readWriteRepository.SetDefaultAsync(id, entity.Code, cancellationToken);
                entity.IsDefault = true;
            }
            else
            {
                entity.IsDefault = false;
            }

            await _readWriteRepository.UpdateAsync(entity, cancellationToken);
            await _readWriteRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cập nhật thành công mẫu email {Code} (Id = {Id})", entity.Code, id);
            return RequestResult<NotificationTemplateDto>.Succeed(_mapper.Map<NotificationTemplateDto>(entity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật mẫu email {Id}: {Message}", id, ex.Message);
            return RequestResult<NotificationTemplateDto>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpDelete]
    [Route(Common.Url.Default.NotificationTemplates.Delete)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<bool>> DeleteTemplateAsync([FromRoute] long id, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _readWriteRepository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                return RequestResult<bool>.Fail(new[]
                {
                    new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy mẫu thông báo cần xóa." }
                });
            }

            if (entity.IsDefault)
            {
                return RequestResult<bool>.Fail(new[]
                {
                    new ErrorItem { FieldName = nameof(id), Error = "Không thể xóa mẫu mặc định đang được hệ thống sử dụng." }
                });
            }

            await _readWriteRepository.RemoveAsync(entity, cancellationToken);
            await _readWriteRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Xóa thành công mẫu email Id = {Id}", id);
            return RequestResult<bool>.Succeed(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa mẫu email {Id}: {Message}", id, ex.Message);
            return RequestResult<bool>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.NotificationTemplates.SetDefault)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<bool>> SetDefaultAsync([FromRoute] long id, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _readWriteRepository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                return RequestResult<bool>.Fail(new[]
                {
                    new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy mẫu thông báo." }
                });
            }

            await _readWriteRepository.SetDefaultAsync(id, entity.Code, cancellationToken);
            await _readWriteRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Đặt làm mẫu mặc định thành công cho mã {Code} (Id = {Id})", entity.Code, id);
            return RequestResult<bool>.Succeed(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thiết lập mẫu mặc định {Id}: {Message}", id, ex.Message);
            return RequestResult<bool>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.NotificationTemplates.Preview)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<PreviewNotificationTemplateResponse>> PreviewTemplateAsync([FromBody] PreviewNotificationTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _templateService.PreviewAsync(request, cancellationToken);
            return RequestResult<PreviewNotificationTemplateResponse>.Succeed(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xem trước mẫu thông báo: {Message}", ex.Message);
            return RequestResult<PreviewNotificationTemplateResponse>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.NotificationTemplates.TestSend)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<bool>> TestSendTemplateAsync([FromBody] TestSendNotificationTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ToEmail))
            {
                return RequestResult<bool>.Fail(new[]
                {
                    new ErrorItem { FieldName = nameof(request.ToEmail), Error = "Vui lòng nhập địa chỉ email nhận kiểm tra." }
                });
            }

            var previewResult = await _templateService.PreviewAsync(new PreviewNotificationTemplateRequest
            {
                Code = request.Code,
                SubjectTemplate = request.SubjectTemplate ?? string.Empty,
                BodyHtmlTemplate = request.BodyHtmlTemplate ?? string.Empty
            }, cancellationToken);
            var subject = previewResult.Subject;
            var bodyHtml = previewResult.BodyHtml;

            var provider = await _emailProviderFactory.GetActiveProviderAsync();
            await provider.SendEmailAsync(new Application.DataTransferObjects.Email.EmailMessageDto
            {
                To = new List<string> { request.ToEmail.Trim() },
                Subject = !string.IsNullOrWhiteSpace(subject) ? subject : "[MDMS Test] Thử nghiệm mẫu email",
                BodyHtml = bodyHtml
            });

            _logger.LogInformation("Gửi email thử nghiệm thành công tới {ToEmail} cho mã {Code}", request.ToEmail, request.Code);
            return RequestResult<bool>.Succeed(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email thử nghiệm mẫu: {Message}", ex.Message);
            return RequestResult<bool>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }
}
