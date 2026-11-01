using System.Threading;
using System.Threading.Tasks;
using Application.Repositories.ReadOnly;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class SystemEmailConfigReadOnlyRepository : RestfulRepository<SystemEmailConfigEntity, AppReadOnlyDbContext>,
    ISystemEmailConfigReadOnlyRepository
{
    private readonly DbSet<SystemEmailConfigEntity> _entities;

    public SystemEmailConfigReadOnlyRepository(AppReadOnlyDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
    {
        _entities = applicationDbContext.Set<SystemEmailConfigEntity>();
    }

    public async Task<SystemEmailConfigEntity?> GetActiveConfigAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _entities.AsNoTracking().FirstOrDefaultAsync(c => c.IsActive, cancellationToken);
        }
        catch
        {
            return null;
        }
    }
}
