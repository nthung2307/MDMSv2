using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointAlertReadWriteRepository : IRepository<MeasurementPointsAlertEntity>
{
    Task<List<MeasurementPointsAlertEntity>> GetAlertsForMeasurementPointsAsync(List<long> measurementPointIds, CancellationToken cancellationToken);
}