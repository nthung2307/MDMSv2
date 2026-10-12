using Application.Repositories.ReadWrite;
using Domain.Entities;
using Nobisoft.Core.Infrastructure.Interfaces;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Services;

public class BackgroundJobArchiveDataService
{
    private readonly IMeasurementPointMonitoringReadWriteRepository _measurementPointMonitoringReadWriteRepository;
    private readonly IArchiveDataMeasurementPointMonitoringReadWriteRepository _archiveDataMeasurementPointMonitoringReadWriteRepository;

    public BackgroundJobArchiveDataService(IMeasurementPointMonitoringReadWriteRepository measurementPointMonitoringReadWriteRepository, IArchiveDataMeasurementPointMonitoringReadWriteRepository archiveDataMeasurementPointMonitoringReadWriteRepository, ISnowflakeIdService snowflakeIdService, IDateTimeService dateTimeService)
    {
        _measurementPointMonitoringReadWriteRepository = measurementPointMonitoringReadWriteRepository;
        _archiveDataMeasurementPointMonitoringReadWriteRepository = archiveDataMeasurementPointMonitoringReadWriteRepository;
    }

    public async Task ArchiveDataMeasurementPointMonitoring(CancellationToken cancellationToken)
    {
        // var measurementPointsMonitoring = await _measurementPointMonitoringReadWriteRepository.GetListMeasurementPointMonitoringAsync(cancellationToken);
        //
        // var archiveDataMeasurementPointsMonitoring = measurementPointsMonitoring.Select(item => new ArchiveDataMeasurementPointMonitoringEntity
        // {
        //     Id = item.Id,
        //     MeasurementPointsId = item.MeasurementPointsId,
        //     TagNameId = item.TagNameId,
        //     TagNameDescription = item.TagNameDescription,
        //     Value = item.Value,
        //     Quanlity = item.Quanlity,
        //     GroupId = item.GroupId,
        //     CreatedTime = item.CreatedTime,
        //     CreatedBy = item.CreatedBy,
        //     CreatedName = item.CreatedName
        // }).ToList();
        //
        // await _archiveDataMeasurementPointMonitoringReadWriteRepository.AddRangeAsync(archiveDataMeasurementPointsMonitoring, cancellationToken);
        // await _archiveDataMeasurementPointMonitoringReadWriteRepository.SaveChangesAsync(cancellationToken);
        //
        // await _measurementPointMonitoringReadWriteRepository.RemoveRangeAsync(measurementPointsMonitoring, cancellationToken);
        // await _measurementPointMonitoringReadWriteRepository.SaveChangesAsync(cancellationToken);
        await Task.CompletedTask;
    }

}