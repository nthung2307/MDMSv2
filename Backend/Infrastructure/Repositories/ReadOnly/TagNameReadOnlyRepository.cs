using Application.DataTransferObjects.Responses.TagName;
using Application.Repositories.ReadOnly;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class TagNameReadOnlyRepository : RestfulRepository<TagNameEntity, AppReadOnlyDbContext>, ITagNameReadOnlyRepository
{
    private readonly DbSet<TagNameEntity> _tagNameEntities;

    public TagNameReadOnlyRepository(AppReadOnlyDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
    {
        _tagNameEntities = applicationDbContext.Set<TagNameEntity>();
    }

    public async Task<List<TagNameDto>> GetListTagNamesAsync(long measurementPointId, string? type, CancellationToken cancellationToken)
    {
        var result = _tagNameEntities.AsNoTracking().Where(x => x.MeasurementPointId == measurementPointId);
        if (type != null)
        {
            result = result.Where(x => x.Type == type);
        }
        return await result.Select(x => new TagNameDto()
        {
            Id = x.Id,
            TagName = x.TagName,
            Description = x.Description,
            Unit = x.Unit,
            CustomName = x.CustomName,
            Type = x.Type,
        }).ToListAsync(cancellationToken);
    }

    public async Task<bool> IsExistedTagName(string name, long id, CancellationToken cancellationToken)
    {
        if (id > 0)
            return await _tagNameEntities.AsNoTracking().Where(x => x.TagName == name && x.Id != id).AnyAsync(cancellationToken);
        return await _tagNameEntities.AsNoTracking().Where(x => x.TagName == name).AnyAsync(cancellationToken);
    }
}