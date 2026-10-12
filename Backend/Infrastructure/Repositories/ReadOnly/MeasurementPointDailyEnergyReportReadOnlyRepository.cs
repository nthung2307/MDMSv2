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
}