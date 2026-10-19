using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointEventReadWriteRepository : IRepository<MeasurementPointsEventEntity>
{
    Task<int> CleanupOldEventsAsync(int monthsToKeep = 3, CancellationToken cancellationToken = default);
}