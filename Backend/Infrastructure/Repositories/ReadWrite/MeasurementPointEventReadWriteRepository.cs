using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class MeasurementPointEventReadWriteRepository : RestfulRepository<MeasurementPointsEventEntity, AppReadWriteDbContext>, IMeasurementPointEventReadWriteRepository
    {
        private readonly DbSet<MeasurementPointsEventEntity> _measurementPointsEventEntities;

        public MeasurementPointEventReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _measurementPointsEventEntities = applicationDbContext.Set<MeasurementPointsEventEntity>();
        }
    }
}
