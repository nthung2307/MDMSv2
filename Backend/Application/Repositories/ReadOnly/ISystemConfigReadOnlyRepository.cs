using Application.DataTransferObjects.Responses.SystemConfig;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface ISystemConfigReadOnlyRepository : IRepository<SystemConfigEntity>
{
    Task<List<SystemConfigDto>> GetListSystemConfigAsync(CancellationToken cancellationToken);
}