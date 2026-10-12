using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Application.Queries.Report;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.Logger.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

[ApiController]
[Area(Common.Url.Areas.Report)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.Report)]
public class ReportController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;
    private readonly IMeasurementPointHalfHourEnergyReportReadWriteRepository _halfHourEnergyReportReadWriteRepository;
    private readonly IMeasurementPointDailyEnergyReportReadWriteRepository _dailyEnergyReportReadWriteRepository;

    public ReportController(IMapper mapper,
        IMediator mediator,
        IHttpContextAccessorService httpContextAccessorService,
        ILoggerService loggerService,
        IMeasurementPointHalfHourEnergyReportReadWriteRepository halfHourEnergyReportReadWriteRepository,
        IMeasurementPointDailyEnergyReportReadWriteRepository dailyEnergyReportReadWriteRepository)
    {
        _mapper = mapper;
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _halfHourEnergyReportReadWriteRepository = halfHourEnergyReportReadWriteRepository;
        _dailyEnergyReportReadWriteRepository = dailyEnergyReportReadWriteRepository;
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.LoadChart)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<LoadChartResponse>> GetListLoadChartAsync(LoadChartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetReportLoadChartQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<LoadChartResponse>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListLoadChartAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.VoltageChart)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<VoltageChartResponse>> GetListVoltageChartAsync(VoltageChartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetReportVoltageChartQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<VoltageChartResponse>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListVoltageChartAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.PowerChart)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<PowerChartResponse>> GetListPowerChartAsync(PowerChartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetReportPowerChartQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<PowerChartResponse>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListVoltageChartAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ConsumptionOutputChart)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<ConsumptionOutputChartResponse>> GetListConsumptionOutputChartAsync(ConsumptionOutputChartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetReportConsumptionOutputChartQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<ConsumptionOutputChartResponse>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListVoltageChartAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.FinalizedOutputElectricityChart)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<FinalizedOutputElectricityChartResponse>> GetFinalizedOutputElectricityChartAsync(FinalizedOutputElectricityChartRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new FinalizedOutputElectricityChartQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<FinalizedOutputElectricityChartResponse>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetFinalizedOutputElectricityChartAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ListFinalizedOutputElectricity)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<List<ListFinalizedOutputElectricityResponse>>> GetListFinalizedOutputElectricityAsync(ListFinalizedOutputElectricityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new ListFinalizedOutputElectricityQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<List<ListFinalizedOutputElectricityResponse>>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListFinalizedOutputElectricityAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.HalfHourFinalizedEnergyReport)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<CursorPaginationResponse<FinalizedEnergyReportResponse>>> GetListHalfHourFinalizedEnergyReportAsync(
        HalfHourFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new HalfHourFinalizedEnergyReportQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var result = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<FinalizedEnergyReportResponse>>.Succeed(result);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListHalfHourFinalizedEnergyReportAsync));
            throw;
        }
    }
    
    [HttpPost]
    [Route(Common.Url.Default.Report.ExportCsvHalfHourFinalizedEnergy)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvHalfHourEnergyReportAsync(HalfHourFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _halfHourEnergyReportReadWriteRepository.GenerateCsv(request, cancellationToken);
            return File(csvData, "text/csv", $"Export-Half-Hour-Report-{DateTime.Now.ToString("s")}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvHalfHourEnergyReportAsync));
            throw;
        }
    }
    
    [HttpPost]
    [Route(Common.Url.Default.Report.ExportPdfHalfHourFinalizedEnergy)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfHalfHourEnergyReportAsync(HalfHourFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _halfHourEnergyReportReadWriteRepository.GeneratePdf(request, cancellationToken);
            return File(pdfData, "application/pdf", $"Export-Half-Hour-Report-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfHalfHourEnergyReportAsync));
            throw;
        }
    }
    
    [HttpPost]
    [Route(Common.Url.Default.Report.DailyFinalizedEnergyReport)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<CursorPaginationResponse<FinalizedEnergyReportResponse>>> GetListDailyFinalizedEnergyReportAsync(
        DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new DailyFinalizedEnergyReportQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var result = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<FinalizedEnergyReportResponse>>.Succeed(result);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListHalfHourFinalizedEnergyReportAsync));
            throw;
        }
    }
    
    [HttpPost]
    [Route(Common.Url.Default.Report.ExportCsvDailyFinalizedEnergy)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvDailyEnergyReportAsync(DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _dailyEnergyReportReadWriteRepository.GenerateCsv(request, cancellationToken);
            return File(csvData, "text/csv", $"Export-Daily-Report-{DateTime.Now.ToString("s")}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvDailyEnergyReportAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportPdfDailyFinalizedEnergy)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfDailyEnergyReportAsync(DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _dailyEnergyReportReadWriteRepository.GeneratePdf(request, cancellationToken);
            return File(pdfData, "application/pdf", $"Export-Daily-Report-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfDailyEnergyReportAsync));
            throw;
        }
    }
}