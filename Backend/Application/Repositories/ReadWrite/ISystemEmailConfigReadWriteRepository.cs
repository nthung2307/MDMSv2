using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface ISystemEmailConfigReadWriteRepository : IRepository<SystemEmailConfigEntity>
{
    Task<SystemEmailConfigEntity?> GetActiveConfigAsync(CancellationToken cancellationToken = default);
    Task EnsureTableCreatedAsync(CancellationToken cancellationToken = default);
}
