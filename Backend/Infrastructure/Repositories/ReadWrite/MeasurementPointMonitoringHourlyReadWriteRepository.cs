using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class MeasurementPointMonitoringHourlyReadWriteRepository : RestfulRepository<MeasurementPointMonitoringHourlyEntity, AppReadWriteDbContext>, IMeasurementPointMonitoringHourlyReadWriteRepository
    {
        private readonly DbSet<MeasurementPointMonitoringHourlyEntity> _measurementPointMonitoringHourlyEntities;

        public MeasurementPointMonitoringHourlyReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _measurementPointMonitoringHourlyEntities = applicationDbContext.Set<MeasurementPointMonitoringHourlyEntity>();
        }
    }
}
