using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointMonitoringReadWriteRepository : IRepository<MeasurementPointMonitoringEntity>
{
    Task<List<MeasurementPointMonitoringEntity>> GetListMeasurementPointMonitoringAsync(CancellationToken cancellationToken);
    Task<Dictionary<(long MeasurementPointId, string TagNameDescription), MeasurementPointMonitoringEntity>> GetLatestFrozenMonitoringRecordsAsync(
        List<long> measurementPointIds,
        HashSet<string> frozenTagDescriptions,
        CancellationToken cancellationToken);
}
