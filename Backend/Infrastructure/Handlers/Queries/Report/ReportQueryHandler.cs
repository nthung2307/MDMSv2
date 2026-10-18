using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Responses.Report;
using Application.Queries.Report;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using MediatR;

namespace Infrastructure.Handlers.Queries.Report;

public class ReportQueryHandler :
    IRequestHandler<GetReportLoadChartQuery, LoadChartResponse?>,
    IRequestHandler<GetReportVoltageChartQuery, VoltageChartResponse?>,
    IRequestHandler<GetReportPowerChartQuery, PowerChartResponse?>,
    IRequestHandler<GetReportConsumptionOutputChartQuery, ConsumptionOutputChartResponse?>,
    IRequestHandler<FinalizedOutputElectricityChartQuery, FinalizedOutputElectricityChartResponse?>,
    IRequestHandler<ListFinalizedOutputElectricityQuery, List<ListFinalizedOutputElectricityResponse>>,
    IRequestHandler<HalfHourFinalizedEnergyReportQuery, CursorPaginationResponse<FinalizedEnergyReportResponse>>,
    IRequestHandler<DailyFinalizedEnergyReportQuery, CursorPaginationResponse<FinalizedEnergyReportResponse>>,
    IRequestHandler<YearlyFinalizedEnergyReportQuery, CursorPaginationResponse<FinalizedEnergyReportResponse>>,
    IRequestHandler<OperatingParameterReportQuery, CursorPaginationResponse<OperatingParameterReportResponse>>,
    IRequestHandler<OperatingParameterReportListQuery, List<OperatingParameterReportResponse>>,
    IRequestHandler<EnergyConsumptionReportQuery, EnergyConsumptionReportResponse>
{
    private readonly IMeasurementPointMonitoringHourlyReadOnlyRepository _measurementPointMonitoringHourlyReadOnlyRepository;
    private readonly IMeasurementPointHalfHourEnergyReportReadOnlyRepository _measurementPointHalfHourEnergyReportReadOnlyRepository;
    private readonly IMeasurementPointDailyEnergyReportReadOnlyRepository _measurementPointDailyEnergyReportReadOnlyRepository;
    private readonly IMeasurementPointHalfHourOperatingReportReadOnlyRepository _measurementPointHalfHourOperatingReportReadOnlyRepository;
    private readonly IEnergyConsumptionReportReadOnlyRepository _energyConsumptionReportReadOnlyRepository;

    public ReportQueryHandler(
        IMeasurementPointMonitoringHourlyReadOnlyRepository measurementPointMonitoringHourlyReadOnlyRepository,
        IMeasurementPointHalfHourEnergyReportReadOnlyRepository measurementPointHalfHourEnergyReportReadOnlyRepository,
        IMeasurementPointDailyEnergyReportReadOnlyRepository measurementPointDailyEnergyReportReadOnlyRepository,
        IMeasurementPointHalfHourOperatingReportReadOnlyRepository measurementPointHalfHourOperatingReportReadOnlyRepository,
        IEnergyConsumptionReportReadOnlyRepository energyConsumptionReportReadOnlyRepository
        )
    {
        _measurementPointMonitoringHourlyReadOnlyRepository = measurementPointMonitoringHourlyReadOnlyRepository;
        _measurementPointHalfHourEnergyReportReadOnlyRepository = measurementPointHalfHourEnergyReportReadOnlyRepository;
        _measurementPointDailyEnergyReportReadOnlyRepository = measurementPointDailyEnergyReportReadOnlyRepository;
        _measurementPointHalfHourOperatingReportReadOnlyRepository = measurementPointHalfHourOperatingReportReadOnlyRepository;
        _energyConsumptionReportReadOnlyRepository = energyConsumptionReportReadOnlyRepository;
    }
    public async Task<LoadChartResponse?> Handle(GetReportLoadChartQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointMonitoringHourlyReadOnlyRepository.GetLoadChartAsync(request.Request, cancellationToken);
    }

    public async Task<VoltageChartResponse?> Handle(GetReportVoltageChartQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointMonitoringHourlyReadOnlyRepository.GetVoltageChartAsync(request.Request, cancellationToken);
    }

    public async Task<PowerChartResponse?> Handle(GetReportPowerChartQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointMonitoringHourlyReadOnlyRepository.GetPowerChartAsync(request.Request, cancellationToken);
    }

    public async Task<ConsumptionOutputChartResponse?> Handle(GetReportConsumptionOutputChartQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointMonitoringHourlyReadOnlyRepository.GetConsumptionOutputChartAsync(request.Request, cancellationToken);
    }

    public async Task<FinalizedOutputElectricityChartResponse?> Handle(FinalizedOutputElectricityChartQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointMonitoringHourlyReadOnlyRepository.GetFinalizedOutputElectricityChartAsync(request.Request, cancellationToken);
    }

    public async Task<List<ListFinalizedOutputElectricityResponse>> Handle(ListFinalizedOutputElectricityQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointMonitoringHourlyReadOnlyRepository.GetListFinalizedOutputElectricityAsync(request.Request, cancellationToken);
    }

    public async Task<CursorPaginationResponse<FinalizedEnergyReportResponse>> Handle(HalfHourFinalizedEnergyReportQuery request,
        CancellationToken cancellationToken)
    {
        return await _measurementPointHalfHourEnergyReportReadOnlyRepository.GetFinalizedEnergyReportsAsync(request.Request, cancellationToken);
    }
    
    public async Task<CursorPaginationResponse<FinalizedEnergyReportResponse>> Handle(DailyFinalizedEnergyReportQuery request,
        CancellationToken cancellationToken)
    {
        return await _measurementPointDailyEnergyReportReadOnlyRepository.GetFinalizedEnergyReportsAsync(request.Request, cancellationToken);
    }

    public async Task<CursorPaginationResponse<FinalizedEnergyReportResponse>> Handle(YearlyFinalizedEnergyReportQuery request,
        CancellationToken cancellationToken)
    {
        return await _measurementPointDailyEnergyReportReadOnlyRepository.GetYearlyFinalizedEnergyReportsAsync(request.Request, cancellationToken);
    }

    public async Task<CursorPaginationResponse<OperatingParameterReportResponse>> Handle(OperatingParameterReportQuery request,
        CancellationToken cancellationToken)
    {
        return await _measurementPointHalfHourOperatingReportReadOnlyRepository.GetOperatingParameterReportsAsync(request.Request, cancellationToken);
    }

    public async Task<List<OperatingParameterReportResponse>> Handle(OperatingParameterReportListQuery request,
        CancellationToken cancellationToken)
    {
        return await _measurementPointHalfHourOperatingReportReadOnlyRepository.GetAllOperatingParameterReportsAsync(request.Request, cancellationToken);
    }

    public async Task<EnergyConsumptionReportResponse> Handle(EnergyConsumptionReportQuery request,
        CancellationToken cancellationToken)
    {
        return await _energyConsumptionReportReadOnlyRepository.GetEnergyConsumptionReportAsync(request.Request, cancellationToken);
    }
}