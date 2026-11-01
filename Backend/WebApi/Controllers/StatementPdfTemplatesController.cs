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
[Area(Common.Url.Areas.StatementPdfTemplates)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.StatementPdfTemplates)]
public class StatementPdfTemplatesController : ControllerBase
{
    private readonly IStatementPdfTemplateReadOnlyRepository _readOnlyRepository;
    private readonly IStatementPdfTemplateReadWriteRepository _readWriteRepository;
    private readonly IStatementPdfGenerator _pdfGenerator;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IMapper _mapper;
    private readonly ILogger<StatementPdfTemplatesController> _logger;

    public StatementPdfTemplatesController(
        IStatementPdfTemplateReadOnlyRepository readOnlyRepository,
        IStatementPdfTemplateReadWriteRepository readWriteRepository,
        IStatementPdfGenerator pdfGenerator,
        ISnowflakeIdService snowflakeIdService,
        IMapper mapper,
        ILogger<StatementPdfTemplatesController> logger)
    {
        _readOnlyRepository = readOnlyRepository;
        _readWriteRepository = readWriteRepository;
        _pdfGenerator = pdfGenerator;
        _snowflakeIdService = snowflakeIdService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [Route(Common.Url.Default.StatementPdfTemplates.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<List<StatementPdfTemplateDto>>> GetAllTemplatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var list = await _readOnlyRepository.GetAllTemplatesAsync(cancellationToken);
            return RequestResult<List<StatementPdfTemplateDto>>.Succeed(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách mẫu phiếu báo PDF: {Message}", ex.Message);
            return RequestResult<List<StatementPdfTemplateDto>>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.StatementPdfTemplates.View)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<StatementPdfTemplateDto>> GetDetailAsync([FromRoute] long id, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _readOnlyRepository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                return RequestResult<StatementPdfTemplateDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy mẫu phiếu báo PDF." }
                });
            }

            return RequestResult<StatementPdfTemplateDto>.Succeed(_mapper.Map<StatementPdfTemplateDto>(entity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy chi tiết mẫu phiếu báo {Id}: {Message}", id, ex.Message);
            return RequestResult<StatementPdfTemplateDto>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.StatementPdfTemplates.Create)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<StatementPdfTemplateDto>> CreateTemplateAsync([FromBody] CreateStatementPdfTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
            {
                return RequestResult<StatementPdfTemplateDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Validation", Error = "Tên mẫu và mã mẫu không được để trống." }
                });
            }

            var entity = _mapper.Map<StatementPdfTemplateEntity>(request);
            entity.Id = await _snowflakeIdService.GenerateId(cancellationToken);
            entity.Code = entity.Code.Trim().ToUpperInvariant();
            entity.CreatedTime = DateTimeOffset.UtcNow;

            if (entity.IsDefault)
            {
                await _readWriteRepository.SetDefaultAsync(entity.Id, cancellationToken);
            }

            await _readWriteRepository.AddAsync(entity, cancellationToken);
            await _readWriteRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Tạo mới thành công mẫu phiếu báo PDF {Code} (Id = {Id})", entity.Code, entity.Id);
            return RequestResult<StatementPdfTemplateDto>.Succeed(_mapper.Map<StatementPdfTemplateDto>(entity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo mới mẫu phiếu báo PDF: {Message}", ex.Message);
            return RequestResult<StatementPdfTemplateDto>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.StatementPdfTemplates.Update)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<StatementPdfTemplateDto>> UpdateTemplateAsync([FromRoute] long id, [FromBody] UpdateStatementPdfTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _readWriteRepository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                return RequestResult<StatementPdfTemplateDto>.Fail(new[]
                {
                    new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy mẫu phiếu báo PDF cần cập nhật." }
                });
            }

            entity.Name = request.Name;
            entity.HeaderTitle = request.HeaderTitle;
            entity.SubHeaderTitle = request.SubHeaderTitle;
            entity.CompanyName = request.CompanyName;
            entity.CompanyAddress = request.CompanyAddress;
            entity.ContactPhone = request.ContactPhone;
            entity.LogoBase64 = request.LogoBase64;
            entity.PaperSize = request.PaperSize;
            entity.DisplayColumnsJson = request.DisplayColumnsJson;
            entity.FooterNotes = request.FooterNotes;
            entity.RequireDualSignatures = request.RequireDualSignatures;
            entity.IsActive = request.IsActive;
            entity.ModifiedTime = DateTimeOffset.UtcNow;

            if (request.IsDefault)
            {
                await _readWriteRepository.SetDefaultAsync(id, cancellationToken);
                entity.IsDefault = true;
            }
            else
            {
                entity.IsDefault = false;
            }

            await _readWriteRepository.UpdateAsync(entity, cancellationToken);
            await _readWriteRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cập nhật thành công mẫu phiếu báo PDF {Code} (Id = {Id})", entity.Code, id);
            return RequestResult<StatementPdfTemplateDto>.Succeed(_mapper.Map<StatementPdfTemplateDto>(entity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật mẫu phiếu báo PDF {Id}: {Message}", id, ex.Message);
            return RequestResult<StatementPdfTemplateDto>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpDelete]
    [Route(Common.Url.Default.StatementPdfTemplates.Delete)]
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
                    new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy mẫu phiếu báo cần xóa." }
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

            _logger.LogInformation("Xóa thành công mẫu phiếu báo PDF Id = {Id}", id);
            return RequestResult<bool>.Succeed(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xóa mẫu phiếu báo PDF {Id}: {Message}", id, ex.Message);
            return RequestResult<bool>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.StatementPdfTemplates.SetDefault)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<bool>> SetDefaultAsync([FromRoute] long id, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _readWriteRepository.GetByIdAsync(id, cancellationToken);
            if (entity == null)
            {
                return RequestResult<bool>.Fail(new[]
                {
                    new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy mẫu phiếu báo." }
                });
            }

            await _readWriteRepository.SetDefaultAsync(id, cancellationToken);
            await _readWriteRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Đặt làm mẫu phiếu báo mặc định thành công (Id = {Id})", id);
            return RequestResult<bool>.Succeed(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thiết lập mẫu phiếu báo mặc định {Id}: {Message}", id, ex.Message);
            return RequestResult<bool>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = ex.Message }
            });
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.StatementPdfTemplates.PreviewPdf)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> PreviewPdfAsync([FromBody] PreviewStatementPdfTemplateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfBytes = await _pdfGenerator.GeneratePreviewPdfAsync(request, cancellationToken);
            return File(pdfBytes, "application/pdf", "preview_statement.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi render xem trước file PDF: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }
}
