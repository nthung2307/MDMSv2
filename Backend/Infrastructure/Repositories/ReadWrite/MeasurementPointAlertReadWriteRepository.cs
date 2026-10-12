using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class MeasurementPointAlertReadWriteRepository : RestfulRepository<MeasurementPointsAlertEntity, AppReadWriteDbContext>, IMeasurementPointAlertReadWriteRepository
    {
        private readonly DbSet<MeasurementPointsAlertEntity> _measurementPointsAlertEntities;

        public MeasurementPointAlertReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _measurementPointsAlertEntities = applicationDbContext.Set<MeasurementPointsAlertEntity>();
        }

        public async Task<List<MeasurementPointsAlertEntity>> GetAlertsForMeasurementPointsAsync(List<long> measurementPointIds, CancellationToken cancellationToken)
        {
            return await _measurementPointsAlertEntities
                .Where(x => measurementPointIds.Contains(x.MeasurementPointId))
                .ToListAsync(cancellationToken);
        }
    }
}
