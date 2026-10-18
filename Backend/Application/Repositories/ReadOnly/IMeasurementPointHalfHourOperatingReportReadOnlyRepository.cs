using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IMeasurementPointHalfHourOperatingReportReadOnlyRepository : IRepository<MeasurementPointHalfHourOperatingReportEntity>
{
    Task<CursorPaginationResponse<OperatingParameterReportResponse>> GetOperatingParameterReportsAsync(
        OperatingParameterReportRequest request, 
        CancellationToken cancellationToken);

    Task<List<OperatingParameterReportResponse>> GetAllOperatingParameterReportsAsync(
        OperatingParameterReportRequest request, 
        CancellationToken cancellationToken);
}
