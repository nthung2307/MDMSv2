using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointDailyEnergyReportReadWriteRepository : IRepository<MeasurementPointDailyEnergyReportEntity>
{
	Task<byte[]> GeneratePdf(Application.DataTransferObjects.Requests.Report.DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
	Task<byte[]> GenerateCsv(Application.DataTransferObjects.Requests.Report.DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
}