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

        public async Task<Dictionary<(long MeasurementPointId, string TagNameDescription), MeasurementPointMonitoringEntity>> GetLatestFrozenMonitoringRecordsAsync(
            List<long> measurementPointIds,
            HashSet<string> frozenTagDescriptions,
            CancellationToken cancellationToken)
        {
            if (measurementPointIds.Count == 0 || frozenTagDescriptions.Count == 0)
                return new Dictionary<(long, string), MeasurementPointMonitoringEntity>();

            var records = await _measurementPointMonitoringEntities
                .Where(x => measurementPointIds.Contains(x.MeasurementPointsId)
                            && x.TagNameDescription != null
                            && frozenTagDescriptions.Contains(x.TagNameDescription))
                .OrderByDescending(x => x.CreatedTime)
                .ThenByDescending(x => x.Id)
                .ToListAsync(cancellationToken);

            return records
                .GroupBy(x => (x.MeasurementPointsId, x.TagNameDescription!))
                .ToDictionary(g => g.Key, g => g.First());
        }
    }
}
