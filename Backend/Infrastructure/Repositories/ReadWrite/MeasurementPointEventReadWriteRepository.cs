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

        public async Task<int> CleanupOldEventsAsync(int monthsToKeep = 3, CancellationToken cancellationToken = default)
        {
            var thresholdDate = DateTime.UtcNow.AddMonths(-monthsToKeep);
            return await _measurementPointsEventEntities
                .Where(x => x.CreatedTime < thresholdDate)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
