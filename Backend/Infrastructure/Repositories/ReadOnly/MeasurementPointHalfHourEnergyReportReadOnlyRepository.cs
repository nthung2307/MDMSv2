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

public class MeasurementPointHalfHourEnergyReportReadOnlyRepository :
    RestfulRepository<MeasurementPointHalfHourEnergyReportEntity, AppReadOnlyDbContext>, IMeasurementPointHalfHourEnergyReportReadOnlyRepository
{
    private readonly DbSet<MeasurementPointHalfHourEnergyReportEntity> _measurementPointHalfHourEnergyReport;
    private readonly IMapper _mapper;
    
    public MeasurementPointHalfHourEnergyReportReadOnlyRepository(AppReadOnlyDbContext context, ISnowflakeIdService snowflakeIdService, IMapper mapper) : base(context, snowflakeIdService)
    {
        _measurementPointHalfHourEnergyReport = context.Set<MeasurementPointHalfHourEnergyReportEntity>();
        _mapper = mapper;
    }

    public async Task<CursorPaginationResponse<FinalizedEnergyReportResponse>> GetFinalizedEnergyReportsAsync(
        HalfHourFinalizedEnergyReportRequest request, 
        CancellationToken cancellationToken)
    {
        var measurementPointHalfHourEnergyReportEntity = _measurementPointHalfHourEnergyReport.AsNoTracking();
        var result = await measurementPointHalfHourEnergyReportEntity.Where(m => m.MeasurementPoint.CustomerId == request.CustomerId)
            .Where(m => m.MeasurementPoint.CustomerId == request.CustomerId)
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= request.StartTime && m.CreatedTime <= request.EndTime)
            .PaginateWithAscAndAsync<MeasurementPointHalfHourEnergyReportEntity, FinalizedEnergyReportResponse>(request, _mapper, cancellationToken);
        return result;
    }
}