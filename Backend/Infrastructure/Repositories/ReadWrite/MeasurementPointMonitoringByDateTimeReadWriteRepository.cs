using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class MeasurementPointMonitoringByDateTimeReadWriteRepository : RestfulRepository<MeasurementPointMonitoringByDateTimeEntity, AppReadWriteDbContext>, IMeasurementPointMonitoringReadWriteByDateTimeRepository
    {
        private readonly DbSet<MeasurementPointMonitoringByDateTimeEntity> _measurementPointMonitoringByDateTimeEntities;

        public MeasurementPointMonitoringByDateTimeReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _measurementPointMonitoringByDateTimeEntities = applicationDbContext.Set<MeasurementPointMonitoringByDateTimeEntity>();
        }

        public async Task<List<MeasurementPointMonitoringByDateTimeEntity>> GetMeasurementPointMonitoringDateTimeAsync(CancellationToken cancellationToken)
        {
            return await _measurementPointMonitoringByDateTimeEntities
                .Where(x => x.CreatedTime <= DateTime.UtcNow.AddHours(-1))
                .ToListAsync(cancellationToken);
        }
    }
}
