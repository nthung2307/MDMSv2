using System.Globalization;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Nobisoft.Core.Infrastructure.Interfaces;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Services;

public class BackgroundJobService
{
    private readonly IMeasurementPointMonitoringReadOnlyRepository _measurementPointMonitoringReadOnlyRepository;
    private readonly IMeasurementPointMonitoringHourlyReadWriteRepository _measurementPointMonitoringHourlyReadWriteRepository;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IMeasurementPointMonitoringReadWriteByDateTimeRepository _measurementPointMonitoringReadWriteByDateTimeRepository;
    private readonly IMeasurementPointDailyEnergyReportReadWriteRepository _measurementPointDailyEnergyReportReadWriteRepository;
    private readonly IMeasurementPointHalfHourEnergyReportReadWriteRepository _measurementPointHalfHourEnergyReportReadWriteRepository;
    private readonly IMeasurementPointHalfHourOperatingReportReadWriteRepository _measurementPointHalfHourOperatingReportReadWriteRepository;
    private readonly IMeasurementPointEventReadWriteRepository? _measurementPointEventReadWriteRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<BackgroundJobService>? _logger;

    public BackgroundJobService(
        IMeasurementPointMonitoringReadOnlyRepository measurementPointMonitoringReadOnlyRepository,
        IMeasurementPointMonitoringHourlyReadWriteRepository measurementPointMonitoringHourlyReadWriteRepository,
        ISnowflakeIdService snowflakeIdService,
        IDateTimeService dateTimeService,
        IMeasurementPointMonitoringReadWriteByDateTimeRepository measurementPointMonitoringReadWriteByDateTimeRepository,
        IMeasurementPointDailyEnergyReportReadWriteRepository measurementPointDailyEnergyReportReadWriteRepository,
        IMeasurementPointHalfHourEnergyReportReadWriteRepository measurementPointHalfHourEnergyReportReadWriteRepository,
        IMeasurementPointHalfHourOperatingReportReadWriteRepository measurementPointHalfHourOperatingReportReadWriteRepository,
        IMapper mapper,
        ILogger<BackgroundJobService>? logger = null,
        IMeasurementPointEventReadWriteRepository? measurementPointEventReadWriteRepository = null)
    {
        _measurementPointMonitoringReadOnlyRepository = measurementPointMonitoringReadOnlyRepository;
        _measurementPointMonitoringHourlyReadWriteRepository = measurementPointMonitoringHourlyReadWriteRepository;
        _snowflakeIdService = snowflakeIdService;
        _dateTimeService = dateTimeService;
        _measurementPointMonitoringReadWriteByDateTimeRepository = measurementPointMonitoringReadWriteByDateTimeRepository;
        _measurementPointDailyEnergyReportReadWriteRepository = measurementPointDailyEnergyReportReadWriteRepository;
        _measurementPointHalfHourEnergyReportReadWriteRepository = measurementPointHalfHourEnergyReportReadWriteRepository;
        _measurementPointHalfHourOperatingReportReadWriteRepository = measurementPointHalfHourOperatingReportReadWriteRepository;
        _mapper = mapper;
        _logger = logger;
        _measurementPointEventReadWriteRepository = measurementPointEventReadWriteRepository;
    }

