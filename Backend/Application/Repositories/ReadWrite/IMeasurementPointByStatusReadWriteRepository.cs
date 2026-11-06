using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointByStatusReadWriteRepository : IRepository<MeasurementPointMonitoringByStatusEntity>
{
    Task<Dictionary<(long MeasurementPointId, string TagNameDescription), MeasurementPointMonitoringByStatusEntity>> GetLatestStatusRecordsAsync(
        List<long> measurementPointIds,
        CancellationToken cancellationToken);
}
