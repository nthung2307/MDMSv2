using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.GcsMonthlyDetail;
using Application.DataTransferObjects.Responses.GcsMonthlyDetail;
using Application.DataTransferObjects.Responses.IoT;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IGcsMonthlyDetailReadOnlyRepository : IRepository<GcsMonthlyDetailEntity>
{
    Task<List<GetDataLatchResponse>> GetGcsMonthlyDetailAsync(int month, int year, long gcsCutoffCycleId, CancellationToken cancellationToken);
    Task<CursorPaginationResponse<GcsMonthlyDetailDto>> GetListGcsMonthlyDetailWithPaginationAsync(GetListGcsMonthlyDetailRequest request, CancellationToken cancellationToken);
    Task<byte[]> ExportPdfGcsMonthlyDetailAsync(long gcsCutOffId, CancellationToken cancellationToken);
    Task<byte[]> ExportCsvGcsMonthlyDetailAsync(long gcsCutOffId, CancellationToken cancellationToken);
}