    public async Task<DateTime> GroupDataPerMeter(CancellationToken cancellationToken)
    {
        var cultureInfo = CultureInfo.InvariantCulture;
        var measurementPointsMonitoring2HoursAgo = await _measurementPointMonitoringReadOnlyRepository.GetListMeasurementPoint2HoursAgoAsync(cancellationToken);
        var measurementPointsMonitoringHourly = await _measurementPointMonitoringReadOnlyRepository.GetListMeasurementPointHourlyAsync(cancellationToken);
        foreach (var item in measurementPointsMonitoringHourly)
        {
            var matching2HoursAgoItem = measurementPointsMonitoring2HoursAgo
                .FirstOrDefault(m => m.MeasurementPointsId == item.MeasurementPointsId);

            await _measurementPointMonitoringHourlyReadWriteRepository.AddAsync(new MeasurementPointMonitoringHourlyEntity()
            {
                Id = await _snowflakeIdService.GenerateId(cancellationToken),
                MeasurementPointId = item.MeasurementPointsId,
                CreatedTime = _dateTimeService.Now().AddHours(-1),
                TrungBinhCongSuatPhanKhangTong3Pha = Math.Round(double.Parse(item.CongSuatPhanKhangTong3Pha ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                CongSuatPhanKhangTong3PhaCucDai = Math.Round(double.Parse(item.CongSuatPhanKhangTong3PhaCucDai ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                CongSuatPhanKhangTong3PhaCucTieu = Math.Round(double.Parse(item.CongSuatPhanKhangTong3PhaCucTieu ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                TrungBinhCongSuatPhanKhangTong3PhaUnit = item.CongSuatPhanKhangTong3PhaUnit ?? "",
                TrungBinhCongSuatTacDungTong3Pha = Math.Round(double.Parse(item.CongSuatTacDungTong3Pha ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                CongSuatTacDungTong3PhaCucDai = Math.Round(double.Parse(item.CongSuatTacDungTong3PhaCucDai ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                CongSuatTacDungTong3PhaCucTieu = Math.Round(double.Parse(item.CongSuatTacDungTong3PhaCucTieu ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                CongSuatTacDungTong3PhaUnit = item.CongSuatTacDungTong3PhaUnit ?? "",
                TrungBinhDienApPhaA = Math.Round(double.Parse(item.DienApPhaA ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                TrungBinhDienApPhaAUnit = item.DienApPhaAUnit ?? "",
                TrungBinhDienApPhaB = Math.Round(double.Parse(item.DienApPhaB ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                TrungBinhDienApPhaBUnit = item.DienApPhaBUnit ?? "",
                TrungBinhDienApPhaC = Math.Round(double.Parse(item.DienApPhaC ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                TrungBinhDienApPhaCUnit = item.DienApPhaCUnit ?? "",
                TrungBinhDongDienPhaA = Math.Round(double.Parse(item.DongDienPhaA ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                TrungBinhDongDienPhaAUnit = item.DongDienPhaAUnit ?? "",
                TrungBinhDongDienPhaB = Math.Round(double.Parse(item.DongDienPhaB ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                TrungBinhDongDienPhaBUnit = item.DongDienPhaBUnit ?? "",
                TrungBinhDongDienPhaC = Math.Round(double.Parse(item.DongDienPhaC ?? "0", cultureInfo), 3).ToString(CultureInfo.InvariantCulture),
                TrungBinhDongDienPhaCUnit = item.DongDienPhaCUnit ?? "",
                ChiSoTichLuyDienNangTacDungAPBieuGioBT = item.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null && matching2HoursAgoItem?.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null ? (Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioBT ?? "0", cultureInfo), 3) - Math.Round(double.Parse(matching2HoursAgoItem?.ChiSoTichLuyDienNangTacDungAPBieuGioBT ?? "0", cultureInfo), 3)).ToString(CultureInfo.InvariantCulture) : null,
                ChiSoTichLuyDienNangTacDungAPBieuGioCD = item.ChiSoTichLuyDienNangTacDungAPBieuGioCD != null && matching2HoursAgoItem?.ChiSoTichLuyDienNangTacDungAPBieuGioCD != null ? (Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioCD ?? "0", cultureInfo), 3) - Math.Round(double.Parse(matching2HoursAgoItem?.ChiSoTichLuyDienNangTacDungAPBieuGioCD ?? "0", cultureInfo), 3)).ToString(CultureInfo.InvariantCulture) : null,
                ChiSoTichLuyDienNangTacDungAPBieuGioTD = item.ChiSoTichLuyDienNangTacDungAPBieuGioTD != null && matching2HoursAgoItem?.ChiSoTichLuyDienNangTacDungAPBieuGioTD != null ? (Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioTD ?? "0", cultureInfo), 3) - Math.Round(double.Parse(matching2HoursAgoItem?.ChiSoTichLuyDienNangTacDungAPBieuGioTD ?? "0", cultureInfo), 3)).ToString(CultureInfo.InvariantCulture) : null,
                ChiSoTichLuyDienNangTacDungAPTong = item.ChiSoTichLuyDienNangTacDungAPTong != null && matching2HoursAgoItem?.ChiSoTichLuyDienNangTacDungAPTong != null ? (Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPTong ?? "0", cultureInfo), 3) - Math.Round(double.Parse(matching2HoursAgoItem?.ChiSoTichLuyDienNangTacDungAPTong ?? "0", cultureInfo), 3)).ToString(CultureInfo.InvariantCulture) : null,
            }, cancellationToken);
        }

        await _measurementPointMonitoringHourlyReadWriteRepository.SaveChangesAsync(cancellationToken);

        var measurementPointMonitoringByDateTime = await _measurementPointMonitoringReadWriteByDateTimeRepository.GetMeasurementPointMonitoringDateTimeAsync(cancellationToken);
        await _measurementPointMonitoringReadWriteByDateTimeRepository.RemoveRangeAsync(measurementPointMonitoringByDateTime, cancellationToken);
        await _measurementPointMonitoringReadWriteByDateTimeRepository.SaveChangesAsync(cancellationToken);
        return new DateTime();
    }

    public async Task<DateTime> GenerateEnergyReportDaily(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Bắt đầu sinh báo cáo đo đếm ngày với Smart Lookback (120 phút)...");
        var createdTime = GetNearestHalfHourSlot(DateTimeOffset.UtcNow.AddMinutes(-5));
        var reportDtos = await _measurementPointMonitoringReadOnlyRepository.GetListMeasurementEnergyReportWithLookbackAsync(120, cancellationToken, createdTime);
        foreach (var dto in reportDtos)
        {
            var entity = _mapper.Map<MeasurementPointDailyEnergyReportEntity>(dto);
            entity.Id = await _snowflakeIdService.GenerateId(cancellationToken);
            entity.CreatedTime = createdTime;
            await _measurementPointDailyEnergyReportReadWriteRepository.AddAsync(entity, cancellationToken);
        }

        await _measurementPointDailyEnergyReportReadWriteRepository.SaveChangesAsync(cancellationToken);
        _logger?.LogInformation("Đã hoàn tất lưu báo cáo đo đếm ngày cho {Count} điểm đo lúc {Slot}.", reportDtos.Count, createdTime);
        return new DateTime();
    }
    
    private static DateTimeOffset GetNearestHalfHourSlot(DateTimeOffset time)
    {
        int minute = time.Minute;
        int roundedMinute = minute < 15 ? 0 : (minute < 45 ? 30 : 0);
        var baseTime = minute >= 45 ? time.AddHours(1) : time;
        return new DateTimeOffset(baseTime.Year, baseTime.Month, baseTime.Day, baseTime.Hour, roundedMinute, 0, baseTime.Offset);
    }

    public async Task<DateTime> GenerateEnergyReportHalfHour(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Bắt đầu sinh báo cáo đo đếm nửa giờ với Smart Lookback (120 phút)...");
        var createdTime = GetNearestHalfHourSlot(DateTimeOffset.UtcNow.AddMinutes(-5));
        var reportDtos = await _measurementPointMonitoringReadOnlyRepository.GetListMeasurementEnergyReportWithLookbackAsync(120, cancellationToken, createdTime);
        foreach (var dto in reportDtos)
        {
            var entity = _mapper.Map<MeasurementPointHalfHourEnergyReportEntity>(dto);
            entity.Id = await _snowflakeIdService.GenerateId(cancellationToken);
            entity.CreatedTime = createdTime;
            await _measurementPointHalfHourEnergyReportReadWriteRepository.AddAsync(entity, cancellationToken);
        }

        await _measurementPointHalfHourEnergyReportReadWriteRepository.SaveChangesAsync(cancellationToken);
        _logger?.LogInformation("Đã hoàn tất lưu báo cáo đo đếm nửa giờ cho {Count} điểm đo lúc {Slot}.", reportDtos.Count, createdTime);
        return new DateTime();
    }

    public async Task<DateTime> GenerateOperatingReportHalfHour(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("Bắt đầu sinh báo cáo thông số vận hành nửa giờ với Smart Lookback (120 phút)...");
        var createdTime = GetNearestHalfHourSlot(DateTimeOffset.UtcNow.AddMinutes(-5));
        var reportDtos = await _measurementPointMonitoringReadOnlyRepository.GetListMeasurementOperatingReportWithLookbackAsync(120, cancellationToken, createdTime);
        foreach (var dto in reportDtos)
        {
            var entity = _mapper.Map<MeasurementPointHalfHourOperatingReportEntity>(dto);
            entity.Id = await _snowflakeIdService.GenerateId(cancellationToken);
            entity.CreatedTime = createdTime;
            await _measurementPointHalfHourOperatingReportReadWriteRepository.AddAsync(entity, cancellationToken);
        }

        await _measurementPointHalfHourOperatingReportReadWriteRepository.SaveChangesAsync(cancellationToken);
        _logger?.LogInformation("Đã hoàn tất lưu báo cáo thông số vận hành nửa giờ cho {Count} điểm đo lúc {Slot}.", reportDtos.Count, createdTime);
        return new DateTime();
    }

    public async Task<DateTime> CleanupOldEvents(CancellationToken cancellationToken)
    {
        if (_measurementPointEventReadWriteRepository != null)
        {
            _logger?.LogInformation("Bắt đầu dọn dẹp các sự kiện cũ hơn 3 tháng...");
            var count = await _measurementPointEventReadWriteRepository.CleanupOldEventsAsync(3, cancellationToken);
            _logger?.LogInformation("Đã xóa {Count} sự kiện cũ hơn 3 tháng để giải phóng dung lượng.", count);
        }
        return DateTime.UtcNow;
    }
}