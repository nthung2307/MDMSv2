using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IMeasurementPointHalfHourEnergyReportReadOnlyRepository : IRepository<MeasurementPointHalfHourEnergyReportEntity>
{
    Task<CursorPaginationResponse<FinalizedEnergyReportResponse>> GetFinalizedEnergyReportsAsync(HalfHourFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
}