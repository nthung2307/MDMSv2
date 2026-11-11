using System.Globalization;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Application.Repositories.ReadOnly;
using Domain.Entities;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class EnergyConsumptionReportReadOnlyRepository :
    RestfulRepository<MeasurementPointDailyEnergyReportEntity, AppReadOnlyDbContext>,
    IEnergyConsumptionReportReadOnlyRepository
{
    private readonly DbSet<MeasurementPointDailyEnergyReportEntity> _dailySet;
    private readonly DbSet<MeasurementPointHalfHourEnergyReportEntity> _halfHourSet;
    private readonly DbSet<MeasurementPointsEntity> _pointSet;

    public EnergyConsumptionReportReadOnlyRepository(
        AppReadOnlyDbContext context,
        ISnowflakeIdService snowflakeIdService) : base(context, snowflakeIdService)
    {
        _dailySet = context.Set<MeasurementPointDailyEnergyReportEntity>();
        _halfHourSet = context.Set<MeasurementPointHalfHourEnergyReportEntity>();
        _pointSet = context.Set<MeasurementPointsEntity>();
    }

    public async Task<EnergyConsumptionReportResponse> GetEnergyConsumptionReportAsync(
        EnergyConsumptionReportRequest request,
        CancellationToken cancellationToken)
    {
        var point = await _pointSet.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.MeasurementPointId && p.CustomerId == request.CustomerId, cancellationToken);

        var multiplier = (point != null && point.MultiplierFactor > 0) ? (double)point.MultiplierFactor : 1.0;

        return request.CycleType?.ToUpperInvariant() switch
        {
            "MONTHLY" => await CalculateMonthlyConsumptionAsync(request, multiplier, cancellationToken),
            "YEARLY" => await CalculateYearlyConsumptionAsync(request, multiplier, cancellationToken),
            _ => await CalculateHalfHourConsumptionAsync(request, multiplier, cancellationToken)
        };
    }

    #region 1. Chu kỳ 30 phút
    private async Task<EnergyConsumptionReportResponse> CalculateHalfHourConsumptionAsync(
        EnergyConsumptionReportRequest request,
        double multiplier,
        CancellationToken cancellationToken)
    {
        var targetDate = request.Date?.ToOffset(TimeSpan.FromHours(7)).Date ?? DateTime.UtcNow.AddHours(7).Date;
        var startLocal = new DateTimeOffset(targetDate, TimeSpan.FromHours(7));
        var endLocal = startLocal.AddDays(1);

        // Lấy từ 23:00 hôm trước đến 00:30 ngày hôm sau để có đủ mốc tính delta cho chu kỳ đầu tiên
        var queryStart = startLocal.AddHours(-2).UtcDateTime;
        var queryEnd = endLocal.AddHours(1).UtcDateTime;

        var currentDayRecords = await _halfHourSet.AsNoTracking()
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= queryStart && m.CreatedTime <= queryEnd)
            .OrderBy(m => m.CreatedTime)
            .ToListAsync(cancellationToken);

        // Lấy dữ liệu ngày hôm trước nếu cần so sánh
        List<MeasurementPointHalfHourEnergyReportEntity> prevDayRecords = new();
        if (request.CompareWithPrevious)
        {
            var prevStart = queryStart.AddDays(-1);
            var prevEnd = queryEnd.AddDays(-1);
            prevDayRecords = await _halfHourSet.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId)
                .Where(m => m.CreatedTime >= prevStart && m.CreatedTime <= prevEnd)
                .OrderBy(m => m.CreatedTime)
                .ToListAsync(cancellationToken);
        }

        var response = new EnergyConsumptionReportResponse();
        var currentItems = CalculateHalfHourDeltas(currentDayRecords, targetDate, multiplier);
        var prevItems = request.CompareWithPrevious ? CalculateHalfHourDeltas(prevDayRecords, targetDate.AddDays(-1), multiplier) : new();

        for (int i = 0; i < currentItems.Count; i++)
        {
            var item = currentItems[i];
            if (request.CompareWithPrevious && i < prevItems.Count)
            {
                var prev = prevItems[i];
                item.PreviousSanLuongTacDungTongKenhGiao = prev.SanLuongTacDungTongKenhGiao;
                item.PreviousSanLuongTacDungBTKenhGiao = prev.SanLuongTacDungBTKenhGiao;
                item.PreviousSanLuongTacDungCDKenhGiao = prev.SanLuongTacDungCDKenhGiao;
                item.PreviousSanLuongTacDungTDKenhGiao = prev.SanLuongTacDungTDKenhGiao;
                item.PreviousSanLuongPhanKhangTongKenhGiao = prev.SanLuongPhanKhangTongKenhGiao;

                item.PreviousSanLuongTacDungTongKenhNhan = prev.SanLuongTacDungTongKenhNhan;
                item.PreviousSanLuongTacDungBTKenhNhan = prev.SanLuongTacDungBTKenhNhan;
                item.PreviousSanLuongTacDungCDKenhNhan = prev.SanLuongTacDungCDKenhNhan;
                item.PreviousSanLuongTacDungTDKenhNhan = prev.SanLuongTacDungTDKenhNhan;
                item.PreviousSanLuongPhanKhangTongKenhNhan = prev.SanLuongPhanKhangTongKenhNhan;

                var curGiao = item.SanLuongTacDungTongKenhGiao ?? 0;
                var prevGiao = prev.SanLuongTacDungTongKenhGiao ?? 0;
                item.DeltaKenhGiao = Math.Round(curGiao - prevGiao, 3);
                item.DeltaPercentageKenhGiao = prevGiao > 0
                    ? Math.Round(((curGiao - prevGiao) / prevGiao) * 100.0, 1)
                    : null;

                var curNhan = item.SanLuongTacDungTongKenhNhan ?? 0;
                var prevNhan = prev.SanLuongTacDungTongKenhNhan ?? 0;
                item.DeltaKenhNhan = Math.Round(curNhan - prevNhan, 3);
                item.DeltaPercentageKenhNhan = prevNhan > 0
                    ? Math.Round(((curNhan - prevNhan) / prevNhan) * 100.0, 1)
                    : null;
            }

            response.Items.Add(item);
            response.Chart.Labels.Add(item.TimeLabel);
            response.Chart.CurrentPeriodGiao.Add(item.SanLuongTacDungTongKenhGiao ?? 0.0);
            response.Chart.PreviousPeriodGiao.Add(item.PreviousSanLuongTacDungTongKenhGiao ?? 0.0);
            response.Chart.CurrentPeriodNhan.Add(item.SanLuongTacDungTongKenhNhan ?? 0.0);
            response.Chart.PreviousPeriodNhan.Add(item.PreviousSanLuongTacDungTongKenhNhan ?? 0.0);
        }

        response.Total = BuildTotalItem("Tổng cộng trong ngày", response.Items, multiplier);
        return response;
    }

    private List<EnergyConsumptionItemResponse> CalculateHalfHourDeltas(
        List<MeasurementPointHalfHourEnergyReportEntity> records,
        DateTime date,
        double multiplier)
    {
        var result = new List<EnergyConsumptionItemResponse>();
        var nowVn = DateTime.UtcNow.AddHours(7);

        // 48 chu kỳ 30 phút trong ngày: 00:00, 00:30, ..., 23:30
        for (int step = 0; step < 48; step++)
        {
            var periodTime = date.AddMinutes(step * 30);
            // Yêu cầu 5: Chỉ hiển thị đến mốc thời gian hiện tại của hệ thống, không tự ý thêm các bản ghi thời gian trong tương lai
            if (date.Date == nowVn.Date && periodTime > nowVn)
            {
                break;
            }
            if (date.Date > nowVn.Date)
            {
                break;
            }

            var timeLabel = periodTime.ToString("HH:mm");

            // Tìm bản ghi hiện tại và bản ghi chu kỳ trước đó
            var (currRecord, currApprox) = FindRecordOrClosest(records, periodTime);
            var (prevRecord, prevApprox) = FindRecordOrClosest(records, periodTime.AddMinutes(-30));

            bool isSuspected = currApprox || prevApprox;

            var item = new EnergyConsumptionItemResponse
            {
                TimeLabel = timeLabel,
                CreatedTime = new DateTimeOffset(periodTime, TimeSpan.FromHours(7)),
                MultiplierFactor = multiplier,
                IsSuspected = isSuspected
            };

            if (currRecord != null && prevRecord != null)
            {
                var (giaoTong, s1) = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, currRecord.SuspectedMask, prevRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, prevRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, multiplier, isSuspected);
                var (giaoBT, s2) = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, currRecord.SuspectedMask, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, prevRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, multiplier, isSuspected);
                var (giaoCD, s3) = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, currRecord.SuspectedMask, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, prevRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, multiplier, isSuspected);
                var (giaoTD, s4) = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, currRecord.SuspectedMask, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, prevRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, multiplier, isSuspected);
                var (giaoVC, s5) = CalculateDelta(currRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, currRecord.SuspectedMask, prevRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, prevRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, multiplier, isSuspected);

                var (nhanTong, s6) = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, currRecord.SuspectedMask, prevRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, prevRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, multiplier, isSuspected);
                var (nhanBT, s7) = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, currRecord.SuspectedMask, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, prevRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, multiplier, isSuspected);
                var (nhanCD, s8) = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, currRecord.SuspectedMask, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, prevRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, multiplier, isSuspected);
                var (nhanTD, s9) = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, currRecord.SuspectedMask, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, prevRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, multiplier, isSuspected);
                var (nhanVC, s10) = CalculateDelta(currRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, currRecord.SuspectedMask, prevRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, prevRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, multiplier, isSuspected);

                item.SanLuongTacDungTongKenhGiao = giaoTong;
                item.SanLuongTacDungBTKenhGiao = giaoBT;
                item.SanLuongTacDungCDKenhGiao = giaoCD;
                item.SanLuongTacDungTDKenhGiao = giaoTD;
                item.SanLuongPhanKhangTongKenhGiao = giaoVC;

                item.SanLuongTacDungTongKenhNhan = nhanTong;
                item.SanLuongTacDungBTKenhNhan = nhanBT;
                item.SanLuongTacDungCDKenhNhan = nhanCD;
                item.SanLuongTacDungTDKenhNhan = nhanTD;
                item.SanLuongPhanKhangTongKenhNhan = nhanVC;

                item.IsSuspected = isSuspected || s1 || s2 || s3 || s4 || s5 || s6 || s7 || s8 || s9 || s10;
            }

            result.Add(item);
        }

        return result;
    }
    #endregion

    #region 2. Chu kỳ Hàng Tháng (Từng ngày: 0h hôm nay - 0h hôm qua)
    private async Task<EnergyConsumptionReportResponse> CalculateMonthlyConsumptionAsync(
        EnergyConsumptionReportRequest request,
        double multiplier,
        CancellationToken cancellationToken)
    {
        var nowVn = DateTime.UtcNow.AddHours(7);
        var year = request.Year > 0 ? request.Year : nowVn.Year;
        var month = request.Month.HasValue && request.Month.Value >= 1 && request.Month.Value <= 12
            ? request.Month.Value
            : nowVn.Month;

        var startLocal = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.FromHours(7));
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var endLocal = startLocal.AddMonths(1).AddDays(1); // Lấy thêm ngày 1 của tháng sau

        var queryStart = startLocal.AddDays(-1).UtcDateTime;
        var queryEnd = endLocal.UtcDateTime;

        var currentRecords = await _dailySet.AsNoTracking()
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= queryStart && m.CreatedTime <= queryEnd)
            .OrderBy(m => m.CreatedTime)
            .ToListAsync(cancellationToken);

        // Lấy tháng trước để so sánh nếu bật CompareWithPrevious
        List<MeasurementPointDailyEnergyReportEntity> prevMonthRecords = new();
        if (request.CompareWithPrevious)
        {
            var prevMonthStart = startLocal.AddMonths(-1);
            var prevQueryStart = prevMonthStart.AddDays(-1).UtcDateTime;
            var prevQueryEnd = startLocal.AddDays(1).UtcDateTime;

            prevMonthRecords = await _dailySet.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId)
                .Where(m => m.CreatedTime >= prevQueryStart && m.CreatedTime <= prevQueryEnd)
                .OrderBy(m => m.CreatedTime)
                .ToListAsync(cancellationToken);
        }

        var response = new EnergyConsumptionReportResponse();
        var currentItems = CalculateDailyDeltasForMonth(currentRecords, year, month, multiplier);
        var prevItems = request.CompareWithPrevious
            ? CalculateDailyDeltasForMonth(prevMonthRecords, startLocal.AddMonths(-1).Year, startLocal.AddMonths(-1).Month, multiplier)
            : new();

        for (int day = 1; day <= daysInMonth; day++)
        {
            // Yêu cầu 5: Không sinh ngày trong tương lai nếu là tháng hiện tại
            if (year == nowVn.Year && month == nowVn.Month && day > nowVn.Day)
                break;
            if (new DateTime(year, month, 1) > new DateTime(nowVn.Year, nowVn.Month, 1))
                break;

            var item = currentItems.FirstOrDefault(x => x.CreatedTime?.Day == day) ?? new EnergyConsumptionItemResponse
            {
                TimeLabel = $"{day:D2}/{month:D2}",
                CreatedTime = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.FromHours(7)),
                MultiplierFactor = multiplier
            };

            if (request.CompareWithPrevious)
            {
                var prev = prevItems.FirstOrDefault(x => x.CreatedTime?.Day == day);
                if (prev != null)
                {
                    item.PreviousSanLuongTacDungTongKenhGiao = prev.SanLuongTacDungTongKenhGiao;
                    item.PreviousSanLuongTacDungBTKenhGiao = prev.SanLuongTacDungBTKenhGiao;
                    item.PreviousSanLuongTacDungCDKenhGiao = prev.SanLuongTacDungCDKenhGiao;
                    item.PreviousSanLuongTacDungTDKenhGiao = prev.SanLuongTacDungTDKenhGiao;
                    item.PreviousSanLuongPhanKhangTongKenhGiao = prev.SanLuongPhanKhangTongKenhGiao;

                    item.PreviousSanLuongTacDungTongKenhNhan = prev.SanLuongTacDungTongKenhNhan;
                    item.PreviousSanLuongTacDungBTKenhNhan = prev.SanLuongTacDungBTKenhNhan;
                    item.PreviousSanLuongTacDungCDKenhNhan = prev.SanLuongTacDungCDKenhNhan;
                    item.PreviousSanLuongTacDungTDKenhNhan = prev.SanLuongTacDungTDKenhNhan;
                    item.PreviousSanLuongPhanKhangTongKenhNhan = prev.SanLuongPhanKhangTongKenhNhan;

                    var curGiao = item.SanLuongTacDungTongKenhGiao ?? 0;
                    var prevGiao = prev.SanLuongTacDungTongKenhGiao ?? 0;
                    item.DeltaKenhGiao = Math.Round(curGiao - prevGiao, 3);
                    item.DeltaPercentageKenhGiao = prevGiao > 0
                        ? Math.Round(((curGiao - prevGiao) / prevGiao) * 100.0, 1)
                        : null;

                    var curNhan = item.SanLuongTacDungTongKenhNhan ?? 0;
                    var prevNhan = prev.SanLuongTacDungTongKenhNhan ?? 0;
                    item.DeltaKenhNhan = Math.Round(curNhan - prevNhan, 3);
                    item.DeltaPercentageKenhNhan = prevNhan > 0
                        ? Math.Round(((curNhan - prevNhan) / prevNhan) * 100.0, 1)
                        : null;
                }
            }

            response.Items.Add(item);
            response.Chart.Labels.Add(item.TimeLabel);
            response.Chart.CurrentPeriodGiao.Add(item.SanLuongTacDungTongKenhGiao ?? 0.0);
            response.Chart.PreviousPeriodGiao.Add(item.PreviousSanLuongTacDungTongKenhGiao ?? 0.0);
            response.Chart.CurrentPeriodNhan.Add(item.SanLuongTacDungTongKenhNhan ?? 0.0);
            response.Chart.PreviousPeriodNhan.Add(item.PreviousSanLuongTacDungTongKenhNhan ?? 0.0);
        }

        response.Total = BuildTotalItem("Tổng cộng tháng", response.Items, multiplier);
        return response;
    }

    private List<EnergyConsumptionItemResponse> CalculateDailyDeltasForMonth(
        List<MeasurementPointDailyEnergyReportEntity> records,
        int year,
        int month,
        double multiplier)
    {
        var result = new List<EnergyConsumptionItemResponse>();
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var nowVn = DateTime.UtcNow.AddHours(7);

        for (int day = 1; day <= daysInMonth; day++)
        {
            if (year == nowVn.Year && month == nowVn.Month && day > nowVn.Day)
                break;
            if (new DateTime(year, month, 1) > new DateTime(nowVn.Year, nowVn.Month, 1))
                break;

            var dayDate = new DateTime(year, month, day);
            var nextDayDate = dayDate.AddDays(1);

            // Chỉ số 0:00 ngày hôm sau - chỉ số 0:00 ngày hôm nay = Sản lượng ngày hôm nay
            var (nextDayRecord, nextApprox) = FindDailyRecordOrClosest(records, nextDayDate);
            var (currDayRecord, currApprox) = FindDailyRecordOrClosest(records, dayDate);

            bool isSuspected = nextApprox || currApprox;

            var item = new EnergyConsumptionItemResponse
            {
                TimeLabel = $"{day:D2}/{month:D2}",
                CreatedTime = new DateTimeOffset(dayDate, TimeSpan.FromHours(7)),
                MultiplierFactor = multiplier,
                IsSuspected = isSuspected
            };

            if (nextDayRecord != null && currDayRecord != null)
            {
                var (giaoTong, s1) = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, nextDayRecord.SuspectedMask, currDayRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, currDayRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, multiplier, isSuspected);
                var (giaoBT, s2) = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, nextDayRecord.SuspectedMask, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, currDayRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, multiplier, isSuspected);
                var (giaoCD, s3) = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, nextDayRecord.SuspectedMask, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, currDayRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, multiplier, isSuspected);
                var (giaoTD, s4) = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, nextDayRecord.SuspectedMask, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, currDayRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, multiplier, isSuspected);
                var (giaoVC, s5) = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, nextDayRecord.SuspectedMask, currDayRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, currDayRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, multiplier, isSuspected);

                var (nhanTong, s6) = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, nextDayRecord.SuspectedMask, currDayRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, currDayRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, multiplier, isSuspected);
                var (nhanBT, s7) = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, nextDayRecord.SuspectedMask, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, currDayRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, multiplier, isSuspected);
                var (nhanCD, s8) = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, nextDayRecord.SuspectedMask, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, currDayRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, multiplier, isSuspected);
                var (nhanTD, s9) = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, nextDayRecord.SuspectedMask, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, currDayRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, multiplier, isSuspected);
                var (nhanVC, s10) = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, nextDayRecord.SuspectedMask, currDayRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, currDayRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, multiplier, isSuspected);

                item.SanLuongTacDungTongKenhGiao = giaoTong;
                item.SanLuongTacDungBTKenhGiao = giaoBT;
                item.SanLuongTacDungCDKenhGiao = giaoCD;
                item.SanLuongTacDungTDKenhGiao = giaoTD;
                item.SanLuongPhanKhangTongKenhGiao = giaoVC;

                item.SanLuongTacDungTongKenhNhan = nhanTong;
                item.SanLuongTacDungBTKenhNhan = nhanBT;
                item.SanLuongTacDungCDKenhNhan = nhanCD;
                item.SanLuongTacDungTDKenhNhan = nhanTD;
                item.SanLuongPhanKhangTongKenhNhan = nhanVC;

                item.IsSuspected = isSuspected || s1 || s2 || s3 || s4 || s5 || s6 || s7 || s8 || s9 || s10;
            }

            result.Add(item);
        }

        return result;
    }
    #endregion

    #region 3. Chu kỳ Hàng Năm (12 tháng: Đầu tháng sau - Đầu tháng này)
    private async Task<EnergyConsumptionReportResponse> CalculateYearlyConsumptionAsync(
        EnergyConsumptionReportRequest request,
        double multiplier,
        CancellationToken cancellationToken)
    {
        var nowVn = DateTime.UtcNow.AddHours(7);
        var year = request.Year > 0 ? request.Year : nowVn.Year;
        var startLocal = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.FromHours(7));
        var endLocal = new DateTimeOffset(year + 1, 1, 2, 23, 59, 59, TimeSpan.FromHours(7));

        var currentRecords = await _dailySet.AsNoTracking()
            .Where(m => m.MeasurementPointId == request.MeasurementPointId)
            .Where(m => m.CreatedTime >= startLocal.UtcDateTime && m.CreatedTime <= endLocal.UtcDateTime)
            .OrderBy(m => m.CreatedTime)
            .ToListAsync(cancellationToken);

        // Năm trước nếu CompareWithPrevious
        List<MeasurementPointDailyEnergyReportEntity> prevYearRecords = new();
        if (request.CompareWithPrevious)
        {
            var prevStart = new DateTimeOffset(year - 1, 1, 1, 0, 0, 0, TimeSpan.FromHours(7));
            var prevEnd = new DateTimeOffset(year, 1, 2, 23, 59, 59, TimeSpan.FromHours(7));

            prevYearRecords = await _dailySet.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId)
                .Where(m => m.CreatedTime >= prevStart.UtcDateTime && m.CreatedTime <= prevEnd.UtcDateTime)
                .OrderBy(m => m.CreatedTime)
                .ToListAsync(cancellationToken);
        }

        var response = new EnergyConsumptionReportResponse();
        var currentItems = CalculateYearlyMonthlyDeltas(currentRecords, year, multiplier);
        var prevItems = request.CompareWithPrevious ? CalculateYearlyMonthlyDeltas(prevYearRecords, year - 1, multiplier) : new();

        for (int m = 1; m <= 12; m++)
        {
            // Yêu cầu 5: Không sinh tháng trong tương lai nếu là năm hiện tại
            if (year == nowVn.Year && m > nowVn.Month)
                break;
            if (year > nowVn.Year)
                break;

            var item = currentItems.FirstOrDefault(x => x.CreatedTime?.Month == m) ?? new EnergyConsumptionItemResponse
            {
                TimeLabel = $"Tháng {m:D2}/{year}",
                CreatedTime = new DateTimeOffset(year, m, 1, 0, 0, 0, TimeSpan.FromHours(7)),
                MultiplierFactor = multiplier
            };

            if (request.CompareWithPrevious)
            {
                var prev = prevItems.FirstOrDefault(x => x.CreatedTime?.Month == m);
                if (prev != null)
                {
                    item.PreviousSanLuongTacDungTongKenhGiao = prev.SanLuongTacDungTongKenhGiao;
                    item.PreviousSanLuongTacDungBTKenhGiao = prev.SanLuongTacDungBTKenhGiao;
                    item.PreviousSanLuongTacDungCDKenhGiao = prev.SanLuongTacDungCDKenhGiao;
                    item.PreviousSanLuongTacDungTDKenhGiao = prev.SanLuongTacDungTDKenhGiao;
                    item.PreviousSanLuongPhanKhangTongKenhGiao = prev.SanLuongPhanKhangTongKenhGiao;

                    item.PreviousSanLuongTacDungTongKenhNhan = prev.SanLuongTacDungTongKenhNhan;
                    item.PreviousSanLuongTacDungBTKenhNhan = prev.SanLuongTacDungBTKenhNhan;
                    item.PreviousSanLuongTacDungCDKenhNhan = prev.SanLuongTacDungCDKenhNhan;
                    item.PreviousSanLuongTacDungTDKenhNhan = prev.SanLuongTacDungTDKenhNhan;
                    item.PreviousSanLuongPhanKhangTongKenhNhan = prev.SanLuongPhanKhangTongKenhNhan;

                    var curGiao = item.SanLuongTacDungTongKenhGiao ?? 0;
                    var prevGiao = prev.SanLuongTacDungTongKenhGiao ?? 0;
                    item.DeltaKenhGiao = Math.Round(curGiao - prevGiao, 3);
                    item.DeltaPercentageKenhGiao = prevGiao > 0
                        ? Math.Round(((curGiao - prevGiao) / prevGiao) * 100.0, 1)
                        : null;

                    var curNhan = item.SanLuongTacDungTongKenhNhan ?? 0;
                    var prevNhan = prev.SanLuongTacDungTongKenhNhan ?? 0;
                    item.DeltaKenhNhan = Math.Round(curNhan - prevNhan, 3);
                    item.DeltaPercentageKenhNhan = prevNhan > 0
                        ? Math.Round(((curNhan - prevNhan) / prevNhan) * 100.0, 1)
                        : null;
                }
            }

            response.Items.Add(item);
            response.Chart.Labels.Add(item.TimeLabel);
            response.Chart.CurrentPeriodGiao.Add(item.SanLuongTacDungTongKenhGiao ?? 0.0);
            response.Chart.PreviousPeriodGiao.Add(item.PreviousSanLuongTacDungTongKenhGiao ?? 0.0);
            response.Chart.CurrentPeriodNhan.Add(item.SanLuongTacDungTongKenhNhan ?? 0.0);
            response.Chart.PreviousPeriodNhan.Add(item.PreviousSanLuongTacDungTongKenhNhan ?? 0.0);
        }

        response.Total = BuildTotalItem("Tổng cộng cả năm", response.Items, multiplier);
        return response;
    }

    private List<EnergyConsumptionItemResponse> CalculateYearlyMonthlyDeltas(
        List<MeasurementPointDailyEnergyReportEntity> records,
        int year,
        double multiplier)
    {
        var result = new List<EnergyConsumptionItemResponse>();
        var nowVn = DateTime.UtcNow.AddHours(7);

        for (int m = 1; m <= 12; m++)
        {
            if (year == nowVn.Year && m > nowVn.Month)
                break;
            if (year > nowVn.Year)
                break;

            var startOfMonth = new DateTime(year, m, 1);
            var endOfMonth = startOfMonth.AddMonths(1); // 0:00 ngày đầu tháng sau

            var (startRecord, startApprox) = FindDailyRecordOrClosest(records, startOfMonth);
            var (endRecord, endApprox) = FindDailyRecordOrClosest(records, endOfMonth);

            bool isSuspected = startApprox || endApprox;

            var item = new EnergyConsumptionItemResponse
            {
                TimeLabel = $"Tháng {m:D2}/{year}",
                CreatedTime = new DateTimeOffset(startOfMonth, TimeSpan.FromHours(7)),
                MultiplierFactor = multiplier,
                IsSuspected = isSuspected
            };

            if (startRecord != null && endRecord != null)
            {
                var (giaoTong, s1) = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, endRecord.SuspectedMask, startRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, startRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, multiplier, isSuspected);
                var (giaoBT, s2) = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, endRecord.SuspectedMask, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, startRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, multiplier, isSuspected);
                var (giaoCD, s3) = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, endRecord.SuspectedMask, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, startRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, multiplier, isSuspected);
                var (giaoTD, s4) = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, endRecord.SuspectedMask, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, startRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, multiplier, isSuspected);
                var (giaoVC, s5) = CalculateDelta(endRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, endRecord.SuspectedMask, startRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, startRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, multiplier, isSuspected);

                var (nhanTong, s6) = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, endRecord.SuspectedMask, startRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, startRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, multiplier, isSuspected);
                var (nhanBT, s7) = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, endRecord.SuspectedMask, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, startRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, multiplier, isSuspected);
                var (nhanCD, s8) = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, endRecord.SuspectedMask, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, startRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, multiplier, isSuspected);
                var (nhanTD, s9) = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, endRecord.SuspectedMask, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, startRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, multiplier, isSuspected);
                var (nhanVC, s10) = CalculateDelta(endRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, endRecord.SuspectedMask, startRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, startRecord.SuspectedMask, EnergySuspectedFlags.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, multiplier, isSuspected);

                item.SanLuongTacDungTongKenhGiao = giaoTong;
                item.SanLuongTacDungBTKenhGiao = giaoBT;
                item.SanLuongTacDungCDKenhGiao = giaoCD;
                item.SanLuongTacDungTDKenhGiao = giaoTD;
                item.SanLuongPhanKhangTongKenhGiao = giaoVC;

                item.SanLuongTacDungTongKenhNhan = nhanTong;
                item.SanLuongTacDungBTKenhNhan = nhanBT;
                item.SanLuongTacDungCDKenhNhan = nhanCD;
                item.SanLuongTacDungTDKenhNhan = nhanTD;
                item.SanLuongPhanKhangTongKenhNhan = nhanVC;

                item.IsSuspected = isSuspected || s1 || s2 || s3 || s4 || s5 || s6 || s7 || s8 || s9 || s10;
            }

            result.Add(item);
        }

        return result;
    }
    #endregion

    #region Helper Methods
    private static (double? Delta, bool IsSuspected) CalculateDelta(
        decimal? currentDec,
        int currMask,
        decimal? prevDec,
        int prevMask,
        EnergySuspectedFlags flag,
        double multiplier,
        bool isSuspectedSource = false)
    {
        if (!currentDec.HasValue || !prevDec.HasValue)
            return (null, false);

        var hasSuspectedFlag = MeteringValueHelper.HasSuspectedFlag(currMask, flag) ||
                               MeteringValueHelper.HasSuspectedFlag(prevMask, flag);
        var isSuspected = isSuspectedSource || hasSuspectedFlag;

        var currentVal = (double)currentDec.Value;
        var prevVal = (double)prevDec.Value;

        double delta;
        if (currentVal < prevVal)
        {
            delta = currentVal;
            isSuspected = true;
        }
        else
        {
            delta = currentVal - prevVal;
            if (delta == 0 && (hasSuspectedFlag || isSuspectedSource))
            {
                isSuspected = true;
            }
        }

        return (Math.Round(delta * multiplier, 3), isSuspected);
    }

    private static (MeasurementPointHalfHourEnergyReportEntity? Record, bool IsApproximate) FindRecordOrClosest(
        List<MeasurementPointHalfHourEnergyReportEntity> records,
        DateTime targetTime)
    {
        if (records == null || records.Count == 0)
            return (null, false);

        // 1. Tìm bản ghi chuẩn (sai lệch <= 15 phút quanh mốc targetTime)
        var exact = records.FirstOrDefault(r =>
        {
            if (!r.CreatedTime.HasValue) return false;
            var local = r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime;
            return Math.Abs((local - targetTime).TotalMinutes) <= 15;
        });

        if (exact != null)
            return (exact, false);

        // 2. Tìm bản ghi Valid gần nhất trong quá khứ (CreatedTime <= targetTime) khi mất kết nối
        var closestPast = records
            .Where(r => r.CreatedTime.HasValue && r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime <= targetTime)
            .OrderByDescending(r => r.CreatedTime)
            .FirstOrDefault();

        if (closestPast != null)
            return (closestPast, true);

        // 3. Nếu không có bản ghi quá khứ, tìm bản ghi tương lai gần nhất trong vòng 30 phút
        var closestFuture = records
            .Where(r => r.CreatedTime.HasValue && (r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime - targetTime).TotalMinutes <= 30)
            .OrderBy(r => r.CreatedTime)
            .FirstOrDefault();

        if (closestFuture != null)
            return (closestFuture, true);

        return (null, false);
    }

    private static (MeasurementPointDailyEnergyReportEntity? Record, bool IsApproximate) FindDailyRecordOrClosest(
        List<MeasurementPointDailyEnergyReportEntity> records,
        DateTime targetDate)
    {
        if (records == null || records.Count == 0)
            return (null, false);

        var exact = records.FirstOrDefault(r =>
        {
            if (!r.CreatedTime.HasValue) return false;
            var local = r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime;
            return local.Year == targetDate.Year && local.Month == targetDate.Month && local.Day == targetDate.Day;
        });

        if (exact != null)
            return (exact, false);

        // Bản ghi valid gần nhất trong quá khứ <= targetDate
        var closestPast = records
            .Where(r => r.CreatedTime.HasValue && r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime.Date <= targetDate.Date)
            .OrderByDescending(r => r.CreatedTime)
            .FirstOrDefault();

        if (closestPast != null)
            return (closestPast, true);

        return (null, false);
    }

    private static EnergyConsumptionItemResponse BuildTotalItem(
        string label,
        List<EnergyConsumptionItemResponse> items,
        double multiplier)
    {
        var total = new EnergyConsumptionItemResponse
        {
            TimeLabel = label,
            MultiplierFactor = multiplier,
            SanLuongTacDungTongKenhGiao = Math.Round(items.Where(x => x.SanLuongTacDungTongKenhGiao.HasValue).Sum(x => x.SanLuongTacDungTongKenhGiao!.Value), 3),
            SanLuongTacDungBTKenhGiao = Math.Round(items.Where(x => x.SanLuongTacDungBTKenhGiao.HasValue).Sum(x => x.SanLuongTacDungBTKenhGiao!.Value), 3),
            SanLuongTacDungCDKenhGiao = Math.Round(items.Where(x => x.SanLuongTacDungCDKenhGiao.HasValue).Sum(x => x.SanLuongTacDungCDKenhGiao!.Value), 3),
            SanLuongTacDungTDKenhGiao = Math.Round(items.Where(x => x.SanLuongTacDungTDKenhGiao.HasValue).Sum(x => x.SanLuongTacDungTDKenhGiao!.Value), 3),
            SanLuongPhanKhangTongKenhGiao = Math.Round(items.Where(x => x.SanLuongPhanKhangTongKenhGiao.HasValue).Sum(x => x.SanLuongPhanKhangTongKenhGiao!.Value), 3),

            SanLuongTacDungTongKenhNhan = Math.Round(items.Where(x => x.SanLuongTacDungTongKenhNhan.HasValue).Sum(x => x.SanLuongTacDungTongKenhNhan!.Value), 3),
            SanLuongTacDungBTKenhNhan = Math.Round(items.Where(x => x.SanLuongTacDungBTKenhNhan.HasValue).Sum(x => x.SanLuongTacDungBTKenhNhan!.Value), 3),
            SanLuongTacDungCDKenhNhan = Math.Round(items.Where(x => x.SanLuongTacDungCDKenhNhan.HasValue).Sum(x => x.SanLuongTacDungCDKenhNhan!.Value), 3),
            SanLuongTacDungTDKenhNhan = Math.Round(items.Where(x => x.SanLuongTacDungTDKenhNhan.HasValue).Sum(x => x.SanLuongTacDungTDKenhNhan!.Value), 3),
            SanLuongPhanKhangTongKenhNhan = Math.Round(items.Where(x => x.SanLuongPhanKhangTongKenhNhan.HasValue).Sum(x => x.SanLuongPhanKhangTongKenhNhan!.Value), 3),

            PreviousSanLuongTacDungTongKenhGiao = Math.Round(items.Where(x => x.PreviousSanLuongTacDungTongKenhGiao.HasValue).Sum(x => x.PreviousSanLuongTacDungTongKenhGiao!.Value), 3),
            PreviousSanLuongTacDungBTKenhGiao = Math.Round(items.Where(x => x.PreviousSanLuongTacDungBTKenhGiao.HasValue).Sum(x => x.PreviousSanLuongTacDungBTKenhGiao!.Value), 3),
            PreviousSanLuongTacDungCDKenhGiao = Math.Round(items.Where(x => x.PreviousSanLuongTacDungCDKenhGiao.HasValue).Sum(x => x.PreviousSanLuongTacDungCDKenhGiao!.Value), 3),
            PreviousSanLuongTacDungTDKenhGiao = Math.Round(items.Where(x => x.PreviousSanLuongTacDungTDKenhGiao.HasValue).Sum(x => x.PreviousSanLuongTacDungTDKenhGiao!.Value), 3),
            PreviousSanLuongPhanKhangTongKenhGiao = Math.Round(items.Where(x => x.PreviousSanLuongPhanKhangTongKenhGiao.HasValue).Sum(x => x.PreviousSanLuongPhanKhangTongKenhGiao!.Value), 3),

            PreviousSanLuongTacDungTongKenhNhan = Math.Round(items.Where(x => x.PreviousSanLuongTacDungTongKenhNhan.HasValue).Sum(x => x.PreviousSanLuongTacDungTongKenhNhan!.Value), 3),
            PreviousSanLuongTacDungBTKenhNhan = Math.Round(items.Where(x => x.PreviousSanLuongTacDungBTKenhNhan.HasValue).Sum(x => x.PreviousSanLuongTacDungBTKenhNhan!.Value), 3),
            PreviousSanLuongTacDungCDKenhNhan = Math.Round(items.Where(x => x.PreviousSanLuongTacDungCDKenhNhan.HasValue).Sum(x => x.PreviousSanLuongTacDungCDKenhNhan!.Value), 3),
            PreviousSanLuongTacDungTDKenhNhan = Math.Round(items.Where(x => x.PreviousSanLuongTacDungTDKenhNhan.HasValue).Sum(x => x.PreviousSanLuongTacDungTDKenhNhan!.Value), 3),
            PreviousSanLuongPhanKhangTongKenhNhan = Math.Round(items.Where(x => x.PreviousSanLuongPhanKhangTongKenhNhan.HasValue).Sum(x => x.PreviousSanLuongPhanKhangTongKenhNhan!.Value), 3)
        };

        var curGiao = total.SanLuongTacDungTongKenhGiao ?? 0;
        var prevGiao = total.PreviousSanLuongTacDungTongKenhGiao ?? 0;
        total.DeltaKenhGiao = Math.Round(curGiao - prevGiao, 3);
        total.DeltaPercentageKenhGiao = prevGiao > 0
            ? Math.Round(((curGiao - prevGiao) / prevGiao) * 100.0, 1)
            : null;

        var curNhan = total.SanLuongTacDungTongKenhNhan ?? 0;
        var prevNhan = total.PreviousSanLuongTacDungTongKenhNhan ?? 0;
        total.DeltaKenhNhan = Math.Round(curNhan - prevNhan, 3);
        total.DeltaPercentageKenhNhan = prevNhan > 0
            ? Math.Round(((curNhan - prevNhan) / prevNhan) * 100.0, 1)
            : null;

        return total;
    }
    #endregion
}
