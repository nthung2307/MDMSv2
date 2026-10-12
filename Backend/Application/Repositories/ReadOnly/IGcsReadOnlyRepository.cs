using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.IoT;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IGcsReadOnlyRepository : IRepository<GcsEntity>
{
    Task<CursorPaginationResponse<GcsDto>> GetListGcsAsync(GetListGcsRequest request, CancellationToken cancellationToken);
    Task<GcsDto> GetGcsAsync(long id, CancellationToken cancellationToken);
    Task<List<GcsCurrentReadingDto>> GetGcsCurrentReadingAsync(long gcsId, List<long> measurementPointIds, CancellationToken cancellationToken);
    Task<GcsDto?> IsExistGcsAsync(long id, CancellationToken cancellationToken);
}