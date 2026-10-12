using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IPeriodicEnergyStatementReadWriteRepository : IRepository<PeriodicEnergyStatementEntity>
{
    Task<List<PeriodicEnergyStatementEntity>> GetPeriodicEnergyStatementByIdsAsync(List<long> ids, CancellationToken cancellationToken);
    Task<List<PeriodicEnergyStatementEntity>> GetPeriodicEnergyStatementByGcsIdAsync(SendAllMailRequest request, CancellationToken cancellationToken);
    Task<List<PeriodicEnergyStatementEntity>> GetListWithFormattedPMaxByIdsAsync(List<long> ids, CancellationToken cancellationToken);
}