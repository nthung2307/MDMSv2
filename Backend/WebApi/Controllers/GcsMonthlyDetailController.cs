using Application.Commands.Gcs;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Requests.GcsCutOffCycle;
using Application.DataTransferObjects.Requests.GcsMonthlyDetail;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.GcsCutOffCycle;
using Application.DataTransferObjects.Responses.GcsMonthlyDetail;
using Application.DataTransferObjects.Responses.IoT;
using Application.Queries.Gcs;
using Application.Queries.GcsCutOffCycle;
using Application.Queries.GcsMonthlyDetail;
using Application.Repositories.ReadOnly;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.Logger.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

[ApiController]
[Area(Common.Url.Areas.GcsMonthlyDetail)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.GcsMonthlyDetail)]
public class GcsMonthlyDetailController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;
    private readonly IGcsMonthlyDetailReadOnlyRepository _gcsMonthlyDetailReadOnlyRepository;

    public GcsMonthlyDetailController(IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService, IGcsMonthlyDetailReadOnlyRepository gcsMonthlyDetailReadOnlyRepository)
    {
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _gcsMonthlyDetailReadOnlyRepository = gcsMonthlyDetailReadOnlyRepository;
    }

    [HttpPost]
    [Route(Common.Url.Default.GcsMonthlyDetail.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<CursorPaginationResponse<GcsMonthlyDetailDto>>> GetListGcsAsync(GetListGcsMonthlyDetailRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListGcsMonthlyDetailQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var gcs = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<GcsMonthlyDetailDto>>.Succeed(gcs);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListGcsAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.GcsMonthlyDetail.ExportPdf)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfAccountAsync(long gcsCutOffId, CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _gcsMonthlyDetailReadOnlyRepository.ExportPdfGcsMonthlyDetailAsync(gcsCutOffId, cancellationToken);
            return File(pdfData, "application/pdf", $"Export-Gcs-Monthly-Detail-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfAccountAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.GcsMonthlyDetail.ExportCsv)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvGcsMonthlyDetailAsync(long gcsCutOffId, CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _gcsMonthlyDetailReadOnlyRepository.ExportCsvGcsMonthlyDetailAsync(gcsCutOffId, cancellationToken);
            return File(
                fileContents: csvData,
                contentType: "application/octet-stream; charset=utf-8",
                fileDownloadName: $"Export-Gcs-Monthly-Detail-{DateTime.Now:s}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvGcsMonthlyDetailAsync));
            throw;
        }
    }
}