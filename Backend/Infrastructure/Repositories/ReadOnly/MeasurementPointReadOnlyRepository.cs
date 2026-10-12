using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.DataTransferObjects.Responses.TagName;
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

public class MeasurementPointReadOnlyRepository : RestfulRepository<MeasurementPointsEntity, AppReadOnlyDbContext>, IMeasurementPointsReadOnlyRepository
{
    private readonly DbSet<MeasurementPointsEntity> _measurementPointsEntities;
    private readonly IMapper _mapper;

    public MeasurementPointReadOnlyRepository(AppReadOnlyDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(applicationDbContext, snowflakeIdService)
    {
        _mapper = mapper;
        _measurementPointsEntities = applicationDbContext.Set<MeasurementPointsEntity>();
    }

    public async Task<CursorPaginationResponse<ListMeasurementPointDto>> GetListMeasurementPointsAsync(GetListMeasurementPointsRequest request, CancellationToken cancellationToken)
    {
        var measurementPoint = _measurementPointsEntities.AsNoTracking();
        if (request.CustomerId != null)
        {
            measurementPoint = measurementPoint.Where(x => x.CustomerId == request.CustomerId);
        }
        var result = await measurementPoint
            .Include(x => x.Customer)
            .Include(x => x.Gcs)
            .Where(x => !x.Deleted)
            .PaginateWithAndAsync<MeasurementPointsEntity, ListMeasurementPointDto>(request, _mapper, cancellationToken);
        return new CursorPaginationResponse<ListMeasurementPointDto>()
        {
            Data = result.Data,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious,
        };
    }

    public async Task<CursorPaginationResponse<ListMeasurementPointDto>> GetListMeasurementPointsForReportAsync(GetListMeasurementPointsForReportRequest request, CancellationToken cancellationToken)
    {
        var measurementPoint = _measurementPointsEntities.AsNoTracking();
        if (request.CustomerId != null)
        {
            measurementPoint = measurementPoint.Where(x => x.CustomerId == request.CustomerId);
        }
        var result = await measurementPoint
            .Where(x => !x.Deleted && x.Status == EntityStatus.Active)
            .Include(x => x.Customer)
            .Include(x => x.Gcs)
            .Where(x => !x.Deleted)
            .PaginateWithAndAsync<MeasurementPointsEntity, ListMeasurementPointDto>(request, _mapper, cancellationToken);
        return new CursorPaginationResponse<ListMeasurementPointDto>()
        {
            Data = result.Data,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious,
        };
    }

    public async Task<MeasurementPointDto> GetMeasurementPointDetailAsync(long id, CancellationToken cancellationToken)
    {
        var user = await _measurementPointsEntities
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Gcs)
            .Where(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync(cancellationToken);
        return _mapper.Map<MeasurementPointDto>(user);
    }

    public async Task<List<MeasurementPointDto>> GetListMeasurementPointAsync(CancellationToken cancellationToken)
    {
        return await _measurementPointsEntities
            .AsNoTracking()
            .Where(x => x.Status == EntityStatus.Active && !x.Deleted)
            .Include(x => x.TagNames)
            .ProjectTo<MeasurementPointDto>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken);
    }

    public async Task<List<MeasurementPointGcsDto>> GetListMeasurementPointGcsAsync(long gcsId, CancellationToken cancellationToken)
    {
        return await _measurementPointsEntities
            .AsNoTracking()
            .Where(x => !x.Deleted && x.GcsId == gcsId && x.Status == EntityStatus.Active)
            .Include(x => x.TagNames)
            .Select(x => new MeasurementPointGcsDto
            {
                Id = x.Id,
                Code = x.Code,
                Description = x.Description,
                CustomerName = x.Customer.FullName,
                MultiplierFactor = x.MultiplierFactor,
                GcsId = x.GcsId,
                OldGcsId = x.OldGcsId,
                IsNewChange = x.IsNewChange,
                TagNames = x.TagNames.Select(t => new TagNameDto
                {
                    Id = t.Id,
                    TagName = t.TagName,
                    Description = t.Description,
                    Unit = t.Unit
                }).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsExistMeasurementPointAsync(long id, CancellationToken cancellationToken)
    {
        return await _measurementPointsEntities.AsNoTracking().Where(x => x.Id == id).AnyAsync(cancellationToken);
    }

    public async Task<bool> IsExistMeasurementPointNameAsync(string code, long id, CancellationToken cancellationToken)
    {
        if(id != 0)
            return await _measurementPointsEntities.AsNoTracking().Where(x => x.Code == code && x.Id != id).AnyAsync(cancellationToken);
        return await _measurementPointsEntities.AsNoTracking().Where(x => x.Code == code).AnyAsync(cancellationToken);
    }
}