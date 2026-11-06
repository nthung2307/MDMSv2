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
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class MeasurementPointReadOnlyRepository : RestfulRepository<MeasurementPointsEntity, AppReadOnlyDbContext>, IMeasurementPointsReadOnlyRepository
{
    private readonly DbSet<MeasurementPointsEntity> _measurementPointsEntities;
    private readonly DbSet<TagNameEntity> _tagNameEntities;
    private readonly DbSet<UserEntity> _userEntities;
    private readonly DbSet<UserCustomerEntity> _userCustomerEntities;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService? _currentUserService;

    public MeasurementPointReadOnlyRepository(
        AppReadOnlyDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService,
        IMapper mapper,
        ICurrentUserService currentUserService) : base(applicationDbContext, snowflakeIdService)
    {
        _mapper = mapper;
        _measurementPointsEntities = applicationDbContext.Set<MeasurementPointsEntity>();
        _tagNameEntities = applicationDbContext.Set<TagNameEntity>();
        _userEntities = applicationDbContext.Set<UserEntity>();
        _userCustomerEntities = applicationDbContext.Set<UserCustomerEntity>();
        _currentUserService = currentUserService;
    }

    private async Task<(bool IsCustomer, List<long> LinkedCustomerIds)> GetCustomerAccessFilterAsync(CancellationToken cancellationToken)
    {
        if (_currentUserService == null) return (false, new List<long>());
        try
        {
            var userId = _currentUserService.UserId;
            if (userId <= 0) return (false, new List<long>());

            var user = await _userEntities.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.Deleted, cancellationToken);

            if (user != null && string.Equals(user.RoleName, DefaultRoles.Customer, StringComparison.OrdinalIgnoreCase))
            {
                var customerIds = await _userCustomerEntities.AsNoTracking()
                    .Where(uc => uc.UserId == userId)
                    .Select(uc => uc.CustomerId)
                    .ToListAsync(cancellationToken);

                return (true, customerIds);
            }
        }
        catch
        {
            // Background jobs / unauthenticated requests
        }

        return (false, new List<long>());
    }

    private async Task PopulateVoltageUnitsAsync(ICollection<ListMeasurementPointDto>? points, CancellationToken cancellationToken)
    {
        if (points == null || !points.Any()) return;
        try
        {
            var pointIds = points.Select(p => p.Id).Distinct().ToList();
            var voltageTags = await _tagNameEntities.AsNoTracking()
                .Where(t => pointIds.Contains(t.MeasurementPointId) &&
                            !t.Deleted &&
                            !string.IsNullOrEmpty(t.Unit) &&
                            (t.Description == ListDataMeasurementPoint.DienApPhaA ||
                             t.Description == ListDataMeasurementPoint.DienApPhaB ||
                             t.Description == ListDataMeasurementPoint.DienApPhaC ||
                             t.Description.Contains("DIEN_AP") ||
                             t.Description.Contains("Điện áp") ||
                             t.Description.Contains("dien ap")))
                .Select(t => new { t.MeasurementPointId, t.Description, t.Unit })
                .ToListAsync(cancellationToken);

            var unitByPoint = voltageTags
                .GroupBy(t => t.MeasurementPointId)
                .ToDictionary(
                    g => g.Key,
                    g => g.FirstOrDefault(t => t.Description == ListDataMeasurementPoint.DienApPhaA)?.Unit
                         ?? g.FirstOrDefault(t => t.Description == ListDataMeasurementPoint.DienApPhaB)?.Unit
                         ?? g.FirstOrDefault(t => t.Description == ListDataMeasurementPoint.DienApPhaC)?.Unit
                         ?? g.First().Unit);

            foreach (var p in points)
            {
                if (unitByPoint.TryGetValue(p.Id, out var unit) && !string.IsNullOrWhiteSpace(unit))
                {
                    p.VoltageUnit = unit;
                }
            }
        }
        catch
        {
            // Ignore if TagName table not available
        }
    }

    public async Task<CursorPaginationResponse<ListMeasurementPointDto>> GetListMeasurementPointsAsync(GetListMeasurementPointsRequest request, CancellationToken cancellationToken)
    {
        var measurementPoint = _measurementPointsEntities.AsNoTracking();
        var (isCustomer, linkedCustomerIds) = await GetCustomerAccessFilterAsync(cancellationToken);
        if (isCustomer)
        {
            measurementPoint = measurementPoint.Where(x => linkedCustomerIds.Contains(x.CustomerId));
            if (request.CustomerId != null && linkedCustomerIds.Contains(request.CustomerId.Value))
            {
                measurementPoint = measurementPoint.Where(x => x.CustomerId == request.CustomerId);
            }
        }
        else if (request.CustomerId != null)
        {
            measurementPoint = measurementPoint.Where(x => x.CustomerId == request.CustomerId);
        }
        var result = await measurementPoint
            .Include(x => x.Customer)
            .Include(x => x.Gcs)
            .Where(x => !x.Deleted && x.Code != "Mã điểm đo" && x.Code != "QUẢN LÝ ĐIỂM ĐO" && (x.Code == null || !x.Code.StartsWith("Thời gian xuất")))
            .PaginateWithAndAsync<MeasurementPointsEntity, ListMeasurementPointDto>(request, _mapper, cancellationToken);

        await PopulateVoltageUnitsAsync(result.Data, cancellationToken);

        return new CursorPaginationResponse<ListMeasurementPointDto>()
        {
            Data = result.Data,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious,
            TotalCount = result.TotalCount,
        };
    }

    public async Task<CursorPaginationResponse<ListMeasurementPointDto>> GetListMeasurementPointsForReportAsync(GetListMeasurementPointsForReportRequest request, CancellationToken cancellationToken)
    {
        var measurementPoint = _measurementPointsEntities.AsNoTracking();
        var (isCustomer, linkedCustomerIds) = await GetCustomerAccessFilterAsync(cancellationToken);
        if (isCustomer)
        {
            measurementPoint = measurementPoint.Where(x => linkedCustomerIds.Contains(x.CustomerId));
            if (request.CustomerId != null && linkedCustomerIds.Contains(request.CustomerId.Value))
            {
                measurementPoint = measurementPoint.Where(x => x.CustomerId == request.CustomerId);
            }
        }
        else if (request.CustomerId != null)
        {
            measurementPoint = measurementPoint.Where(x => x.CustomerId == request.CustomerId);
        }
        var result = await measurementPoint
            .Where(x => !x.Deleted && x.Status == EntityStatus.Active && x.Code != "Mã điểm đo" && x.Code != "QUẢN LÝ ĐIỂM ĐO" && (x.Code == null || !x.Code.StartsWith("Thời gian xuất")))
            .Include(x => x.Customer)
            .Include(x => x.Gcs)
            .PaginateWithAndAsync<MeasurementPointsEntity, ListMeasurementPointDto>(request, _mapper, cancellationToken);

        await PopulateVoltageUnitsAsync(result.Data, cancellationToken);

        return new CursorPaginationResponse<ListMeasurementPointDto>()
        {
            Data = result.Data,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious,
            TotalCount = result.TotalCount,
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
        return await _measurementPointsEntities.AsNoTracking().Where(x => x.Id == id && !x.Deleted).AnyAsync(cancellationToken);
    }

    public async Task<bool> IsExistMeasurementPointNameAsync(string code, long id, CancellationToken cancellationToken)
    {
        if(id != 0)
            return await _measurementPointsEntities.AsNoTracking().Where(x => x.Code == code && x.Id != id && !x.Deleted).AnyAsync(cancellationToken);
        return await _measurementPointsEntities.AsNoTracking().Where(x => x.Code == code && !x.Deleted).AnyAsync(cancellationToken);
    }
}
