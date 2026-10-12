using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointMonitoringReadWriteRepository : IRepository<MeasurementPointMonitoringEntity>
{
    Task<List<MeasurementPointMonitoringEntity>> GetListMeasurementPointMonitoringAsync(CancellationToken cancellationToken);
}