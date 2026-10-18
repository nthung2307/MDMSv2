using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IEnergyConsumptionReportReadOnlyRepository : IRepository<MeasurementPointDailyEnergyReportEntity>
{
    Task<EnergyConsumptionReportResponse> GetEnergyConsumptionReportAsync(
        EnergyConsumptionReportRequest request, 
        CancellationToken cancellationToken);
}
