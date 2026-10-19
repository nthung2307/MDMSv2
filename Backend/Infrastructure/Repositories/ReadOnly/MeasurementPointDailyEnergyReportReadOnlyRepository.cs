using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Application.Extensions;
using Application.Repositories.ReadOnly;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class MeasurementPointDailyEnergyReportReadOnlyRepository :
    RestfulRepository<MeasurementPointDailyEnergyReportEntity, AppReadOnlyDbContext>, IMeasurementPointDailyEnergyReportReadOnlyRepository
{
    private readonly DbSet<MeasurementPointDailyEnergyReportEntity> _measurementPointDailyEnergyReport;
    private readonly IMapper _mapper;
    
    public MeasurementPointDailyEnergyReportReadOnlyRepository(AppReadOnlyDbContext context, ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(context, snowflakeIdService)
    {
        _mapper = mapper;
        _measurementPointDailyEnergyReport = context.Set<MeasurementPointDailyEnergyReportEntity>();
    }
    
    public async Task<CursorPaginationResponse<FinalizedEnergyReportResponse>> GetFinalizedEnergyReportsAsync(
        DailyFinalizedEnergyReportRequest request, 
        CancellationToken cancellationToken)
    {
        var query = _measurementPointDailyEnergyReport.AsNoTracking();
        var startLocal = new DateTimeOffset(request.Year, request.Month, 1, 0, 0, 0, TimeSpan.FromHours(7));
        var endLocal = startLocal.AddMonths(1);
        var startTime = startLocal.UtcDateTime;
        var endTime = endLocal.UtcDateTime;
        var result = await query
            .Where(m => m.MeasurementPoint.CustomerId == request.CustomerId)
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= startTime && m.CreatedTime < endTime)
            .PaginateWithAscAndAsync<MeasurementPointDailyEnergyReportEntity, FinalizedEnergyReportResponse>(request, _mapper, cancellationToken);
        return result;
    }

    public async Task<CursorPaginationResponse<FinalizedEnergyReportResponse>> GetYearlyFinalizedEnergyReportsAsync(
        YearlyFinalizedEnergyReportRequest request,
        CancellationToken cancellationToken)
    {
        var startLocal = new DateTimeOffset(request.Year, 1, 1, 0, 0, 0, TimeSpan.FromHours(7));
        var endLocal = new DateTimeOffset(request.Year + 1, 1, 2, 23, 59, 59, TimeSpan.FromHours(7));
        var startTime = startLocal.UtcDateTime;
        var endTime = endLocal.UtcDateTime;

        var allDailyRecords = await _measurementPointDailyEnergyReport.AsNoTracking()
            .Where(m => m.MeasurementPoint.CustomerId == request.CustomerId)
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= startTime && m.CreatedTime <= endTime)
            .OrderBy(m => m.CreatedTime)
            .ToListAsync(cancellationToken);

        // Lọc lấy bản ghi ngày đầu tiên (Day == 1) của mỗi tháng từ tháng 1 đến tháng 12 năm request.Year và tháng 1 năm sau
        var targetMonths = new List<(int Year, int Month)>();
        for (int m = 1; m <= 12; m++)
        {
            targetMonths.Add((request.Year, m));
        }
        targetMonths.Add((request.Year + 1, 1));

        var yearlyRecords = new List<MeasurementPointDailyEnergyReportEntity>();
        foreach (var (y, m) in targetMonths)
        {
            var recordForMonth = allDailyRecords.FirstOrDefault(r =>
            {
                if (!r.CreatedTime.HasValue) return false;
                var localTime = r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7));
                return localTime.Year == y && localTime.Month == m && localTime.Day == 1;
            });

            // Nếu không có chính xác ngày 1, lấy bản ghi đầu tiên trong tháng đó
            if (recordForMonth == null)
            {
                recordForMonth = allDailyRecords.FirstOrDefault(r =>
                {
                    if (!r.CreatedTime.HasValue) return false;
                    var localTime = r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7));
                    return localTime.Year == y && localTime.Month == m;
                });
            }

            if (recordForMonth != null)
            {
                yearlyRecords.Add(recordForMonth);
            }
        }

        var mappedItems = _mapper.Map<List<FinalizedEnergyReportResponse>>(yearlyRecords);
        return new CursorPaginationResponse<FinalizedEnergyReportResponse>
        {
            Data = mappedItems,
            HasNext = false,
            HasPrevious = false,
            TotalCount = mappedItems.Count
        };
    }
}