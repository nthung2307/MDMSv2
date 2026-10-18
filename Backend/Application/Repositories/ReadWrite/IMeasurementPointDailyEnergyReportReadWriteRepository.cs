using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointDailyEnergyReportReadWriteRepository : IRepository<MeasurementPointDailyEnergyReportEntity>
{
	Task<byte[]> GeneratePdf(Application.DataTransferObjects.Requests.Report.DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
	Task<byte[]> GenerateCsv(Application.DataTransferObjects.Requests.Report.DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
	Task<byte[]> GenerateExcel(Application.DataTransferObjects.Requests.Report.DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
	Task<byte[]> GenerateYearlyPdf(Application.DataTransferObjects.Requests.Report.YearlyFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
	Task<byte[]> GenerateYearlyCsv(Application.DataTransferObjects.Requests.Report.YearlyFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
	Task<byte[]> GenerateYearlyExcel(Application.DataTransferObjects.Requests.Report.YearlyFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
}