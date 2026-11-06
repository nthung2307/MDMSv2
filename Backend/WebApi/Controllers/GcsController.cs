using Application.Commands.Gcs;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Responses.Gcs;
using Application.Queries.Gcs;
using Application.Repositories.ReadWrite;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.Logger.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

[ApiController]
[Area(Common.Url.Areas.Gcs)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.Gcs)]
public class GcsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;
    private readonly IGcsReadWriteRepository _gcsReadWriteRepository;
    private readonly IGcsTemporaryReadWriteRepository _gcsTemporaryReadWriteRepository;

    public GcsController(IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService, IGcsReadWriteRepository gcsReadWriteRepository, IGcsTemporaryReadWriteRepository gcsTemporaryReadWriteRepository)
    {
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _gcsReadWriteRepository = gcsReadWriteRepository;
        _gcsTemporaryReadWriteRepository = gcsTemporaryReadWriteRepository;
    }

    [HttpPost]
    [Route(Common.Url.Default.Gcs.Create)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<GcsResponse>> CreateGcsAsync(CreateGcsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var createGcsCommand = new CreateGcsCommand()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(createGcsCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(CreateGcsAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Gcs.Update)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<GcsResponse>> UpdateGcsAsync(UpdateGcsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updateGcsCommand = new UpdateGcsCommand()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(updateGcsCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(UpdateGcsAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Gcs.ChangeStatus)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<GcsResponse>> ChangeStatusGcsAsync(ChangeStatusGcsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updateGcsCommand = new ChangeStatusGcsCommand()
            {
                Id = request.Id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(updateGcsCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ChangeStatusGcsAsync));
            throw;
        }
    }

    [HttpDelete]
    [Route(Common.Url.Default.Gcs.Delete)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<GcsResponse>> DeleteGcsAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var deleteGcsCommand = new DeleteGcsCommand()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(deleteGcsCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(DeleteGcsAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Gcs.FinalMeterReading)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<GcsResponse>> FinalMeterReadingGcsAsync(FinalMeterReadingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var finalMeterReadingCommand = new FinalMeterReadingCommand()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(finalMeterReadingCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(FinalMeterReadingGcsAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Gcs.UpdateGcsTemporary)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<GcsResponse>> UpdateGcsTemporaryAsync(UpdateListGcsTemporaryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var finalMeterReadingCommand = new UpdateGcsTemporaryCommand()
            {
                Request = request.GcsTemporaryRequests,
                GcsId = request.GcsId,
                Month = request.Month,
                Year = request.Year,
                PeriodNumber = request.PeriodNumber,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(finalMeterReadingCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(FinalMeterReadingGcsAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Gcs.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<CursorPaginationResponse<GcsDto>>> GetListGcsAsync(GetListGcsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListGcsQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var gcs = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<GcsDto>>.Succeed(gcs);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListGcsAsync));
            throw;
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.Gcs.View)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<GcsDto>> GetGcsDetailAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetGcsQuery()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var gcs = await _mediator.Send(query, cancellationToken);
            return RequestResult<GcsDto>.Succeed(gcs);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetGcsDetailAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Gcs.ExportPdf)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfGcsAsync(ExportPdfAndExcelRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _gcsReadWriteRepository.GenerateGcsPdf(request, cancellationToken);
            return File(pdfData, "application/pdf", $"Export-GCS-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfGcsAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Gcs.ExportCsv)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvGcsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _gcsReadWriteRepository.GenerateGcsCsv(cancellationToken);
            return File(
                fileContents: csvData,
                contentType: "application/octet-stream; charset=utf-8",
                fileDownloadName: $"Export-Gcs-{DateTime.Now:s}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvGcsAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Gcs.FinalExportPdf)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfFinalAsync(ExportPdfAndExcelGcsFinalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _gcsReadWriteRepository.GenerateGcsFinalPdf(request, cancellationToken);
            return File(pdfData, "application/pdf", $"Export-Final-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfFinalAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Gcs.FinalExportCsv)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvGcsFinalAsync(ExportPdfAndExcelGcsFinalRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _gcsReadWriteRepository.GenerateGcsFinalCsv(request, cancellationToken);
            return File(
                fileContents: csvData,
                contentType: "application/octet-stream; charset=utf-8",
                fileDownloadName: $"Export-Gcs-{DateTime.Now:s}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvGcsFinalAsync));
            throw;
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.Gcs.ExportExcelGcsTemporary)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportExcelGcsTemporaryAsync([FromQuery] long gcsId, [FromQuery] int month, [FromQuery] int year, [FromQuery] int periodNumber, [FromQuery] bool? onlySuspicious, CancellationToken cancellationToken)
    {
        try
        {
            var excelData = await _gcsTemporaryReadWriteRepository.ExportGcsTemporaryExcelAsync(gcsId, month, year, periodNumber, onlySuspicious, cancellationToken);
            var fileName = $"MDMS_SoGCS_Ky{periodNumber}_T{month:D2}_{year}.xlsx";
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportExcelGcsTemporaryAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Gcs.ImportExcelGcsTemporary)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<ImportGcsTemporaryExcelResponse>> ImportExcelGcsTemporaryAsync([FromForm] IFormFile file, [FromQuery] long gcsId, [FromQuery] int month, [FromQuery] int year, [FromQuery] int periodNumber, CancellationToken cancellationToken)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return RequestResult<ImportGcsTemporaryExcelResponse>.Fail(new[]
                {
                    new ErrorItem { FieldName = "file", Error = "Vui lòng chọn file Excel để tải lên." }
                });
            }

            using var stream = file.OpenReadStream();
            var result = await _gcsTemporaryReadWriteRepository.ImportGcsTemporaryExcelAsync(stream, gcsId, month, year, periodNumber, cancellationToken);
            return RequestResult<ImportGcsTemporaryExcelResponse>.Succeed(result);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ImportExcelGcsTemporaryAsync));
            return RequestResult<ImportGcsTemporaryExcelResponse>.Fail(new[]
            {
                new ErrorItem { FieldName = "System", Error = $"Có lỗi khi xử lý import file Excel: {e.Message}" }
            });
        }
    }
}