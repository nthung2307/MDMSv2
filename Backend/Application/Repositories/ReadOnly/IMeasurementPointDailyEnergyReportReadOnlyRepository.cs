using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IMeasurementPointDailyEnergyReportReadOnlyRepository : IRepository<MeasurementPointDailyEnergyReportEntity>
{
    Task<CursorPaginationResponse<FinalizedEnergyReportResponse>> GetFinalizedEnergyReportsAsync(DailyFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
    Task<CursorPaginationResponse<FinalizedEnergyReportResponse>> GetYearlyFinalizedEnergyReportsAsync(YearlyFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
}