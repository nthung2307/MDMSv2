using Application.Commands.MeasurementPoints;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.Queries.MeasurementPoints;
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
[Area(Common.Url.Areas.MeasurementPoints)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.MeasurementPoints)]
public class MeasurementPointController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;

    public MeasurementPointController(IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService, IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository)
    {
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPoints.Create)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<MeasurementPointResponse>> CreateMeasurementPointAsync(CreateMeasurementPointsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var createMeasurementPointCommand = new CreateMeasurementPointsCommands()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(createMeasurementPointCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(CreateMeasurementPointAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.MeasurementPoints.Update)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<MeasurementPointResponse>> UpdateMeasurementPointAsync(UpdateMeasurementPointsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updateMeasurementPointCommand = new UpdateMeasurementPointsCommand()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(updateMeasurementPointCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(UpdateMeasurementPointAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.MeasurementPoints.ChangeStatus)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<MeasurementPointResponse>> ChangeStatusMeasurementPointAsync(ChangeStatusMeasurementPointsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updateMeasurementPointCommand = new ChangeStatusMeasurementPointCommand()
            {
                Id = request.Id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(updateMeasurementPointCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(UpdateMeasurementPointAsync));
            throw;
        }
    }

    [HttpDelete]
    [Route(Common.Url.Default.MeasurementPoints.Delete)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<MeasurementPointResponse>> DeleteAccountAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var deleteMeasurementPointCommand = new DeleteMeasurementPointsCommand()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(deleteMeasurementPointCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(DeleteAccountAsync));
            throw;
        }
    }
    
    [HttpPost]
    [Route(Common.Url.Default.MeasurementPoints.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<CursorPaginationResponse<ListMeasurementPointDto>>> GetListMeasurementPointsAsync(GetListMeasurementPointsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListMeasurementPointsQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var measurementPoint = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<ListMeasurementPointDto>>.Succeed(measurementPoint);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListMeasurementPointsAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPoints.ListForReport)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<CursorPaginationResponse<ListMeasurementPointDto>>> GetListMeasurementPointsForReportAsync(GetListMeasurementPointsForReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListMeasurementPointsForReportQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var measurementPoint = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<ListMeasurementPointDto>>.Succeed(measurementPoint);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListMeasurementPointsAsync));
            throw;
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.MeasurementPoints.View)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<MeasurementPointDto>> GetMeasurementPointDetailAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetMeasurementPointDetailQuery()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var measurementPoint = await _mediator.Send(query, cancellationToken);
            return RequestResult<MeasurementPointDto>.Succeed(measurementPoint);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetMeasurementPointDetailAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPoints.ExportPdf)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfAccountAsync(ExportPdfAndExcelRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _measurementPointsReadWriteRepository.GenerateCustomerPdf(request, cancellationToken);
            return File(pdfData, "application/pdf", $"Export-Measurement-Point-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfAccountAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.MeasurementPoints.ExportCsv)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvMeasurementPointAsync(CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _measurementPointsReadWriteRepository.GenerateCustomerCsv(cancellationToken);
            return File(
                fileContents: csvData,
                contentType: "application/octet-stream; charset=utf-8",
                fileDownloadName: $"Export-Measurement-Point-{DateTime.Now:s}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvMeasurementPointAsync));
            throw;
        }
    }
}