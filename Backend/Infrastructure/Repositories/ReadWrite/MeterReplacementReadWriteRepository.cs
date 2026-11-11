using System.Threading;
using System.Threading.Tasks;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite;

public class MeterReplacementReadWriteRepository : RestfulRepository<MeterReplacementEntity, AppReadWriteDbContext>,
    IMeterReplacementReadWriteRepository
{
    private readonly DbSet<MeterReplacementEntity> _entities;

    public MeterReplacementReadWriteRepository(
        AppReadWriteDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
    {
        _entities = applicationDbContext.Set<MeterReplacementEntity>();
    }

    public async Task<MeterReplacementEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
