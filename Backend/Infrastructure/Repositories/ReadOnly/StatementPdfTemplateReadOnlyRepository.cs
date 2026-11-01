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

public class StatementPdfTemplateReadOnlyRepository : RestfulRepository<StatementPdfTemplateEntity, AppReadOnlyDbContext>,
    IStatementPdfTemplateReadOnlyRepository
{
    private readonly DbSet<StatementPdfTemplateEntity> _entities;
    private readonly IMapper _mapper;

    public StatementPdfTemplateReadOnlyRepository(
        AppReadOnlyDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService,
        IMapper mapper) : base(applicationDbContext, snowflakeIdService)
    {
        _entities = applicationDbContext.Set<StatementPdfTemplateEntity>();
        _mapper = mapper;
    }

    public async Task<List<StatementPdfTemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _entities.AsNoTracking()
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<StatementPdfTemplateDto>>(list);
    }

    public async Task<StatementPdfTemplateEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _entities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<StatementPdfTemplateEntity?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await _entities.AsNoTracking()
            .Where(x => x.IsActive && x.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<StatementPdfTemplateEntity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var codeUpper = code.Trim().ToUpperInvariant();
        return await _entities.AsNoTracking()
            .Where(x => x.Code.ToUpper() == codeUpper && x.IsActive)
            .OrderByDescending(x => x.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
