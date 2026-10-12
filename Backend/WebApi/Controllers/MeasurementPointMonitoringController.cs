using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.MeasurementPointMonitoring;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Queries.MeasurementPointMonitoring;
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
[Area(Common.Url.Areas.MeasurementPointMonitoring)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.MeasurementPointMonitoring)]
public class MeasurementPointMonitoringController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;
    private readonly IMeasurementPointAlertReadOnlyRepository _measurementPointAlertReadOnlyRepository;
    private readonly IMeasurementPointMonitoringReadOnlyRepository _measurementPointMonitoringReadOnlyRepository;
    private readonly IMeasurementPointEventReadOnlyRepository _measurementPointEventReadOnlyRepository;

    public MeasurementPointMonitoringController(IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService, IMeasurementPointAlertReadOnlyRepository measurementPointAlertReadOnlyRepository, IMeasurementPointMonitoringReadOnlyRepository measurementPointMonitoringReadOnlyRepository, IMeasurementPointEventReadOnlyRepository measurementPointEventReadOnlyRepository)
    {
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _measurementPointAlertReadOnlyRepository = measurementPointAlertReadOnlyRepository;
        _measurementPointMonitoringReadOnlyRepository = measurementPointMonitoringReadOnlyRepository;
        _measurementPointEventReadOnlyRepository = measurementPointEventReadOnlyRepository;
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPointMonitoring.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<CursorPaginationResponse<ListMeasurementPointMonitoringDto>>> GetListMeasurementPointsAsync(GetListMeasurementPointMonitoringRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListMeasurementPointMonitoringQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var measurementPoint = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<ListMeasurementPointMonitoringDto>>.Succeed(measurementPoint);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListMeasurementPointsAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPointMonitoring.ListAlert)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<CursorPaginationResponse<MeasurementPointAlertDto>>> GetListMeasurementPointsAlertAsync(GetListMeasurementPointAlertRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListMeasurementPointAlertQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var measurementPoint = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<MeasurementPointAlertDto>>.Succeed(measurementPoint);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListMeasurementPointsAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPointMonitoring.ListEvent)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<CursorPaginationResponse<MeasurementPointEventDto>>> GetListMeasurementPointsEventAsync(GetListMeasurementPointEventRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListMeasurementPointEventQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var measurementPoint = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<MeasurementPointEventDto>>.Succeed(measurementPoint);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListMeasurementPointsAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPointMonitoring.View)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<MeasurementPointMonitoringDto>> GetMeasurementPointsAsync(string tagName, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetMeasurementPointMonitoringQuery()
            {
                TagName = tagName,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var measurementPoint = await _mediator.Send(query, cancellationToken);
            return RequestResult<MeasurementPointMonitoringDto>.Succeed(measurementPoint);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListMeasurementPointsAsync));
            throw;
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.MeasurementPointMonitoring.CountAlert)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<int>> CountAlertAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _measurementPointAlertReadOnlyRepository.CountMeasurementPointAlert(cancellationToken);
            return RequestResult<int>.Succeed(response);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListMeasurementPointsAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPointMonitoring.ExportPdfMeasurementPointMonitoring)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfMeasurementPointMonitoringAsync(CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _measurementPointMonitoringReadOnlyRepository.ExportPdfMeasurementPointsMonitoringAsync(cancellationToken);
            return File(pdfData, "application/pdf", $"Export-Measurement-Point-Monitoring-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfMeasurementPointMonitoringAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPointMonitoring.ExportCsvMeasurementPointMonitoring)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvMeasurementPointMonitoringAsync(CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _measurementPointMonitoringReadOnlyRepository.ExportCsvMeasurementPointsMonitoringAsync(cancellationToken);
            return File(
                fileContents: csvData,
                contentType: "application/octet-stream; charset=utf-8",
                fileDownloadName: $"Export-Measurement-Point-Monitoring-{DateTime.Now:s}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvMeasurementPointMonitoringAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPointMonitoring.ExportPdfMeasurementPointAlert)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfAlertAsync(CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _measurementPointAlertReadOnlyRepository.ExportPdfMeasurementPointsAlertAsync(cancellationToken);
            return File(pdfData, "application/pdf", $"Export-Measurement-Point-Alert-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfMeasurementPointMonitoringAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPointMonitoring.ExportCsvMeasurementPointAlert)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvMeasurementPointMonitoringAlertAsync(CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _measurementPointAlertReadOnlyRepository.ExportCsvMeasurementPointsAlertAsync(cancellationToken);
            return File(
                fileContents: csvData,
                contentType: "application/octet-stream; charset=utf-8",
                fileDownloadName: $"Export-Measurement-Point-Monitoring-Alert-{DateTime.Now:s}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvMeasurementPointMonitoringAlertAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPointMonitoring.ExportPdfMeasurementPointEvent)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfEventAsync(CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _measurementPointEventReadOnlyRepository.ExportPdfMeasurementPointsEventAsync(cancellationToken);
            return File(pdfData, "application/pdf", $"Export-Measurement-Point-Event-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfMeasurementPointMonitoringAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPointMonitoring.ExportCsvMeasurementPointEvent)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvMeasurementPointMonitoringEventAsync(CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _measurementPointEventReadOnlyRepository.ExportCsvMeasurementPointsEventAsync(cancellationToken);
            return File(
                fileContents: csvData,
                contentType: "application/octet-stream; charset=utf-8",
                fileDownloadName: $"Export-Measurement-Point-Monitoring-Event-{DateTime.Now:s}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvMeasurementPointMonitoringEventAsync));
            throw;
        }
    }
}