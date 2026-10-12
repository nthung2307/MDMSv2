using System.Globalization;
using Application.Commands.Gcs;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.Gcs;
using Application.Handlers.Commands.Gcs;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Gcs;

public class FinalMeterReadingCommandHandler : IFinalMeterReadingCommandHandler
{
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly ILoggerService _loggerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGcsMonthlyDetailReadWriteRepository _gcsMonthlyDetailReadWriteRepository;
    private readonly IGcsCutOffCycleReadWriteRepository _gcsCutOffCycleReadWriteRepository;
    private readonly IGcsTemporaryReadWriteRepository _gcsTemporaryReadWriteRepository;
    private readonly IPeriodicEnergyStatementReadWriteRepository _periodicEnergyStatementReadWriteRepository;
    private readonly IPeriodicEnergyStatementReadOnlyRepository _periodicEnergyStatementReadOnlyRepository;
    private readonly IGcsReadOnlyRepository _gcsReadOnlyRepository;
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;
    private readonly IGcsReadWriteRepository _gcsReadWriteRepository;
    private readonly IGcsCutOffCycleReadOnlyRepository _gcsCutOffCycleReadOnlyRepository;

    public FinalMeterReadingCommandHandler(ISnowflakeIdService snowflakeIdService, ILoggerService loggerService, ICurrentUserService currentUserService, IGcsMonthlyDetailReadWriteRepository gcsMonthlyDetailReadWriteRepository, IGcsCutOffCycleReadWriteRepository gcsCutOffCycleReadWriteRepository, IGcsTemporaryReadWriteRepository gcsTemporaryReadWriteRepository, IPeriodicEnergyStatementReadWriteRepository periodicEnergyStatementReadWriteRepository, IPeriodicEnergyStatementReadOnlyRepository periodicEnergyStatementReadOnlyRepository, IGcsReadOnlyRepository gcsReadOnlyRepository, IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository, IGcsReadWriteRepository gcsReadWriteRepository, IGcsCutOffCycleReadOnlyRepository gcsCutOffCycleReadOnlyRepository)
    {
        _snowflakeIdService = snowflakeIdService;
        _loggerService = loggerService;
        _currentUserService = currentUserService;
        _gcsMonthlyDetailReadWriteRepository = gcsMonthlyDetailReadWriteRepository;
        _gcsCutOffCycleReadWriteRepository = gcsCutOffCycleReadWriteRepository;
        _gcsTemporaryReadWriteRepository = gcsTemporaryReadWriteRepository;
        _periodicEnergyStatementReadWriteRepository = periodicEnergyStatementReadWriteRepository;
        _periodicEnergyStatementReadOnlyRepository = periodicEnergyStatementReadOnlyRepository;
        _gcsReadOnlyRepository = gcsReadOnlyRepository;
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
        _gcsReadWriteRepository = gcsReadWriteRepository;
        _gcsCutOffCycleReadOnlyRepository = gcsCutOffCycleReadOnlyRepository;
    }

