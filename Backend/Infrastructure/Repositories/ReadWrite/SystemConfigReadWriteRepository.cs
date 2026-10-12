using Application.Repositories.ReadWrite;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite;

public class SystemConfigReadWriteRepository : RestfulRepository<SystemConfigEntity, AppReadWriteDbContext>,
    ISystemConfigReadWriteRepository
{
    private readonly DbSet<SystemConfigEntity> _systemConfigEntities;
    private readonly IMapper _mapper;

    public SystemConfigReadWriteRepository(AppReadWriteDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(
        applicationDbContext, snowflakeIdService)
    {
        _mapper = mapper;
        _systemConfigEntities = applicationDbContext.Set<SystemConfigEntity>();
    }

    public async Task<SystemConfigEntity?> GetConfigByKeyAsync(string configKey,
        CancellationToken cancellationToken = default)
    {
        return await _systemConfigEntities.FirstOrDefaultAsync(c => c.Key == configKey, cancellationToken);
    }
}