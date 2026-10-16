using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadWrite
{
    public class PeriodicEnergyStatementReadWriteRepository : RestfulRepository<PeriodicEnergyStatementEntity, AppReadWriteDbContext>, IPeriodicEnergyStatementReadWriteRepository
    {
        private readonly DbSet<PeriodicEnergyStatementEntity> _periodicEnergyStatementEntities;

        public PeriodicEnergyStatementReadWriteRepository(AppReadWriteDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService) : base(applicationDbContext, snowflakeIdService)
        {
            _periodicEnergyStatementEntities = applicationDbContext.Set<PeriodicEnergyStatementEntity>();
        }

        public async Task<List<PeriodicEnergyStatementEntity>> GetPeriodicEnergyStatementByIdsAsync(List<long> ids, CancellationToken cancellationToken)
        {
            return await _periodicEnergyStatementEntities
                .Include(x => x.MeasurementPoint)
                .ThenInclude(x => x.Customer)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<PeriodicEnergyStatementEntity>> GetPeriodicEnergyStatementByGcsIdAsync(SendAllMailRequest request, CancellationToken cancellationToken)
        {
            return await _periodicEnergyStatementEntities
                .Include(x => x.MeasurementPoint)
                .ThenInclude(x => x.Customer)
                .Where(x => request.GcsId == x.GcsId && x.Month == request.Month && x.Year == request.Year && x.PeriodNumber == request.PeriodNumber &&
                            x.Status != "SEND_SUCCESS" && (x.Status == null || !x.Status.Contains("\"overall\":\"SEND_SUCCESS\"")))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<PeriodicEnergyStatementEntity>> GetListWithFormattedPMaxByIdsAsync(List<long> ids, CancellationToken cancellationToken)
        {
            var entities = await _periodicEnergyStatementEntities
                .Include(x => x.MeasurementPoint)
                .ThenInclude(x => x.Customer)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);
            var measurementPointIds = entities.Select(x => x.MeasurementPointId).ToList();
            var gcsId = entities.First().GcsId;
            var currentYear = entities.First().Year;
            var currentMonth = entities.First().Month;
            var currentPeriodNumber = entities.First().PeriodNumber;
            var previousPMax = await GetPreviousPeriodPMax(
                gcsId,
                measurementPointIds,
                currentYear,
                currentMonth,
                currentPeriodNumber,
                cancellationToken);
            foreach (var entity in entities)
            {
                var (prevPMax, prev2PMax) = previousPMax[entity.MeasurementPointId];
                entity.Pmax = prevPMax;
                entity.PreviousPmax = prev2PMax;
            }
            return entities;
        }

        private async Task<Dictionary<long, (double prevPMax, double prev2PMax)>> GetPreviousPeriodPMax(
            long gcsId,
            List<long> measurementPointId,
            int currentYear,
            int currentMonth,
            int currentPeriodNumber,
            CancellationToken cancellationToken)
        {
            int prevMonth;
            int prevYear;
            int prevPeriodNumber;
            int prev2Month;
            int prev2Year;
            int prev2PeriodNumber;
            List<PeriodicEnergyStatementEntity>? prevPeriods = null;
            List<PeriodicEnergyStatementEntity>? prev2Periods = null;

            Dictionary<long, (double prevPMax, double prev2PMax)> result = new();

            prevMonth = currentMonth;
            prevYear = currentYear;
            prevPeriodNumber = currentPeriodNumber;
            if (prevPeriodNumber > 1)
            {
                prevPeriodNumber = prevPeriodNumber - 1;
            }
            else
            {
                prevMonth -= 1;
                if (prevMonth == 0)
                {
                    prevMonth = 12;
                    prevYear--;
                }
            }
            prevPeriods = await _periodicEnergyStatementEntities
                    .Where(x => x.GcsId == gcsId
                        && measurementPointId.Contains(x.MeasurementPointId)
                        && x.Year == prevYear
                        && x.Month == prevMonth
                        && x.PeriodNumber == prevPeriodNumber)
                    .ToListAsync(cancellationToken);
            var prevPmaxDict = prevPeriods.ToDictionary(k => k.MeasurementPointId, k => k.Pmax);

            prev2Month = prevMonth;
            prev2Year = prevYear;
            prev2PeriodNumber = prevPeriodNumber;
            if (prev2PeriodNumber > 1)
            {
                prev2PeriodNumber = prev2PeriodNumber - 1;
            }
            else
            {
                prev2Month -= 1;
                if (prev2Month == 0)
                {
                    prev2Month = 12;
                    prev2Year--;
                }
            }
            prev2Periods = await _periodicEnergyStatementEntities
                    .Where(x => x.GcsId == gcsId
                        && measurementPointId.Contains(x.MeasurementPointId)
                        && x.Year == prev2Year
                        && x.Month == prev2Month
                        && x.PeriodNumber == prev2PeriodNumber)
                    .ToListAsync(cancellationToken);
            var prev2PmaxDict = prev2Periods.ToDictionary(k => k.MeasurementPointId, k => k.Pmax);
            foreach (var id in measurementPointId)
            {
                result.Add(id, (
                    prevPmaxDict.GetValueOrDefault(id),
                    prev2PmaxDict.GetValueOrDefault(id)));
            }
            return result;
        }
    }
}
