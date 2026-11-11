using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeterReplacementReadWriteRepository : IRepository<MeterReplacementEntity>
{
    Task<MeterReplacementEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
