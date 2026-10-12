using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class MeasurementPointMonitoringReadWriteRepository : RestfulRepository<MeasurementPointMonitoringEntity, AppReadWriteDbContext>, IMeasurementPointMonitoringReadWriteRepository
    {
        private readonly DbSet<MeasurementPointMonitoringEntity> _measurementPointMonitoringEntities;

        public MeasurementPointMonitoringReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _measurementPointMonitoringEntities = applicationDbContext.Set<MeasurementPointMonitoringEntity>();
        }

        public async Task<List<MeasurementPointMonitoringEntity>> GetListMeasurementPointMonitoringAsync(CancellationToken cancellationToken)
        {
            return await _measurementPointMonitoringEntities
                .Where(x => x.CreatedTime.Value.Day == DateTime.Now.AddDays(-2).Day)
                .ToListAsync(cancellationToken);
        }
    }
}
