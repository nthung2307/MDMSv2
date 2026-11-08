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
                        Error = "Không thể chốt sổ: Còn chỉ số chưa cập nhật hoặc số liệu nghi ngờ chưa được kiểm tra / hiệu chỉnh. Người dùng chỉ được xác nhận chốt sổ sau khi không còn số liệu nghi ngờ (hoặc sau khi đã nhập liệu thủ công / Import file Excel chuyển màu sang Normal)."
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

            try
            {
                await _gcsTemporaryReadWriteRepository.EnsureColumnsExistAsync(cancellationToken);
            }
            catch { }

            var gcsTemporary = await _gcsTemporaryReadWriteRepository.GetListGcsTemporaryAsync(new GetDataLatchRequest()
            {
                GcsId = request.Request.GcsId,
                Month = request.Request.Month,
                Year = request.Request.Year,
                PeriodNumber = request.Request.PeriodNumber,
                IsRefresh = false
            }, cancellationToken);

            // Đảm bảo toàn bộ Sản lượng và hệ số CosPhi (PF) trung bình được tính toán chuẩn xác trước khi chốt sổ
            foreach (var ptGroup in gcsTemporary.GroupBy(x => x.MeasurementPointId))
            {
                Infrastructure.Repositories.ReadWrite.GcsTemporaryReadWriteRepository.RecalculatePointConsumptionAndCosPhi(ptGroup);
            }

            var groupedTemporary = gcsTemporary.GroupBy(x => x.MeasurementPointId)
                .Select(group =>
                {
                    var btRec = group.FirstOrDefault(x => x.MeterReadingSet == "BT");
                    var cdRec = group.FirstOrDefault(x => x.MeterReadingSet == "CD");
                    var tdRec = group.FirstOrDefault(x => x.MeterReadingSet == "TD");
                    var sgRec = group.FirstOrDefault(x => x.MeterReadingSet == "SG");
                    var vcRec = group.FirstOrDefault(x => x.MeterReadingSet == "VC");
                    var pmaxRec = group.FirstOrDefault(x => x.MeterReadingSet == "Pmax");

                    var btCurr = btRec?.NewCurrentReading ?? btRec?.CurrentReading ?? 0;
                    var cdCurr = cdRec?.NewCurrentReading ?? cdRec?.CurrentReading ?? 0;
                    var tdCurr = tdRec?.NewCurrentReading ?? tdRec?.CurrentReading ?? 0;
                    var sgCurr = sgRec?.NewCurrentReading ?? sgRec?.CurrentReading ?? 0;
                    var vcCurr = vcRec?.NewCurrentReading ?? vcRec?.CurrentReading ?? 0;
                    var pmaxCurr = pmaxRec?.NewCurrentReading ?? pmaxRec?.CurrentReading ?? 0;

                    var btPrev = btRec?.NewPreviousReading ?? btRec?.PreviousReading ?? 0;
                    var cdPrev = cdRec?.NewPreviousReading ?? cdRec?.PreviousReading ?? 0;
                    var tdPrev = tdRec?.NewPreviousReading ?? tdRec?.PreviousReading ?? 0;
                    var sgPrev = sgRec?.NewPreviousReading ?? sgRec?.PreviousReading ?? 0;
                    var vcPrev = vcRec?.NewPreviousReading ?? vcRec?.PreviousReading ?? 0;
                    var pmaxPrev = pmaxRec?.NewPreviousReading ?? pmaxRec?.PreviousReading ?? 0;

                    var mult = btRec?.MultiplierFactor ?? sgRec?.MultiplierFactor ?? group.FirstOrDefault()?.MultiplierFactor ?? 1;

                    var consBt = btRec?.Consumption ?? ((btCurr - btPrev) * mult);
                    var consCd = cdRec?.Consumption ?? ((cdCurr - cdPrev) * mult);
                    var consTd = tdRec?.Consumption ?? ((tdCurr - tdPrev) * mult);
                    var consSg = sgRec?.Consumption ?? ((sgCurr - sgPrev) * mult);
                    var consVc = vcRec?.Consumption ?? ((vcCurr - vcPrev) * mult);

                    double? effectiveAp = consSg > 0 ? consSg : ((consBt + consCd + consTd) > 0 ? (consBt + consCd + consTd) : consSg);
                    var cosPhi = group.FirstOrDefault(x => x.CosPhi.HasValue)?.CosPhi
                                 ?? Infrastructure.Repositories.ReadWrite.GcsTemporaryReadWriteRepository.CalculateAverageCosPhi(effectiveAp, consVc);

                    var billingTimestamp = sgRec?.ReadingTime ?? btRec?.ReadingTime ?? group.FirstOrDefault()?.ReadingTime;
                    var pmaxTime = pmaxRec?.PmaxTime ?? group.FirstOrDefault(x => x.PmaxTime.HasValue)?.PmaxTime;

                    return new
                    {
                        MeasurementPointId = group.Key,
                        Bt = btCurr,
                        Cd = cdCurr,
                        Td = tdCurr,
                        Sg = sgCurr,
                        Vc = vcCurr,
                        Pmax = pmaxCurr,
                        PreviousBt = btPrev,
                        PreviousCd = cdPrev,
                        PreviousTd = tdPrev,
                        PreviousSg = sgPrev,
                        PreviousVc = vcPrev,
                        PreviousPmax = pmaxPrev,
                        Multiplier = mult,
                        ConsumtionBt = consBt,
                        ConsumtionCd = consCd,
                        ConsumtionTd = consTd,
                        ConsumtionSg = consSg,
                        ConsumtionVc = consVc,
                        CosPhi = cosPhi,
                        BillingTimestamp = billingTimestamp,
                        PmaxTime = pmaxTime,
                        PreviousDay = group.FirstOrDefault()?.PreviousDay,
                        PreviousMonth = group.FirstOrDefault()?.PreviousMonth,
                        PreviousYear = group.FirstOrDefault()?.PreviousYear,
                        PreviousPeriodNumber = group.FirstOrDefault()?.PreviousPeriodNumber,
                        Day = group.FirstOrDefault()?.Day,
                        Month = group.FirstOrDefault()?.Month,
                        Year = group.FirstOrDefault()?.Year,
                        PeriodNumber = group.FirstOrDefault()?.PeriodNumber
                    };
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

            var orderedTemporary = gcsTemporary.OrderByDescending(x => x.Id).ToList();
            var monthlyDetailsToInsert = new List<GcsMonthlyDetailEntity>();

            // Duyệt ngược từ cuối danh sách lên đầu để sinh Snowflake ID tăng dần:
            // Nhờ đó orderedTemporary[0] (dòng đầu tiên trên màn hình Ghi chỉ số) sẽ có Snowflake ID lớn nhất,
            // khi truy vấn GcsMonthlyDetail bằng Id DESC sẽ hiển thị orderedTemporary[0] đầu tiên,
            // bảo toàn 100% thứ tự từng điểm đo và từng bộ chỉ số như trước khi chốt!
            for (int i = orderedTemporary.Count - 1; i >= 0; i--)
            {
                var item = orderedTemporary[i];
                gcsCutOffCycle.Day = item.Day;
                monthlyDetailsToInsert.Add(new GcsMonthlyDetailEntity
                {
                    Id = await _snowflakeIdService.GenerateId(cancellationToken),
                    GcsCutoffCycleId = gcsCutoffCycleId,
                    MeasurementPointId = item.MeasurementPointId,
                    MeterReadingSet = item.MeterReadingSet,
                    PreviousReading = item.NewPreviousReading ?? item.PreviousReading,
                    CurrentReading = item.NewCurrentReading ?? item.CurrentReading,
                    MultiplierFactor = item.MultiplierFactor,
                    Consumption = item.Consumption,
                    CosPhi = item.CosPhi,
                    ReadingTime = item.ReadingTime,
                    PmaxTime = item.PmaxTime,
                    IsChange = item.IsChange,
                    CreatedTime = DateTimeOffset.Now,
                    CreatedBy = currentId,
                    CreatedName = userName
                });
            }

            monthlyDetailsToInsert.Reverse();
            foreach (var detail in monthlyDetailsToInsert)
            {
                await _gcsMonthlyDetailReadWriteRepository.AddAsync(detail, cancellationToken);
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
                    Bt = Math.Round((double)group.Bt, 2),
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
                    CosPhi = group.CosPhi.HasValue ? Math.Round(group.CosPhi.Value, 2, MidpointRounding.AwayFromZero) : null,
                    BillingTimestamp = group.BillingTimestamp,
                    PmaxTime = group.PmaxTime,
                    Day = group.Day ?? gcsCutOffCycle.Day,
                    PreviousDay = group.PreviousDay ?? gcsCutOffCycle.PreviousDay ?? gcsCutoffCycleOld?.Day ?? (request.Request.PeriodNumber == 1 ? 1 : (int?)null),
                    PreviousMonth = group.PreviousMonth ?? gcsCutOffCycle.PreviousMonth ?? gcsCutoffCycleOld?.Month ?? (request.Request.PeriodNumber == 1 ? request.Request.Month : (int?)null),
                    PreviousYear = group.PreviousYear ?? gcsCutOffCycle.PreviousYear ?? gcsCutoffCycleOld?.Year ?? (request.Request.PeriodNumber == 1 ? request.Request.Year : (int?)null),
                    PreviousPeriodNumber = group.PreviousPeriodNumber ?? gcsCutOffCycle.PreviousPeriodNumber ?? gcsCutoffCycleOld?.PeriodNumber,
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