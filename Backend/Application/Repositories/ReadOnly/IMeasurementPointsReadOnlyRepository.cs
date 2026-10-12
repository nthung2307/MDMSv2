using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IMeasurementPointsReadOnlyRepository : IRepository<MeasurementPointsEntity>
{
    Task<CursorPaginationResponse<ListMeasurementPointDto>> GetListMeasurementPointsAsync(GetListMeasurementPointsRequest request, CancellationToken cancellationToken);
    Task<CursorPaginationResponse<ListMeasurementPointDto>> GetListMeasurementPointsForReportAsync(GetListMeasurementPointsForReportRequest request, CancellationToken cancellationToken);
    Task<MeasurementPointDto> GetMeasurementPointDetailAsync(long id, CancellationToken cancellationToken);
    Task<List<MeasurementPointDto>> GetListMeasurementPointAsync(CancellationToken cancellationToken);
    Task<List<MeasurementPointGcsDto>> GetListMeasurementPointGcsAsync(long gcsId, CancellationToken cancellationToken);
    Task<bool> IsExistMeasurementPointAsync(long id, CancellationToken cancellationToken);
    Task<bool> IsExistMeasurementPointNameAsync(string code, long id, CancellationToken cancellationToken);
}