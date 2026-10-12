using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.GcsCutOffCycle;
using Application.DataTransferObjects.Responses.IoT;
using Application.Extensions;
using Application.Repositories.ReadOnly;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class GcsReadOnlyRepository : RestfulRepository<GcsEntity, AppReadOnlyDbContext>, IGcsReadOnlyRepository
{
    private readonly DbSet<GcsEntity> _gcsEntities;
    private readonly DbSet<GcsCutoffCycleEntity> _gcsCutoffCycleEntities;
    private readonly DbSet<GcsMonthlyDetailEntity> _gcsMonthlyDetailEntities;
    private readonly IMapper _mapper;

    public GcsReadOnlyRepository(AppReadOnlyDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(applicationDbContext, snowflakeIdService)
    {
        _mapper = mapper;
        _gcsEntities = applicationDbContext.Set<GcsEntity>();
        _gcsCutoffCycleEntities = applicationDbContext.Set<GcsCutoffCycleEntity>();
        _gcsMonthlyDetailEntities = applicationDbContext.Set<GcsMonthlyDetailEntity>();
    }

    public async Task<CursorPaginationResponse<GcsDto>> GetListGcsAsync(GetListGcsRequest request, CancellationToken cancellationToken)
    {
        var result = await _gcsEntities
            .AsNoTracking()
            .Include(x => x.MeasurementPoints)
            .Where(x => !x.Deleted)
            .PaginateWithAndAsync<GcsEntity, GcsDto>(request, _mapper, cancellationToken);
        return new CursorPaginationResponse<GcsDto>()
        {
            Data = result.Data,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious,
        };
    }

    public async Task<GcsDto> GetGcsAsync(long id, CancellationToken cancellationToken)
    {
        var user = await _gcsEntities.AsNoTracking().Where(x => x.Id == id && !x.Deleted).Include(x => x.MeasurementPoints.Where(mp => mp.Status != EntityStatus.Deleted)).FirstOrDefaultAsync(cancellationToken);
        return _mapper.Map<GcsDto>(user);
    }

    public async Task<List<GcsCurrentReadingDto>> GetGcsCurrentReadingAsync(long gcsId, List<long> measurementPointIds, CancellationToken cancellationToken)
    {
        var latestCutoffCycle = await _gcsCutoffCycleEntities
            .Where(x => x.GcsId == gcsId)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ThenByDescending(x => x.PeriodNumber)
            .Include(x => x.GcsMonthlyDetail)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestCutoffCycle == null)
        {
            return new List<GcsCurrentReadingDto>();
        }

        var result = await _gcsEntities
            .AsNoTracking()
            .Where(gcs => gcs.Id == gcsId)
            .Join(
                _gcsMonthlyDetailEntities,
                gcs => latestCutoffCycle.Id, // Use the found cutoff cycle's Id
                monthlyDetail => monthlyDetail.GcsCutoffCycleId,
                (gcs, monthlyDetail) => new { gcs, monthlyDetail }
            )
            .Where(x => measurementPointIds.Contains(x.monthlyDetail.MeasurementPointId))
            .Select(x => new GcsCurrentReadingDto
            {
                CurrentPeriodNumber = latestCutoffCycle.PeriodNumber,
                CurrentDay = latestCutoffCycle.Day,
                CurrentMonth = latestCutoffCycle.Month,
                CurrentYear = latestCutoffCycle.Year,
                MeasurementPointId = x.monthlyDetail.MeasurementPointId,
                CurrentReading = x.monthlyDetail.CurrentReading,
                MeterReadingSet = x.monthlyDetail.MeterReadingSet
            })
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<GcsDto?> IsExistGcsAsync(long id, CancellationToken cancellationToken)
    {
        return await _gcsEntities
            .AsNoTracking()
            .Where(x => x.Id == id && !x.Deleted && x.Status == EntityStatus.Active)
            .ProjectTo<GcsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}