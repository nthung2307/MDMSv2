using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Constants;
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
            var clockTags = new List<string>
            {
                ListDataMeasurementPoint.NgayHienTai,
                ListDataMeasurementPoint.ThangHienTai,
                ListDataMeasurementPoint.NamHienTai,
                ListDataMeasurementPoint.GioHienTai,
                ListDataMeasurementPoint.PhutHienTai,
                ListDataMeasurementPoint.GiayHienTai
            };

            // Chỉ dọn các bản ghi rỗng hoặc các bản ghi đồng hồ hiện tại cũ dư thừa,
            // tuyệt đối giữ lại bản ghi mới nhất của mỗi điểm đo và toàn bộ lịch sử thời điểm chốt / Pmax
            var latestClockIds = await _measurementPointMonitoringByDateTimeEntities
                .Where(x => x.TagNameDescription != null && clockTags.Contains(x.TagNameDescription))
                .GroupBy(x => new { x.MeasurementPointId, x.TagNameDescription })
                .Select(g => g.Max(x => x.Id))
                .ToListAsync(cancellationToken);

            var latestClockIdSet = latestClockIds.ToHashSet();

            return await _measurementPointMonitoringByDateTimeEntities
                .Where(x => x.CreatedTime <= DateTime.UtcNow.AddHours(-1)
                            && (x.TagNameDescription == null
                                || (clockTags.Contains(x.TagNameDescription) && !latestClockIdSet.Contains(x.Id))))
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<(long MeasurementPointId, string TagNameDescription), MeasurementPointMonitoringByDateTimeEntity>> GetLatestDateTimeRecordsAsync(
            List<long> measurementPointIds,
            CancellationToken cancellationToken)
        {
            if (measurementPointIds.Count == 0)
                return new Dictionary<(long, string), MeasurementPointMonitoringByDateTimeEntity>();

            var records = await _measurementPointMonitoringByDateTimeEntities
                .Where(x => measurementPointIds.Contains(x.MeasurementPointId) && x.TagNameDescription != null)
                .OrderByDescending(x => x.CreatedTime)
                .ThenByDescending(x => x.Id)
                .ToListAsync(cancellationToken);

            return records
                .GroupBy(x => (x.MeasurementPointId, x.TagNameDescription!))
                .ToDictionary(g => g.Key, g => g.First());
        }
    }
}
