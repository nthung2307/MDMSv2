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
                .Where(x => x.CreatedTime != null && x.CreatedTime <= DateTimeOffset.UtcNow.AddDays(-2))
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<(long MeasurementPointId, string TagNameDescription), MeasurementPointMonitoringEntity>> GetLatestFrozenMonitoringRecordsAsync(
            List<long> measurementPointIds,
            HashSet<string> frozenTagDescriptions,
            CancellationToken cancellationToken)
        {
            if (measurementPointIds.Count == 0 || frozenTagDescriptions.Count == 0)
                return new Dictionary<(long, string), MeasurementPointMonitoringEntity>();

            var latestIds = await _measurementPointMonitoringEntities
                .AsNoTracking()
                .Where(x => measurementPointIds.Contains(x.MeasurementPointsId)
                            && x.TagNameDescription != null
                            && frozenTagDescriptions.Contains(x.TagNameDescription))
                .GroupBy(x => new { x.MeasurementPointsId, x.TagNameDescription })
                .Select(g => g.Max(x => x.Id))
                .ToListAsync(cancellationToken);

            if (latestIds.Count == 0)
                return new Dictionary<(long, string), MeasurementPointMonitoringEntity>();

            var records = await _measurementPointMonitoringEntities
                .Where(x => latestIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            return records
                .Where(x => x.TagNameDescription != null)
                .GroupBy(x => (x.MeasurementPointsId, x.TagNameDescription!))
                .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.CreatedTime).ThenByDescending(r => r.Id).First());
        }
    }
}
