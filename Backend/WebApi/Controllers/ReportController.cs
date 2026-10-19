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
    private readonly IMeasurementPointHalfHourOperatingReportReadWriteRepository _operatingReportReadWriteRepository;
    private readonly IEnergyConsumptionReportReadWriteRepository _consumptionReportReadWriteRepository;

    public ReportController(IMapper mapper,
        IMediator mediator,
        IHttpContextAccessorService httpContextAccessorService,
        ILoggerService loggerService,
        IMeasurementPointHalfHourEnergyReportReadWriteRepository halfHourEnergyReportReadWriteRepository,
        IMeasurementPointDailyEnergyReportReadWriteRepository dailyEnergyReportReadWriteRepository,
        IMeasurementPointHalfHourOperatingReportReadWriteRepository operatingReportReadWriteRepository,
        IEnergyConsumptionReportReadWriteRepository consumptionReportReadWriteRepository)
    {
        _mapper = mapper;
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _halfHourEnergyReportReadWriteRepository = halfHourEnergyReportReadWriteRepository;
        _dailyEnergyReportReadWriteRepository = dailyEnergyReportReadWriteRepository;
        _operatingReportReadWriteRepository = operatingReportReadWriteRepository;
        _consumptionReportReadWriteRepository = consumptionReportReadWriteRepository;
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

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportExcelHalfHourFinalizedEnergy)]
    public async Task<IActionResult> ExportExcelHalfHourEnergyReportAsync(HalfHourFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _halfHourEnergyReportReadWriteRepository.GenerateCsv(request, cancellationToken);
            return File(data, "text/csv", $"Export-HalfHour-Report-{DateTime.Now.ToString("s")}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportExcelHalfHourEnergyReportAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportExcelDailyFinalizedEnergy)]
    public async Task<IActionResult> ExportExcelDailyEnergyReportAsync(DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _dailyEnergyReportReadWriteRepository.GenerateExcel(request, cancellationToken);
            return File(data, "text/csv", $"Export-Daily-Report-{DateTime.Now.ToString("s")}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportExcelDailyEnergyReportAsync));
            throw;
        }
    }

    #region Báo cáo Chỉ số Điện Hàng Năm (0:00 đầu mỗi tháng)
    [HttpPost]
    [Route(Common.Url.Default.Report.YearlyFinalizedEnergyReport)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<CursorPaginationResponse<FinalizedEnergyReportResponse>>> GetListYearlyFinalizedEnergyReportAsync(
        YearlyFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new YearlyFinalizedEnergyReportQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var result = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<FinalizedEnergyReportResponse>>.Succeed(result);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListYearlyFinalizedEnergyReportAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportCsvYearlyFinalizedEnergy)]
    public async Task<IActionResult> ExportCsvYearlyEnergyReportAsync(YearlyFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _dailyEnergyReportReadWriteRepository.GenerateYearlyCsv(request, cancellationToken);
            return File(csvData, "text/csv", $"Export-Yearly-Report-{DateTime.Now.ToString("s")}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvYearlyEnergyReportAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportExcelYearlyFinalizedEnergy)]
    public async Task<IActionResult> ExportExcelYearlyEnergyReportAsync(YearlyFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var excelData = await _dailyEnergyReportReadWriteRepository.GenerateYearlyExcel(request, cancellationToken);
            return File(excelData, "text/csv", $"Export-Yearly-Report-{DateTime.Now.ToString("s")}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportExcelYearlyEnergyReportAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportPdfYearlyFinalizedEnergy)]
    public async Task<IActionResult> ExportPdfYearlyEnergyReportAsync(YearlyFinalizedEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _dailyEnergyReportReadWriteRepository.GenerateYearlyPdf(request, cancellationToken);
            return File(pdfData, "application/pdf", $"Export-Yearly-Report-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfYearlyEnergyReportAsync));
            throw;
        }
    }
    #endregion

    #region Báo cáo Thông số Vận hành 30 Phút
    [HttpPost]
    [Route(Common.Url.Default.Report.OperatingParameterReport)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<CursorPaginationResponse<OperatingParameterReportResponse>>> GetOperatingParameterReportAsync(
        OperatingParameterReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new OperatingParameterReportQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var result = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<OperatingParameterReportResponse>>.Succeed(result);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetOperatingParameterReportAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.OperatingParameterChart)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<List<OperatingParameterReportResponse>>> GetOperatingParameterChartAsync(
        OperatingParameterReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new OperatingParameterReportListQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var result = await _mediator.Send(query, cancellationToken);
            return RequestResult<List<OperatingParameterReportResponse>>.Succeed(result);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetOperatingParameterChartAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportCsvOperatingParameter)]
    public async Task<IActionResult> ExportCsvOperatingParameterAsync(OperatingParameterReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _operatingReportReadWriteRepository.GenerateCsv(request, cancellationToken);
            return File(data, "text/csv", $"Export-Operating-Parameter-{DateTime.Now.ToString("s")}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvOperatingParameterAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportExcelOperatingParameter)]
    public async Task<IActionResult> ExportExcelOperatingParameterAsync(OperatingParameterReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _operatingReportReadWriteRepository.GenerateExcel(request, cancellationToken);
            return File(data, "text/csv", $"Export-Operating-Parameter-{DateTime.Now.ToString("s")}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportExcelOperatingParameterAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportPdfOperatingParameter)]
    public async Task<IActionResult> ExportPdfOperatingParameterAsync(OperatingParameterReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _operatingReportReadWriteRepository.GeneratePdf(request, cancellationToken);
            return File(data, "application/pdf", $"Export-Operating-Parameter-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfOperatingParameterAsync));
            throw;
        }
    }
    #endregion

    #region Báo cáo Sản lượng Điện (30 phút, Tháng, Năm)
    [HttpPost]
    [Route(Common.Url.Default.Report.EnergyConsumptionReport)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<EnergyConsumptionReportResponse>> GetEnergyConsumptionReportAsync(
        EnergyConsumptionReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new EnergyConsumptionReportQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var result = await _mediator.Send(query, cancellationToken);
            return RequestResult<EnergyConsumptionReportResponse>.Succeed(result);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetEnergyConsumptionReportAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportCsvEnergyConsumption)]
    public async Task<IActionResult> ExportCsvEnergyConsumptionAsync(EnergyConsumptionReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _consumptionReportReadWriteRepository.GenerateCsv(request, cancellationToken);
            return File(data, "text/csv", $"Export-Energy-Consumption-{DateTime.Now.ToString("s")}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvEnergyConsumptionAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportExcelEnergyConsumption)]
    public async Task<IActionResult> ExportExcelEnergyConsumptionAsync(EnergyConsumptionReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _consumptionReportReadWriteRepository.GenerateExcel(request, cancellationToken);
            return File(data, "text/csv", $"Export-Energy-Consumption-{DateTime.Now.ToString("s")}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportExcelEnergyConsumptionAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportPdfEnergyConsumption)]
    public async Task<IActionResult> ExportPdfEnergyConsumptionAsync(EnergyConsumptionReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _consumptionReportReadWriteRepository.GeneratePdf(request, cancellationToken);
            return File(data, "application/pdf", $"Export-Energy-Consumption-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfEnergyConsumptionAsync));
            throw;
        }
    }
    #endregion

    #region Báo cáo Chỉ số điện tất cả điểm đo đang hoạt động tại 1 thời điểm
    [HttpPost]
    [Route(Common.Url.Default.Report.ActivePointsEnergyReport)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<List<ActivePointsEnergyReportResponse>>> GetActivePointsEnergyReportAsync(
        ActivePointsEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetActivePointsEnergyReportQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var result = await _mediator.Send(query, cancellationToken);
            return RequestResult<List<ActivePointsEnergyReportResponse>>.Succeed(result);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetActivePointsEnergyReportAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportCsvActivePointsEnergy)]
    public async Task<IActionResult> ExportCsvActivePointsEnergyAsync(
        ActivePointsEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _halfHourEnergyReportReadWriteRepository.GenerateActivePointsEnergyCsvAsync(request, cancellationToken);
            return File(data, "text/csv", $"Export-Active-Points-Energy-{DateTime.Now:yyyyMMddHHmmss}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvActivePointsEnergyAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportExcelActivePointsEnergy)]
    public async Task<IActionResult> ExportExcelActivePointsEnergyAsync(
        ActivePointsEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _halfHourEnergyReportReadWriteRepository.GenerateActivePointsEnergyExcelAsync(request, cancellationToken);
            return File(data, "text/csv", $"Export-Active-Points-Energy-{DateTime.Now:yyyyMMddHHmmss}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportExcelActivePointsEnergyAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Report.ExportPdfActivePointsEnergy)]
    public async Task<IActionResult> ExportPdfActivePointsEnergyAsync(
        ActivePointsEnergyReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _halfHourEnergyReportReadWriteRepository.GenerateActivePointsEnergyCsvAsync(request, cancellationToken);
            return File(data, "text/csv", $"Export-Active-Points-Energy-{DateTime.Now:yyyyMMddHHmmss}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfActivePointsEnergyAsync));
            throw;
        }
    }
    #endregion
}