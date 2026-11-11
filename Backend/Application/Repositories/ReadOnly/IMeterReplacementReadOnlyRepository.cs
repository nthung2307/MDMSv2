using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.MeterReplacement;
using Application.DataTransferObjects.Responses.MeterReplacement;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IMeterReplacementReadOnlyRepository : IRepository<MeterReplacementEntity>
{
    Task<OffsetPaginationResponse<MeterReplacementDto>> GetPagedListAsync(GetListMeterReplacementRequest request, CancellationToken cancellationToken = default);
    Task<MeterReplacementEntity?> GetByIdWithPointAsync(long id, CancellationToken cancellationToken = default);
    Task<MeterReplacementEntity?> GetApprovedReplacementInPeriodAsync(long measurementPointId, DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken cancellationToken = default);
    Task<PointMeterInfoDto?> GetPointMeterInfoAsync(long pointId, CancellationToken cancellationToken = default);
}

