using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IMeasurementPointMonitoringHourlyReadOnlyRepository : IRepository<MeasurementPointMonitoringHourlyEntity>
{
    Task<LoadChartResponse?> GetLoadChartAsync(LoadChartRequest request, CancellationToken cancellationToken);
    Task<VoltageChartResponse?> GetVoltageChartAsync(VoltageChartRequest request, CancellationToken cancellationToken);
    Task<PowerChartResponse?> GetPowerChartAsync(PowerChartRequest request, CancellationToken cancellationToken);
    Task<ConsumptionOutputChartResponse?> GetConsumptionOutputChartAsync(ConsumptionOutputChartRequest request, CancellationToken cancellationToken);
    Task<FinalizedOutputElectricityChartResponse?> GetFinalizedOutputElectricityChartAsync(FinalizedOutputElectricityChartRequest request, CancellationToken cancellationToken);
    Task<List<ListFinalizedOutputElectricityResponse>> GetListFinalizedOutputElectricityAsync(ListFinalizedOutputElectricityRequest request, CancellationToken cancellationToken);
}