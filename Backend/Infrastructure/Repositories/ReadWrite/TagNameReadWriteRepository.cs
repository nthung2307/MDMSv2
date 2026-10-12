using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite;

public class TagNameReadWriteRepository : RestfulRepository<TagNameEntity, AppReadWriteDbContext>, ITagNameReadWriteRepository
{
    private readonly DbSet<TagNameEntity> _tagNameEntities;

    public TagNameReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
    {
        _tagNameEntities = applicationDbContext.Set<TagNameEntity>();
    }

    public async Task<TagNameEntity?> GetTagNameIdAsync(long id, CancellationToken cancellationToken)
    {
        return await _tagNameEntities
            .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, cancellationToken);
    }

    public async Task<List<TagNameEntity>> GetTagNamesForMeasurementPointsAsync(List<long> measurementPointIds, CancellationToken cancellationToken)
    {
        return await _tagNameEntities
            .Where(x => measurementPointIds.Contains(x.MeasurementPointId) && !x.Deleted && x.Type == "WARNING")
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TagNameEntity>> GetByMeasurementPointIdAndTypeAsync(long measurementPointId, CancellationToken cancellationToken)
    {
        return await _tagNameEntities
            .Where(x => x.MeasurementPointId == measurementPointId && !x.Deleted)
            .ToListAsync(cancellationToken);
    }
}