using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite;

public class StatementPdfTemplateReadWriteRepository : RestfulRepository<StatementPdfTemplateEntity, AppReadWriteDbContext>,
    IStatementPdfTemplateReadWriteRepository
{
    private readonly DbSet<StatementPdfTemplateEntity> _entities;

    public StatementPdfTemplateReadWriteRepository(
        AppReadWriteDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
    {
        _entities = applicationDbContext.Set<StatementPdfTemplateEntity>();
    }

    public async Task<StatementPdfTemplateEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<StatementPdfTemplateEntity?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _entities
            .Where(x => x.IsActive && x.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SetDefaultAsync(long id, CancellationToken cancellationToken = default)
    {
        var existing = await _entities.ToListAsync(cancellationToken);
        foreach (var t in existing)
        {
            t.IsDefault = (t.Id == id);
        }
    }
}
