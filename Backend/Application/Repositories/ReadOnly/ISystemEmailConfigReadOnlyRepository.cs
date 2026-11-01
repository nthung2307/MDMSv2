using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface ISystemEmailConfigReadOnlyRepository : IRepository<SystemEmailConfigEntity>
{
    Task<SystemEmailConfigEntity?> GetActiveConfigAsync(CancellationToken cancellationToken = default);
}
