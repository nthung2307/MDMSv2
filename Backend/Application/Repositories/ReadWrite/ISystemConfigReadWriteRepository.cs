using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface ISystemConfigReadWriteRepository : IRepository<SystemConfigEntity>
{
    Task<SystemConfigEntity?> GetConfigByKeyAsync(string configKey, CancellationToken cancellationToken = default);
}