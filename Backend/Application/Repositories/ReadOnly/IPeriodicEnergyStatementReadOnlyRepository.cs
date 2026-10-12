using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IPeriodicEnergyStatementReadOnlyRepository : IRepository<PeriodicEnergyStatementEntity>
{
    Task<CursorPaginationResponse<PeriodicEnergyStatementDto>> GetListPeriodicEnergyStatementAsync(GetListPeriodicEnergyStatementRequest request, CancellationToken cancellationToken);
    Task<int> GetMaxSerialNumberPeriodicEnergyStatementAsync(CancellationToken cancellationToken);
}