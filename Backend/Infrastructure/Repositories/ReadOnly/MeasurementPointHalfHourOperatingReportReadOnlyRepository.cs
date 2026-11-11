using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Application.Extensions;
using Application.Repositories.ReadOnly;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class MeasurementPointHalfHourOperatingReportReadOnlyRepository :
    RestfulRepository<MeasurementPointHalfHourOperatingReportEntity, AppReadOnlyDbContext>,
    IMeasurementPointHalfHourOperatingReportReadOnlyRepository
{
    private readonly AppReadOnlyDbContext _dbContext;
    private readonly DbSet<MeasurementPointHalfHourOperatingReportEntity> _dbSet;
    private readonly IMapper _mapper;

    public MeasurementPointHalfHourOperatingReportReadOnlyRepository(
        AppReadOnlyDbContext context,
        ISnowflakeIdService snowflakeIdService,
        IMapper mapper) : base(context, snowflakeIdService)
    {
        _dbContext = context;
        _dbSet = context.Set<MeasurementPointHalfHourOperatingReportEntity>();
        _mapper = mapper;
    }

    public async Task<CursorPaginationResponse<OperatingParameterReportResponse>> GetOperatingParameterReportsAsync(
        OperatingParameterReportRequest request,
        CancellationToken cancellationToken)
    {
        var configuredUnits = await OperatingReportFallbackHelper.GetConfiguredUnitsAsync(
            _dbContext, request.MeasurementPointId, cancellationToken);

        var query = _dbSet.AsNoTracking()
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= request.StartTime && m.CreatedTime <= request.EndTime);

        var result = await query.PaginateWithAscAndAsync<MeasurementPointHalfHourOperatingReportEntity, OperatingParameterReportResponse>(
            request, _mapper, cancellationToken);

        if (result.Data != null && result.Data.Any())
        {
            OperatingReportFallbackHelper.ApplyConfiguredUnits(result.Data, configuredUnits);
            return result;
        }

        // Fallback từ partitioned wide-table MeasurementPointTimeSeries (cửa sổ 12 tháng)
        var fallbackEntities = await OperatingReportFallbackHelper.GetFallbackFromTimeSeriesAsync(
            _dbContext,
            request.MeasurementPointId,
            request.StartTime,
            request.EndTime,
            cancellationToken);

        var fallbackResponses = _mapper.Map<List<OperatingParameterReportResponse>>(fallbackEntities);
        OperatingReportFallbackHelper.ApplyConfiguredUnits(fallbackResponses, configuredUnits);
        return PaginateInMemory(fallbackResponses, request);
    }

    public async Task<List<OperatingParameterReportResponse>> GetAllOperatingParameterReportsAsync(
        OperatingParameterReportRequest request,
        CancellationToken cancellationToken)
    {
        var configuredUnits = await OperatingReportFallbackHelper.GetConfiguredUnitsAsync(
            _dbContext, request.MeasurementPointId, cancellationToken);

        var query = _dbSet.AsNoTracking()
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= request.StartTime && m.CreatedTime <= request.EndTime)
            .OrderBy(m => m.CreatedTime);

        var list = await query.ProjectTo<OperatingParameterReportResponse>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        if (list.Any())
        {
            OperatingReportFallbackHelper.ApplyConfiguredUnits(list, configuredUnits);
            return list;
        }

        var fallbackEntities = await OperatingReportFallbackHelper.GetFallbackFromTimeSeriesAsync(
            _dbContext,
            request.MeasurementPointId,
            request.StartTime,
            request.EndTime,
            cancellationToken);

        var fallbackResponses = _mapper.Map<List<OperatingParameterReportResponse>>(fallbackEntities);
        OperatingReportFallbackHelper.ApplyConfiguredUnits(fallbackResponses, configuredUnits);
        return fallbackResponses;
    }

    private static CursorPaginationResponse<OperatingParameterReportResponse> PaginateInMemory(
        List<OperatingParameterReportResponse> items,
        OperatingParameterReportRequest request)
    {
        var pageSize = request.PageSize > 0 ? request.PageSize : 48;
        var startIndex = 0;

        if (request.PageNumber is > 0 && !request.After.HasValue && !request.Before.HasValue)
        {
            startIndex = (request.PageNumber.Value - 1) * pageSize;
        }
        else if (request.After.HasValue)
        {
            var idx = items.FindIndex(x => x.Id == request.After.Value);
            if (idx >= 0) startIndex = idx + 1;
        }
        else if (request.Before.HasValue)
        {
            var idx = items.FindIndex(x => x.Id == request.Before.Value);
            if (idx >= 0) startIndex = Math.Max(0, idx - pageSize);
        }

        var pagedItems = items.Skip(startIndex).Take(pageSize).ToList();
        var hasPrevious = startIndex > 0 && items.Count > 0;
        var hasNext = (startIndex + pagedItems.Count) < items.Count;

        return new CursorPaginationResponse<OperatingParameterReportResponse>
        {
            Data = pagedItems,
            HasNext = hasNext,
            HasPrevious = hasPrevious,
            TotalCount = items.Count
        };
    }
}
