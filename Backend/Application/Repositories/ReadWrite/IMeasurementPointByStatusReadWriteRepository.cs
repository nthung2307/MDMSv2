using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointByStatusReadWriteRepository : IRepository<MeasurementPointMonitoringByStatusEntity>
{
}