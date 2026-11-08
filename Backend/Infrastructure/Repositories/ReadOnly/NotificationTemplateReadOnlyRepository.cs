using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DataTransferObjects.Templates;
using Application.Repositories.ReadOnly;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class NotificationTemplateReadOnlyRepository : RestfulRepository<NotificationTemplateEntity, AppReadOnlyDbContext>,
    INotificationTemplateReadOnlyRepository
{
    private readonly DbSet<NotificationTemplateEntity> _entities;
    private readonly IMapper _mapper;

    public NotificationTemplateReadOnlyRepository(
        AppReadOnlyDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService,
        IMapper mapper) : base(applicationDbContext, snowflakeIdService)
    {
        _entities = applicationDbContext.Set<NotificationTemplateEntity>();
        _mapper = mapper;
    }

    public async Task<List<NotificationTemplateDto>> GetAllTemplatesAsync(string? code = null, CancellationToken cancellationToken = default)
    {
        var query = _entities.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(code))
        {
            var codeUpper = code.Trim().ToUpperInvariant();
            query = query.Where(x => x.Code.ToUpper() == codeUpper);
        }

        var list = await query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name).ToListAsync(cancellationToken);
        return _mapper.Map<List<NotificationTemplateDto>>(list);
    }

    public async Task<NotificationTemplateEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _entities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<NotificationTemplateEntity?> GetDefaultByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var codeUpper = code.Trim().ToUpperInvariant();

        // 1. Ưu tiên 1: Đang kích hoạt (IsActive) và là mặc định (IsDefault)
        var template = await _entities.AsNoTracking()
            .Where(x => x.Code.ToUpper() == codeUpper && x.IsActive && x.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);

        if (template != null) return template;

        // 2. Ưu tiên 2: Đang kích hoạt (IsActive) gần nhất
        template = await _entities.AsNoTracking()
            .Where(x => x.Code.ToUpper() == codeUpper && x.IsActive)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (template != null) return template;

        // 3. Ưu tiên 3: Là mặc định (IsDefault)
        template = await _entities.AsNoTracking()
            .Where(x => x.Code.ToUpper() == codeUpper && x.IsDefault)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (template != null) return template;

        // 4. Ưu tiên 4: Bất kỳ mẫu nào theo mã
        return await _entities.AsNoTracking()
            .Where(x => x.Code.ToUpper() == codeUpper)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<NotificationTemplateEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var codeUpper = code.Trim().ToUpperInvariant();
        return await _entities.AsNoTracking()
            .Where(x => x.Code.ToUpper() == codeUpper && x.IsActive)
            .OrderByDescending(x => x.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
