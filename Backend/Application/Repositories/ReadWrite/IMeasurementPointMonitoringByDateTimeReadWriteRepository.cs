using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointMonitoringReadWriteByDateTimeRepository : IRepository<MeasurementPointMonitoringByDateTimeEntity>
{
    Task<List<MeasurementPointMonitoringByDateTimeEntity>> GetMeasurementPointMonitoringDateTimeAsync(CancellationToken cancellationToken);
    Task<Dictionary<(long MeasurementPointId, string TagNameDescription), MeasurementPointMonitoringByDateTimeEntity>> GetLatestDateTimeRecordsAsync(
        List<long> measurementPointIds,
        CancellationToken cancellationToken);
}
