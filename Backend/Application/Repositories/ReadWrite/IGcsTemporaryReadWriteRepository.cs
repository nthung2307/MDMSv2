using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.IoT;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IGcsTemporaryReadWriteRepository : IRepository<GcsTemporaryEntity>
{
    Task<CursorPaginationResponse<GetDataLatchResponse>> GetListGcsTemporaryWithPaginationAsync(GetDataLatchRequest request, CancellationToken cancellationToken);
    Task<List<GcsTemporaryEntity>> GetListGcsTemporaryAsync(GetDataLatchRequest request, CancellationToken cancellationToken);
    Task<List<GcsTemporaryEntity>> GetListGcsTemporaryToUpdateAsync(List<UpdateGcsTemporaryRequest> request, long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken);
    Task<List<GcsTemporaryEntity>> GetListGcsTemporaryWithGcsAsync(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken);
    Task<bool> CheckExistGcsTemporaryAsync(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken);
}