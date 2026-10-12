using Application.DataTransferObjects.Responses.SystemConfig;
using Application.Repositories.ReadOnly;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class SystemConfigReadOnlyRepository : RestfulRepository<SystemConfigEntity, AppReadOnlyDbContext>,
    ISystemConfigReadOnlyRepository
{
    private readonly DbSet<SystemConfigEntity> _systemConfigEntities;

    public SystemConfigReadOnlyRepository(
        AppReadOnlyDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(applicationDbContext, snowflakeIdService)
    {
        _systemConfigEntities = applicationDbContext.Set<SystemConfigEntity>();
    }

    public async Task<List<SystemConfigDto>> GetListSystemConfigAsync(CancellationToken cancellationToken)
    {
        return await _systemConfigEntities.Select(x => new SystemConfigDto()
        {
            Id = x.Id,
            Key = x.Key,
            Value = x.Value
        }).ToListAsync(cancellationToken);
    }
}