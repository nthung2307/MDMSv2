using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using Application.Extensions;
using Application.Repositories.ReadOnly;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class PeriodicEnergyStatementReadOnlyRepository : RestfulRepository<PeriodicEnergyStatementEntity, AppReadOnlyDbContext>, IPeriodicEnergyStatementReadOnlyRepository
{
    private readonly DbSet<PeriodicEnergyStatementEntity> _periodicEnergyStatementEntities;
    private readonly IMapper _mapper;

    public PeriodicEnergyStatementReadOnlyRepository(AppReadOnlyDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(applicationDbContext, snowflakeIdService)
    {
        _mapper = mapper;
        _periodicEnergyStatementEntities = applicationDbContext.Set<PeriodicEnergyStatementEntity>();
    }

    public async Task<CursorPaginationResponse<PeriodicEnergyStatementDto>> GetListPeriodicEnergyStatementAsync(GetListPeriodicEnergyStatementRequest request, CancellationToken cancellationToken)
    {
        var result = await _periodicEnergyStatementEntities.AsNoTracking()
            .Where(x => x.GcsId == request.GcsId && x.Month == request.Month && x.Year == request.Year && x.PeriodNumber == request.PeriodNumber)
            .Include(x => x.MeasurementPoint)
            .ThenInclude(x => x.Customer)
            .PaginateWithAndAsync<PeriodicEnergyStatementEntity, PeriodicEnergyStatementDto>(request, _mapper, cancellationToken);
        // var measurementPointIds = result.Data!.Select(x => x.MeasurementPointId).ToList();
        // var previousPMax = await GetPreviousPeriodPMax(
        //     request.GcsId,
        //     measurementPointIds,
        //     request.Year,
        //     request.Month,
        //     request.PeriodNumber,
        //     cancellationToken);
        // foreach (var item in result.Data!)
        // {
        //     var (prevPMax, prev2PMax) = previousPMax[item.MeasurementPointId];
        //     item.Pmax = prevPMax;
        //     item.PreviousPmax = prev2PMax;
        // }

        return new CursorPaginationResponse<PeriodicEnergyStatementDto>()
        {
            Data = result.Data,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious,
        };
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

    public async Task<int> GetMaxSerialNumberPeriodicEnergyStatementAsync(CancellationToken cancellationToken)
    {
        bool hasRecords = await _periodicEnergyStatementEntities
            .AsNoTracking()
            .AnyAsync(cancellationToken);

        if (!hasRecords)
            return 0;

        return _periodicEnergyStatementEntities
            .AsNoTracking()
            .AsEnumerable()
            .Max(x => int.Parse(x.SerialNumber));
    }
}