    public async Task<RequestResult<GcsResponse>> Handle(FinalMeterReadingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var checkExistGcsTemporaryErrorAsync = await _gcsTemporaryReadWriteRepository.CheckExistGcsTemporaryAsync(request.Request.GcsId, request.Request.Month, request.Request.Year, request.Request.PeriodNumber, cancellationToken);
            if (checkExistGcsTemporaryErrorAsync)
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.GcsId),
                        Error = "Còn chỉ số lỗi"
                    }
                });
            }

            var gcs = await _gcsReadWriteRepository.GetGcsByIdAsync(request.Request.GcsId, cancellationToken);
            if (gcs == null)
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.GcsId),
                        Error = "Không tìm thấy sổ ghi chốt"
                    }
                });
            }

            var gcsTemporary = await _gcsTemporaryReadWriteRepository.GetListGcsTemporaryAsync(new GetDataLatchRequest()
            {
                GcsId = request.Request.GcsId,
                Month = request.Request.Month,
                Year = request.Request.Year,
                PeriodNumber = request.Request.PeriodNumber,
                IsRefresh = false
            }, cancellationToken);

            var groupedTemporary = gcsTemporary.GroupBy(x => x.MeasurementPointId)
                .Select(group => new
                {
                    MeasurementPointId = group.Key,
                    Bt = group.FirstOrDefault(x => x.MeterReadingSet == "BT")?.Quanlity == null || group.FirstOrDefault(x => x.MeterReadingSet == "BT")?.Quanlity == false ? group.FirstOrDefault(x => x.MeterReadingSet == "BT")?.NewCurrentReading ?? 0 : group.FirstOrDefault(x => x.MeterReadingSet == "BT")?.CurrentReading ?? 0,
                    Cd = group.FirstOrDefault(x => x.MeterReadingSet == "CD")?.Quanlity == null || group.FirstOrDefault(x => x.MeterReadingSet == "CD")?.Quanlity == false ? group.FirstOrDefault(x => x.MeterReadingSet == "CD")?.NewCurrentReading ?? 0 : group.FirstOrDefault(x => x.MeterReadingSet == "CD")?.CurrentReading ?? 0,
                    Td = group.FirstOrDefault(x => x.MeterReadingSet == "TD")?.Quanlity == null || group.FirstOrDefault(x => x.MeterReadingSet == "TD")?.Quanlity == false ? group.FirstOrDefault(x => x.MeterReadingSet == "TD")?.NewCurrentReading ?? 0 : group.FirstOrDefault(x => x.MeterReadingSet == "TD")?.CurrentReading ?? 0,
                    Sg = group.FirstOrDefault(x => x.MeterReadingSet == "SG")?.Quanlity == null || group.FirstOrDefault(x => x.MeterReadingSet == "SG")?.Quanlity == false ? group.FirstOrDefault(x => x.MeterReadingSet == "SG")?.NewCurrentReading ?? 0 : group.FirstOrDefault(x => x.MeterReadingSet == "SG")?.CurrentReading ?? 0,
                    Vc = group.FirstOrDefault(x => x.MeterReadingSet == "VC")?.Quanlity == null || group.FirstOrDefault(x => x.MeterReadingSet == "VC")?.Quanlity == false ? group.FirstOrDefault(x => x.MeterReadingSet == "VC")?.NewCurrentReading ?? 0 : group.FirstOrDefault(x => x.MeterReadingSet == "VC")?.CurrentReading ?? 0,
                    Pmax = group.FirstOrDefault(x => x.MeterReadingSet == "Pmax")?.Quanlity == null || group.FirstOrDefault(x => x.MeterReadingSet == "Pmax")?.Quanlity == false ? group.FirstOrDefault(x => x.MeterReadingSet == "Pmax")?.NewCurrentReading ?? 0 : group.FirstOrDefault(x => x.MeterReadingSet == "Pmax")?.CurrentReading ?? 0,
                    PreviousBt = group.FirstOrDefault(x => x.MeterReadingSet == "BT")?.NewPreviousReading ?? group.FirstOrDefault(x => x.MeterReadingSet == "BT")?.PreviousReading ?? 0,
                    PreviousCd = group.FirstOrDefault(x => x.MeterReadingSet == "CD")?.NewPreviousReading ?? group.FirstOrDefault(x => x.MeterReadingSet == "CD")?.PreviousReading ?? 0,
                    PreviousTd = group.FirstOrDefault(x => x.MeterReadingSet == "TD")?.NewPreviousReading ?? group.FirstOrDefault(x => x.MeterReadingSet == "TD")?.PreviousReading ?? 0,
                    PreviousSg = group.FirstOrDefault(x => x.MeterReadingSet == "SG")?.NewPreviousReading ?? group.FirstOrDefault(x => x.MeterReadingSet == "SG")?.PreviousReading ?? 0,
                    PreviousVc = group.FirstOrDefault(x => x.MeterReadingSet == "VC")?.NewPreviousReading ?? group.FirstOrDefault(x => x.MeterReadingSet == "VC")?.PreviousReading ?? 0,
                    PreviousPmax = group.FirstOrDefault(x => x.MeterReadingSet == "Pmax")?.NewPreviousReading ?? group.FirstOrDefault(x => x.MeterReadingSet == "Pmax")?.PreviousReading ?? 0,
                    Multiplier = group.FirstOrDefault(x => x.MeterReadingSet == "BT")?.MultiplierFactor ?? 0,
                    ConsumtionBt = group.FirstOrDefault(x => x.MeterReadingSet == "BT")?.Consumption ?? 0,
                    ConsumtionCd = group.FirstOrDefault(x => x.MeterReadingSet == "CD")?.Consumption ?? 0,
                    ConsumtionTd = group.FirstOrDefault(x => x.MeterReadingSet == "TD")?.Consumption ?? 0,
                    ConsumtionSg = group.FirstOrDefault(x => x.MeterReadingSet == "SG")?.Consumption ?? 0,
                    ConsumtionVc = group.FirstOrDefault(x => x.MeterReadingSet == "VC")?.Consumption ?? 0,
                    PreviousDay = group.FirstOrDefault()?.PreviousDay,
                    PreviousMonth = group.FirstOrDefault()?.PreviousMonth,
                    PreviousYear = group.FirstOrDefault()?.PreviousYear,
                    PreviousPeriodNumber = group.FirstOrDefault()?.PreviousPeriodNumber,
                    Day = group.FirstOrDefault()?.Day,
                    Month = group.FirstOrDefault()?.Month,
                    Year = group.FirstOrDefault()?.Year,
                    PeriodNumber = group.FirstOrDefault()?.PeriodNumber
                }).ToList();

            var gcsCutOffEntity = await _gcsCutOffCycleReadWriteRepository.GetGcsCutOffCycleEntityAsync(request.Request.GcsId, request.Request.Month, request.Request.Year, request.Request.PeriodNumber, cancellationToken);
            if (gcsCutOffEntity != null)
            {
                return RequestResult<GcsResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.GcsId),
                        Error = $"Kì {request.Request.PeriodNumber} Tháng {request.Request.Month} năm {request.Request.Year} đã được chốt"
                    }
                });
            }
            var gcsCutoffCycleId = await _snowflakeIdService.GenerateId(cancellationToken);
            var gcsCutoffCycleOld = await _gcsCutOffCycleReadOnlyRepository.GetGcsCutOffCycleAsync(request.Request.GcsId, cancellationToken);
            var gcsCutOffCycle = new GcsCutoffCycleEntity()
            {
                Id = gcsCutoffCycleId,
                GcsId = request.Request.GcsId,
                Month = request.Request.Month,
                Year = request.Request.Year,
                PeriodNumber = request.Request.PeriodNumber,
                CreatedTime = DateTimeOffset.Now,
                CreatedBy = currentId,
                CreatedName = userName,
                PreviousDay = gcsCutoffCycleOld?.Day,
                PreviousMonth = gcsCutoffCycleOld?.Month,
                PreviousYear = gcsCutoffCycleOld?.Year,
                PreviousPeriodNumber = gcsCutoffCycleOld?.PeriodNumber
            };

            foreach (var item in gcsTemporary)
            {
                gcsCutOffCycle.Day = item.Day;
                await _gcsMonthlyDetailReadWriteRepository.AddAsync(new GcsMonthlyDetailEntity
                {
                    Id = await _snowflakeIdService.GenerateId(cancellationToken),
                    GcsCutoffCycleId = gcsCutoffCycleId,
                    MeasurementPointId = item.MeasurementPointId,
                    MeterReadingSet = item.MeterReadingSet,
                    PreviousReading = item.NewPreviousReading ?? item.PreviousReading,
                    CurrentReading = item.Quanlity != null && item.Quanlity != false && item.ModifiedBy == null ? item.CurrentReading : item.NewCurrentReading ?? item.CurrentReading,
                    MultiplierFactor = item.MultiplierFactor,
                    Consumption = item.Consumption,
                    IsChange = item.ModifiedBy != null,
                    CreatedTime = DateTimeOffset.Now,
                    CreatedBy = currentId,
                    CreatedName = userName
                }, cancellationToken);
            }

            await _gcsCutOffCycleReadWriteRepository.AddAsync(gcsCutOffCycle, cancellationToken);
            await _gcsCutOffCycleReadWriteRepository.SaveChangesAsync(cancellationToken);

            var maxSerialNumberPeriodicEnergyStatementAsync = await _periodicEnergyStatementReadOnlyRepository.GetMaxSerialNumberPeriodicEnergyStatementAsync(cancellationToken);

            foreach (var group in groupedTemporary)
            {
                maxSerialNumberPeriodicEnergyStatementAsync++;
                await _periodicEnergyStatementReadWriteRepository.AddAsync(new PeriodicEnergyStatementEntity
                {
                    Id = await _snowflakeIdService.GenerateId(cancellationToken),
                    GcsId = request.Request.GcsId,
                    MeasurementPointId = group.MeasurementPointId,
                    Month = request.Request.Month,
                    Year = request.Request.Year,
                    PeriodNumber = request.Request.PeriodNumber,
                    Bt = Math.Round((double)group.Bt , 2),
                    Cd = Math.Round((double)group.Cd, 2),
                    Td = Math.Round((double)group.Td, 2),
                    Sg = Math.Round((double)group.Sg, 2),
                    Vc = Math.Round((double)group.Vc, 2),
                    Pmax = Math.Round((double)group.Pmax, 2),
                    PreviousBt = Math.Round((double)group.PreviousBt, 2),
                    PreviousCd = Math.Round((double)group.PreviousCd, 2),
                    PreviousTd = Math.Round((double)group.PreviousTd, 2),
                    PreviousSg = Math.Round((double)group.PreviousSg, 2),
                    PreviousVc = Math.Round((double)group.PreviousVc, 2),
                    PreviousPmax = Math.Round((double)group.PreviousPmax, 2),
                    Multiplier = Math.Round((double)group.Multiplier, 2),
                    ConsumtionBt = Math.Round((double)group.ConsumtionBt, 2),
                    ConsumtionCd = Math.Round((double)group.ConsumtionCd, 2),
                    ConsumtionTd = Math.Round((double)group.ConsumtionTd, 2),
                    ConsumtionSg = Math.Round((double)group.ConsumtionSg, 2),
                    ConsumtionVc = Math.Round((double)group.ConsumtionVc, 2),
                    Day = group.Day,
                    PreviousDay = group.PreviousDay,
                    PreviousMonth = group.PreviousMonth,
                    PreviousYear = group.PreviousYear,
                    PreviousPeriodNumber = group.PreviousPeriodNumber,
                    Status = "",
                    CreatedTime = DateTimeOffset.Now,
                    SerialNumber = maxSerialNumberPeriodicEnergyStatementAsync.ToString("D8"),
                }, cancellationToken);
                var measurementPoint = await _measurementPointsReadWriteRepository.GetMeasurementPointByIdAsync(group.MeasurementPointId, cancellationToken);
                if (measurementPoint != null && measurementPoint.IsNewChange)
                {
                    measurementPoint.IsNewChange = false;
                    await _measurementPointsReadWriteRepository.UpdateAsync(measurementPoint, cancellationToken);
                }
            }

            gcs.IsNewChange = false;
            await _gcsReadWriteRepository.UpdateAsync(gcs, cancellationToken);
            await _gcsReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _gcsTemporaryReadWriteRepository.RemoveRangeAsync(gcsTemporary, cancellationToken);
            await _gcsTemporaryReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _gcsMonthlyDetailReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _periodicEnergyStatementReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _measurementPointsReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<GcsResponse>.Succeed(new GcsResponse()
            {
                Id = gcsCutoffCycleId
            });
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}