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

public class NotificationTemplateReadWriteRepository : RestfulRepository<NotificationTemplateEntity, AppReadWriteDbContext>,
    INotificationTemplateReadWriteRepository
{
    private readonly DbSet<NotificationTemplateEntity> _entities;

    public NotificationTemplateReadWriteRepository(
        AppReadWriteDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
    {
        _entities = applicationDbContext.Set<NotificationTemplateEntity>();
    }

    public async Task<NotificationTemplateEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _entities.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<NotificationTemplateEntity?> GetDefaultByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var codeUpper = code.Trim().ToUpperInvariant();
        return await _entities
            .Where(x => x.Code.ToUpper() == codeUpper && x.IsActive && x.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SetDefaultAsync(long id, string code, CancellationToken cancellationToken = default)
    {
        var codeUpper = code.Trim().ToUpperInvariant();
        var existing = await _entities.Where(x => x.Code.ToUpper() == codeUpper).ToListAsync(cancellationToken);
        foreach (var t in existing)
        {
            t.IsDefault = (t.Id == id);
        }
    }
}
