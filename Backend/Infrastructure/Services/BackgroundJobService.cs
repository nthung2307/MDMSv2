using System.Globalization;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using AutoMapper;
using Domain.Entities;
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
    private readonly IMapper _mapper;

    public BackgroundJobService(
        IMeasurementPointMonitoringReadOnlyRepository measurementPointMonitoringReadOnlyRepository,
        IMeasurementPointMonitoringHourlyReadWriteRepository measurementPointMonitoringHourlyReadWriteRepository,
        ISnowflakeIdService snowflakeIdService,
        IDateTimeService dateTimeService,
        IMeasurementPointMonitoringReadWriteByDateTimeRepository measurementPointMonitoringReadWriteByDateTimeRepository,
        IMeasurementPointDailyEnergyReportReadWriteRepository measurementPointDailyEnergyReportReadWriteRepository,
        IMeasurementPointHalfHourEnergyReportReadWriteRepository measurementPointHalfHourEnergyReportReadWriteRepository,
        IMapper mapper)
    {
        _measurementPointMonitoringReadOnlyRepository = measurementPointMonitoringReadOnlyRepository;
        _measurementPointMonitoringHourlyReadWriteRepository = measurementPointMonitoringHourlyReadWriteRepository;
        _snowflakeIdService = snowflakeIdService;
        _dateTimeService = dateTimeService;
        _measurementPointMonitoringReadWriteByDateTimeRepository = measurementPointMonitoringReadWriteByDateTimeRepository;
        _measurementPointDailyEnergyReportReadWriteRepository = measurementPointDailyEnergyReportReadWriteRepository;
        _measurementPointHalfHourEnergyReportReadWriteRepository = measurementPointHalfHourEnergyReportReadWriteRepository;
        _mapper = mapper;
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
        var reportDtos = await _measurementPointMonitoringReadOnlyRepository.GetListMeasurementEnergyReportAsync(cancellationToken);
        var createdTime = DateTimeOffset.UtcNow.AddMinutes(-5);
        foreach (var dto in reportDtos)
        {
            var entity = _mapper.Map<MeasurementPointDailyEnergyReportEntity>(dto);
            entity.Id = await _snowflakeIdService.GenerateId(cancellationToken);
            entity.CreatedTime = createdTime;
            await _measurementPointDailyEnergyReportReadWriteRepository.AddAsync(entity, cancellationToken);
        }

        await _measurementPointDailyEnergyReportReadWriteRepository.SaveChangesAsync(cancellationToken);
        return new DateTime();
    }
    
    public async Task<DateTime> GenerateEnergyReportHalfHour(CancellationToken cancellationToken)
    {
        var reportDtos = await _measurementPointMonitoringReadOnlyRepository.GetListMeasurementEnergyReportAsync(cancellationToken);
        var createdTime = DateTimeOffset.UtcNow.AddMinutes(-5);
        foreach (var dto in reportDtos)
        {
            var entity = _mapper.Map<MeasurementPointHalfHourEnergyReportEntity>(dto);
            entity.Id = await _snowflakeIdService.GenerateId(cancellationToken);
            entity.CreatedTime = createdTime;
            await _measurementPointHalfHourEnergyReportReadWriteRepository.AddAsync(entity, cancellationToken);
        }

        await _measurementPointHalfHourEnergyReportReadWriteRepository.SaveChangesAsync(cancellationToken);
        return new DateTime();
    }
}