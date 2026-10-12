using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.GcsCutOffCycle;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Application.DataTransferObjects.Responses.GcsCutOffCycle;
using Application.DataTransferObjects.Responses.IoT;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IGcsCutOffCycleReadOnlyRepository : IRepository<GcsCutoffCycleEntity>
{
    Task<bool> GetExistGcsCutOff(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken);
    Task<RequestResult<CursorPaginationResponse<GetDataLatchResponse>>> GetDataLatchResponseAsync(GetDataLatchRequest request, CancellationToken cancellationToken);
    Task<GcsCutOffCycleDto?> GetGcsCutOffCycleAsync(long gcsId, CancellationToken cancellationToken);
    Task<CursorPaginationResponse<GcsCutOffCycleResponse>> GetListGcsCutOffCyclesAsync(GetListGcsCutOffCycleRequest request, CancellationToken cancellationToken);
    Task<List<int>> GetListYearGcsCutOffCyclesAsync(long gcsId, CancellationToken cancellationToken);
    Task<bool> IsSendingMail(SendAllMailRequest request, CancellationToken cancellationToken);
}