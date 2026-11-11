using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Services;

public class BackgroundJobService
{
    private readonly IMeasurementPointMonitoringReadOnlyRepository _measurementPointMonitoringReadOnlyRepository;
    private readonly IMeasurementPointDailyEnergyReportReadWriteRepository _measurementPointDailyEnergyReportReadWriteRepository;
    private readonly IMeasurementPointHalfHourEnergyReportReadWriteRepository _measurementPointHalfHourEnergyReportReadWriteRepository;
    private readonly IMeasurementPointHalfHourOperatingReportReadWriteRepository _measurementPointHalfHourOperatingReportReadWriteRepository;
    private readonly IMeasurementPointEventReadWriteRepository _measurementPointEventReadWriteRepository;
    private readonly IPartitionManagementService _partitionManagementService;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IMapper _mapper;
    private readonly ILogger<BackgroundJobService>? _logger;

    public BackgroundJobService(
        IMeasurementPointMonitoringReadOnlyRepository measurementPointMonitoringReadOnlyRepository,
        IMeasurementPointDailyEnergyReportReadWriteRepository measurementPointDailyEnergyReportReadWriteRepository,
        IMeasurementPointHalfHourEnergyReportReadWriteRepository measurementPointHalfHourEnergyReportReadWriteRepository,
        IMeasurementPointHalfHourOperatingReportReadWriteRepository measurementPointHalfHourOperatingReportReadWriteRepository,
        IMeasurementPointEventReadWriteRepository measurementPointEventReadWriteRepository,
        IPartitionManagementService partitionManagementService,
        ISnowflakeIdService snowflakeIdService,
        IMapper mapper,
        ILogger<BackgroundJobService>? logger = null)
    {
        _measurementPointMonitoringReadOnlyRepository = measurementPointMonitoringReadOnlyRepository;
        _measurementPointDailyEnergyReportReadWriteRepository = measurementPointDailyEnergyReportReadWriteRepository;
        _measurementPointHalfHourEnergyReportReadWriteRepository = measurementPointHalfHourEnergyReportReadWriteRepository;
        _measurementPointHalfHourOperatingReportReadWriteRepository = measurementPointHalfHourOperatingReportReadWriteRepository;
        _measurementPointEventReadWriteRepository = measurementPointEventReadWriteRepository;
        _partitionManagementService = partitionManagementService;
        _snowflakeIdService = snowflakeIdService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<DateTime> GenerateEnergyReportDaily(CancellationToken cancellationToken)
    {
        var createdTime = GetNearestHalfHourSlot(DateTimeOffset.UtcNow.AddMinutes(-5));
        var reportDtos = await _measurementPointMonitoringReadOnlyRepository
            .GetListMeasurementEnergyReportWithLookbackAsync(120, cancellationToken, createdTime);

        var existingPointIds = await _measurementPointDailyEnergyReportReadWriteRepository
            .Queryable()
            .AsNoTracking()
            .Where(x => x.CreatedTime == createdTime)
            .Select(x => x.MeasurementPointId)
            .ToListAsync(cancellationToken);
        var existingSet = existingPointIds.ToHashSet();

        foreach (var dto in reportDtos)
        {
            if (existingSet.Contains(dto.MeasurementPointId))
            {
                continue;
            }

            var entity = _mapper.Map<MeasurementPointDailyEnergyReportEntity>(dto);
            entity.Id = await _snowflakeIdService.GenerateId(cancellationToken);
            entity.CreatedTime = createdTime;
            await _measurementPointDailyEnergyReportReadWriteRepository.AddAsync(entity, cancellationToken);
        }

        await _measurementPointDailyEnergyReportReadWriteRepository.SaveChangesAsync(cancellationToken);
        _logger?.LogInformation("Đã hoàn tất lưu báo cáo đo đếm ngày cho {Count} điểm đo lúc {Slot}.", reportDtos.Count, createdTime);
        return DateTime.UtcNow;
    }

    private static DateTimeOffset GetNearestHalfHourSlot(DateTimeOffset time)
    {
        var minute = time.Minute;
        var roundedMinute = minute < 15 ? 0 : (minute < 45 ? 30 : 0);
        var baseTime = minute >= 45 ? time.AddHours(1) : time;
        return new DateTimeOffset(baseTime.Year, baseTime.Month, baseTime.Day, baseTime.Hour, roundedMinute, 0, baseTime.Offset);
    }

    public async Task<DateTime> GenerateEnergyReportHalfHour(CancellationToken cancellationToken)
    {
        var createdTime = GetNearestHalfHourSlot(DateTimeOffset.UtcNow.AddMinutes(-5));
        var reportDtos = await _measurementPointMonitoringReadOnlyRepository
            .GetListMeasurementEnergyReportWithLookbackAsync(120, cancellationToken, createdTime);

        var existingPointIds = await _measurementPointHalfHourEnergyReportReadWriteRepository
            .Queryable()
            .AsNoTracking()
            .Where(x => x.CreatedTime == createdTime)
            .Select(x => x.MeasurementPointId)
            .ToListAsync(cancellationToken);
        var existingSet = existingPointIds.ToHashSet();

        foreach (var dto in reportDtos)
        {
            if (existingSet.Contains(dto.MeasurementPointId))
            {
                continue;
            }

            var entity = _mapper.Map<MeasurementPointHalfHourEnergyReportEntity>(dto);
            entity.Id = await _snowflakeIdService.GenerateId(cancellationToken);
            entity.CreatedTime = createdTime;
            await _measurementPointHalfHourEnergyReportReadWriteRepository.AddAsync(entity, cancellationToken);
        }

        await _measurementPointHalfHourEnergyReportReadWriteRepository.SaveChangesAsync(cancellationToken);
        _logger?.LogInformation("Đã hoàn tất lưu báo cáo đo đếm nửa giờ cho {Count} điểm đo lúc {Slot}.", reportDtos.Count, createdTime);
        return DateTime.UtcNow;
    }

    public async Task<DateTime> GenerateOperatingReportHalfHour(CancellationToken cancellationToken)
    {
        var createdTime = GetNearestHalfHourSlot(DateTimeOffset.UtcNow.AddMinutes(-5));
        var reportDtos = await _measurementPointMonitoringReadOnlyRepository
            .GetListMeasurementOperatingReportWithLookbackAsync(120, cancellationToken, createdTime);

        var existingPointIds = await _measurementPointHalfHourOperatingReportReadWriteRepository
            .Queryable()
            .AsNoTracking()
            .Where(x => x.CreatedTime == createdTime)
            .Select(x => x.MeasurementPointId)
            .ToListAsync(cancellationToken);
        var existingSet = existingPointIds.ToHashSet();

        foreach (var dto in reportDtos)
        {
            if (existingSet.Contains(dto.MeasurementPointId))
            {
                continue;
            }

            var entity = _mapper.Map<MeasurementPointHalfHourOperatingReportEntity>(dto);
            entity.Id = await _snowflakeIdService.GenerateId(cancellationToken);
            entity.CreatedTime = createdTime;
            await _measurementPointHalfHourOperatingReportReadWriteRepository.AddAsync(entity, cancellationToken);
        }

        await _measurementPointHalfHourOperatingReportReadWriteRepository.SaveChangesAsync(cancellationToken);
        _logger?.LogInformation("Đã hoàn tất lưu báo cáo thông số vận hành nửa giờ cho {Count} điểm đo lúc {Slot}.", reportDtos.Count, createdTime);
        return DateTime.UtcNow;
    }

    public async Task<DateTime> MaintainTimeSeriesPartitions(CancellationToken cancellationToken)
    {
        await _partitionManagementService.EnsureAndCleanupPartitionsAsync(
            monthsAhead: 3,
            retentionMonths: 12,
            cancellationToken: cancellationToken);
        _logger?.LogInformation("Đã hoàn tất kiểm tra và bảo trì Partition cho bảng MeasurementPointTimeSeries (12 tháng).");
        return DateTime.UtcNow;
    }

    public async Task<DateTime> CleanupOldEvents(CancellationToken cancellationToken)
    {
        var deletedCount = await _measurementPointEventReadWriteRepository.CleanupOldEventsAsync(3, cancellationToken);
        _logger?.LogInformation("Đã dọn dẹp {Count} bản ghi sự kiện điểm đo cũ quá 3 tháng.", deletedCount);
        return DateTime.UtcNow;
    }
}
