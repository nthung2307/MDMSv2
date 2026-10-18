using Application.DataTransferObjects.Requests.Report;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IEnergyConsumptionReportReadWriteRepository : IRepository<MeasurementPointDailyEnergyReportEntity>
{
    Task<byte[]> GenerateExcel(EnergyConsumptionReportRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateCsv(EnergyConsumptionReportRequest request, CancellationToken cancellationToken);
    Task<byte[]> GeneratePdf(EnergyConsumptionReportRequest request, CancellationToken cancellationToken);
}
