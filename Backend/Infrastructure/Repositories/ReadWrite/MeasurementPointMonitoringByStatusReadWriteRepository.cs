using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class MeasurementPointMonitoringByStatusReadWriteRepository : RestfulRepository<MeasurementPointMonitoringByStatusEntity, AppReadWriteDbContext>, IMeasurementPointByStatusReadWriteRepository
    {
        private readonly DbSet<MeasurementPointMonitoringByStatusEntity> _measurementPointMonitoringByStatusEntities;

        public MeasurementPointMonitoringByStatusReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _measurementPointMonitoringByStatusEntities = applicationDbContext.Set<MeasurementPointMonitoringByStatusEntity>();
        }

        public async Task<List<MeasurementPointMonitoringByStatusEntity>> GetMeasurementPointMonitoringByStatusAsync(CancellationToken cancellationToken)
        {
            return await _measurementPointMonitoringByStatusEntities
                .Where(x => x.CreatedTime.Value.Hour == DateTime.Now.AddHours(-1).Hour)
                .ToListAsync(cancellationToken);
        }
    }
}
