using System.Threading;
using System.Threading.Tasks;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.MeterReplacement;
using Application.DataTransferObjects.Responses.MeterReplacement;

namespace Application.Services;

public interface IMeterReplacementService
{
    Task<OffsetPaginationResponse<MeterReplacementDto>> GetListAsync(GetListMeterReplacementRequest request, CancellationToken cancellationToken = default);
    Task<MeterReplacementDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<MeterReplacementDto> CreateAsync(CreateMeterReplacementRequest request, CancellationToken cancellationToken = default);
    Task<MeterReplacementDto> UpdateAsync(long id, UpdateMeterReplacementRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<MeterReplacementDto> ApproveAsync(long id, CancellationToken cancellationToken = default);
    Task<MeterReplacementDto> CancelAsync(long id, CancellationToken cancellationToken = default);
    Task<PointMeterInfoDto?> GetPointMeterInfoAsync(long pointId, CancellationToken cancellationToken = default);
}

