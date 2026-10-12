using Application.DataTransferObjects.Requests.Customers;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface ITagNameReadWriteRepository : IRepository<TagNameEntity>
{
    Task<TagNameEntity?> GetTagNameIdAsync(long id, CancellationToken cancellationToken);
    Task<List<TagNameEntity>> GetTagNamesForMeasurementPointsAsync(List<long> measurementPointIds, CancellationToken cancellationToken);
    Task<List<TagNameEntity>> GetByMeasurementPointIdAndTypeAsync(long measurementPointId, CancellationToken cancellationToken);
}