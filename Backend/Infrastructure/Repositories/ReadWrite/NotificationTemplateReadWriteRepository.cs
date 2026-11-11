using System.Collections.Generic;
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

    public async Task<NotificationTemplateEntity?> GetAnyByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var codeUpper = code.Trim().ToUpperInvariant();
        return await _entities
            .Where(x => x.Code.ToUpper() == codeUpper)
            .OrderByDescending(x => x.IsDefault)
            .ThenByDescending(x => x.IsActive)
            .ThenByDescending(x => x.ModifiedTime ?? x.CreatedTime)
            .ThenByDescending(x => x.Id)
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

    public async Task<int> DeduplicateTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var all = await _entities.ToListAsync(cancellationToken);
        if (all.Count <= 1) return 0;

        var toDelete = new List<NotificationTemplateEntity>();
        var grouped = all.GroupBy(x => (x.Code ?? string.Empty).Trim().ToUpperInvariant());

        foreach (var group in grouped)
        {
            var items = group.ToList();
            if (items.Count <= 1) continue;

            // Giữ lại 1 bản ghi ưu tiên nhất: Default + Active + cập nhật gần nhất + Id lớn nhất
            var keeper = items
                .OrderByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.IsActive)
                .ThenByDescending(x => x.ModifiedTime.HasValue)
                .ThenByDescending(x => x.ModifiedTime)
                .ThenByDescending(x => x.CreatedTime)
                .ThenByDescending(x => x.Id)
                .First();

            keeper.IsDefault = true;
            keeper.IsActive = true;

            var keeperName = (keeper.Name ?? string.Empty).Trim();
            foreach (var item in items)
            {
                if (item.Id == keeper.Id) continue;

                var itemName = (item.Name ?? string.Empty).Trim();
                // Xóa bản sao seed trùng lặp (cùng tên hoặc cùng bị đánh dấu Mặc định)
                if (item.IsDefault || string.Equals(itemName, keeperName, System.StringComparison.OrdinalIgnoreCase))
                {
                    toDelete.Add(item);
                }
            }
        }

        if (toDelete.Count > 0)
        {
            _entities.RemoveRange(toDelete);
            await SaveChangesAsync(cancellationToken);
        }

        return toDelete.Count;
    }
}
