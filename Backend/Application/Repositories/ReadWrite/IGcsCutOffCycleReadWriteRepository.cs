using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IGcsCutOffCycleReadWriteRepository : IRepository<GcsCutoffCycleEntity>
{
    Task<GcsCutoffCycleEntity?> GetGcsCutOffCycleEntityAsync(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken);
    Task<GcsCutoffCycleEntity?> GetMaxGcsCutOffCycleAsync(long gcsId, CancellationToken cancellationToken);

}