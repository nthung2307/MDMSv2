using System.Globalization;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Application.Repositories.ReadOnly;
using Domain.Entities;
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

        var multiplier = (point != null && point.MultiplierFactor > 0) ? point.MultiplierFactor : 1.0;

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
                item.PreviousSanLuongTacDungTongKenhNhan = prev.SanLuongTacDungTongKenhNhan;

                item.DeltaKenhGiao = Math.Round(item.SanLuongTacDungTongKenhGiao - prev.SanLuongTacDungTongKenhGiao, 3);
                item.DeltaPercentageKenhGiao = prev.SanLuongTacDungTongKenhGiao > 0
                    ? Math.Round(((item.SanLuongTacDungTongKenhGiao - prev.SanLuongTacDungTongKenhGiao) / prev.SanLuongTacDungTongKenhGiao) * 100.0, 1)
                    : null;

                item.DeltaKenhNhan = Math.Round(item.SanLuongTacDungTongKenhNhan - prev.SanLuongTacDungTongKenhNhan, 3);
                item.DeltaPercentageKenhNhan = prev.SanLuongTacDungTongKenhNhan > 0
                    ? Math.Round(((item.SanLuongTacDungTongKenhNhan - prev.SanLuongTacDungTongKenhNhan) / prev.SanLuongTacDungTongKenhNhan) * 100.0, 1)
                    : null;
            }

            response.Items.Add(item);
            response.Chart.Labels.Add(item.TimeLabel);
            response.Chart.CurrentPeriodGiao.Add(item.SanLuongTacDungTongKenhGiao);
            response.Chart.PreviousPeriodGiao.Add(item.PreviousSanLuongTacDungTongKenhGiao ?? 0.0);
            response.Chart.CurrentPeriodNhan.Add(item.SanLuongTacDungTongKenhNhan);
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
        // 48 chu kỳ 30 phút trong ngày: 00:00, 00:30, ..., 23:30
        for (int step = 0; step < 48; step++)
        {
            var periodTime = date.AddMinutes(step * 30);
            var timeLabel = periodTime.ToString("HH:mm");

            // Tìm bản ghi hiện tại và bản ghi chu kỳ trước đó
            var currRecord = FindClosestRecord(records, periodTime);
            var prevRecord = FindClosestRecord(records, periodTime.AddMinutes(-30));

            var item = new EnergyConsumptionItemResponse
            {
                TimeLabel = timeLabel,
                CreatedTime = new DateTimeOffset(periodTime, TimeSpan.FromHours(7)),
                MultiplierFactor = multiplier
            };

            if (currRecord != null && prevRecord != null)
            {
                item.SanLuongTacDungTongKenhGiao = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, prevRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, multiplier);
                item.SanLuongTacDungBTKenhGiao = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, multiplier);
                item.SanLuongTacDungCDKenhGiao = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, multiplier);
                item.SanLuongTacDungTDKenhGiao = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, multiplier);
                item.SanLuongPhanKhangTongKenhGiao = CalculateDelta(currRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, prevRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, multiplier);

                item.SanLuongTacDungTongKenhNhan = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, prevRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, multiplier);
                item.SanLuongTacDungBTKenhNhan = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, multiplier);
                item.SanLuongTacDungCDKenhNhan = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, multiplier);
                item.SanLuongTacDungTDKenhNhan = CalculateDelta(currRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, prevRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, multiplier);
                item.SanLuongPhanKhangTongKenhNhan = CalculateDelta(currRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, prevRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, multiplier);
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
        var year = request.Year > 0 ? request.Year : DateTime.UtcNow.AddHours(7).Year;
        var month = request.Month.HasValue && request.Month.Value >= 1 && request.Month.Value <= 12
            ? request.Month.Value
            : DateTime.UtcNow.AddHours(7).Month;

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
                    item.PreviousSanLuongTacDungTongKenhNhan = prev.SanLuongTacDungTongKenhNhan;

                    item.DeltaKenhGiao = Math.Round(item.SanLuongTacDungTongKenhGiao - prev.SanLuongTacDungTongKenhGiao, 3);
                    item.DeltaPercentageKenhGiao = prev.SanLuongTacDungTongKenhGiao > 0
                        ? Math.Round(((item.SanLuongTacDungTongKenhGiao - prev.SanLuongTacDungTongKenhGiao) / prev.SanLuongTacDungTongKenhGiao) * 100.0, 1)
                        : null;

                    item.DeltaKenhNhan = Math.Round(item.SanLuongTacDungTongKenhNhan - prev.SanLuongTacDungTongKenhNhan, 3);
                    item.DeltaPercentageKenhNhan = prev.SanLuongTacDungTongKenhNhan > 0
                        ? Math.Round(((item.SanLuongTacDungTongKenhNhan - prev.SanLuongTacDungTongKenhNhan) / prev.SanLuongTacDungTongKenhNhan) * 100.0, 1)
                        : null;
                }
            }

            response.Items.Add(item);
            response.Chart.Labels.Add(item.TimeLabel);
            response.Chart.CurrentPeriodGiao.Add(item.SanLuongTacDungTongKenhGiao);
            response.Chart.PreviousPeriodGiao.Add(item.PreviousSanLuongTacDungTongKenhGiao ?? 0.0);
            response.Chart.CurrentPeriodNhan.Add(item.SanLuongTacDungTongKenhNhan);
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

        for (int day = 1; day <= daysInMonth; day++)
        {
            var dayDate = new DateTime(year, month, day);
            var nextDayDate = dayDate.AddDays(1);

            // Chỉ số 0:00 ngày hôm sau - chỉ số 0:00 ngày hôm nay = Sản lượng ngày hôm nay
            var nextDayRecord = FindClosestDailyRecord(records, nextDayDate);
            var currDayRecord = FindClosestDailyRecord(records, dayDate);

            var item = new EnergyConsumptionItemResponse
            {
                TimeLabel = $"{day:D2}/{month:D2}",
                CreatedTime = new DateTimeOffset(dayDate, TimeSpan.FromHours(7)),
                MultiplierFactor = multiplier
            };

            if (nextDayRecord != null && currDayRecord != null)
            {
                item.SanLuongTacDungTongKenhGiao = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, currDayRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, multiplier);
                item.SanLuongTacDungBTKenhGiao = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, multiplier);
                item.SanLuongTacDungCDKenhGiao = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, multiplier);
                item.SanLuongTacDungTDKenhGiao = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, multiplier);
                item.SanLuongPhanKhangTongKenhGiao = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, currDayRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, multiplier);

                item.SanLuongTacDungTongKenhNhan = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, currDayRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, multiplier);
                item.SanLuongTacDungBTKenhNhan = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, multiplier);
                item.SanLuongTacDungCDKenhNhan = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, multiplier);
                item.SanLuongTacDungTDKenhNhan = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, currDayRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, multiplier);
                item.SanLuongPhanKhangTongKenhNhan = CalculateDelta(nextDayRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, currDayRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, multiplier);
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
        var year = request.Year > 0 ? request.Year : DateTime.UtcNow.AddHours(7).Year;
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
                    item.PreviousSanLuongTacDungTongKenhNhan = prev.SanLuongTacDungTongKenhNhan;

                    item.DeltaKenhGiao = Math.Round(item.SanLuongTacDungTongKenhGiao - prev.SanLuongTacDungTongKenhGiao, 3);
                    item.DeltaPercentageKenhGiao = prev.SanLuongTacDungTongKenhGiao > 0
                        ? Math.Round(((item.SanLuongTacDungTongKenhGiao - prev.SanLuongTacDungTongKenhGiao) / prev.SanLuongTacDungTongKenhGiao) * 100.0, 1)
                        : null;

                    item.DeltaKenhNhan = Math.Round(item.SanLuongTacDungTongKenhNhan - prev.SanLuongTacDungTongKenhNhan, 3);
                    item.DeltaPercentageKenhNhan = prev.SanLuongTacDungTongKenhNhan > 0
                        ? Math.Round(((item.SanLuongTacDungTongKenhNhan - prev.SanLuongTacDungTongKenhNhan) / prev.SanLuongTacDungTongKenhNhan) * 100.0, 1)
                        : null;
                }
            }

            response.Items.Add(item);
            response.Chart.Labels.Add(item.TimeLabel);
            response.Chart.CurrentPeriodGiao.Add(item.SanLuongTacDungTongKenhGiao);
            response.Chart.PreviousPeriodGiao.Add(item.PreviousSanLuongTacDungTongKenhGiao ?? 0.0);
            response.Chart.CurrentPeriodNhan.Add(item.SanLuongTacDungTongKenhNhan);
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
        for (int m = 1; m <= 12; m++)
        {
            var startOfMonth = new DateTime(year, m, 1);
            var endOfMonth = startOfMonth.AddMonths(1); // 0:00 ngày đầu tháng sau

            var startRecord = FindClosestDailyRecord(records, startOfMonth);
            var endRecord = FindClosestDailyRecord(records, endOfMonth);

            var item = new EnergyConsumptionItemResponse
            {
                TimeLabel = $"Tháng {m:D2}/{year}",
                CreatedTime = new DateTimeOffset(startOfMonth, TimeSpan.FromHours(7)),
                MultiplierFactor = multiplier
            };

            if (startRecord != null && endRecord != null)
            {
                item.SanLuongTacDungTongKenhGiao = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, startRecord.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, multiplier);
                item.SanLuongTacDungBTKenhGiao = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, multiplier);
                item.SanLuongTacDungCDKenhGiao = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, multiplier);
                item.SanLuongTacDungTDKenhGiao = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, multiplier);
                item.SanLuongPhanKhangTongKenhGiao = CalculateDelta(endRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, startRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, multiplier);

                item.SanLuongTacDungTongKenhNhan = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, startRecord.ChiSoTichLuyDienNangTacDungAPTongKenhNhan, multiplier);
                item.SanLuongTacDungBTKenhNhan = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhNhan, multiplier);
                item.SanLuongTacDungCDKenhNhan = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhNhan, multiplier);
                item.SanLuongTacDungTDKenhNhan = CalculateDelta(endRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, startRecord.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhNhan, multiplier);
                item.SanLuongPhanKhangTongKenhNhan = CalculateDelta(endRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, startRecord.ChiSoTichLuyDienNangPhanKhangAQTongKenhNhan, multiplier);
            }

            result.Add(item);
        }

        return result;
    }
    #endregion

    #region Helper Methods
    private static double CalculateDelta(string? currentStr, string? prevStr, double multiplier)
    {
        if (string.IsNullOrWhiteSpace(currentStr) || string.IsNullOrWhiteSpace(prevStr))
            return 0.0;

        if (!double.TryParse(currentStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var currentVal) ||
            !double.TryParse(prevStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var prevVal))
            return 0.0;

        var delta = currentVal - prevVal;
        // Xử lý chống số âm do lỗi hoặc quay vòng
        if (delta < 0)
        {
            delta = 0;
        }

        return Math.Round(delta * multiplier, 3);
    }

    private static MeasurementPointHalfHourEnergyReportEntity? FindClosestRecord(
        List<MeasurementPointHalfHourEnergyReportEntity> records,
        DateTime targetTime)
    {
        return records.FirstOrDefault(r =>
        {
            if (!r.CreatedTime.HasValue) return false;
            var local = r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime;
            return Math.Abs((local - targetTime).TotalMinutes) <= 15;
        });
    }

    private static MeasurementPointDailyEnergyReportEntity? FindClosestDailyRecord(
        List<MeasurementPointDailyEnergyReportEntity> records,
        DateTime targetDate)
    {
        return records.FirstOrDefault(r =>
        {
            if (!r.CreatedTime.HasValue) return false;
            var local = r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7)).DateTime;
            return local.Year == targetDate.Year && local.Month == targetDate.Month && local.Day == targetDate.Day;
        });
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
            SanLuongTacDungTongKenhGiao = Math.Round(items.Sum(x => x.SanLuongTacDungTongKenhGiao), 3),
            SanLuongTacDungBTKenhGiao = Math.Round(items.Sum(x => x.SanLuongTacDungBTKenhGiao), 3),
            SanLuongTacDungCDKenhGiao = Math.Round(items.Sum(x => x.SanLuongTacDungCDKenhGiao), 3),
            SanLuongTacDungTDKenhGiao = Math.Round(items.Sum(x => x.SanLuongTacDungTDKenhGiao), 3),
            SanLuongPhanKhangTongKenhGiao = Math.Round(items.Sum(x => x.SanLuongPhanKhangTongKenhGiao), 3),

            SanLuongTacDungTongKenhNhan = Math.Round(items.Sum(x => x.SanLuongTacDungTongKenhNhan), 3),
            SanLuongTacDungBTKenhNhan = Math.Round(items.Sum(x => x.SanLuongTacDungBTKenhNhan), 3),
            SanLuongTacDungCDKenhNhan = Math.Round(items.Sum(x => x.SanLuongTacDungCDKenhNhan), 3),
            SanLuongTacDungTDKenhNhan = Math.Round(items.Sum(x => x.SanLuongTacDungTDKenhNhan), 3),
            SanLuongPhanKhangTongKenhNhan = Math.Round(items.Sum(x => x.SanLuongPhanKhangTongKenhNhan), 3),

            PreviousSanLuongTacDungTongKenhGiao = Math.Round(items.Sum(x => x.PreviousSanLuongTacDungTongKenhGiao ?? 0), 3),
            PreviousSanLuongTacDungTongKenhNhan = Math.Round(items.Sum(x => x.PreviousSanLuongTacDungTongKenhNhan ?? 0), 3)
        };

        total.DeltaKenhGiao = Math.Round(total.SanLuongTacDungTongKenhGiao - (total.PreviousSanLuongTacDungTongKenhGiao ?? 0), 3);
        total.DeltaPercentageKenhGiao = (total.PreviousSanLuongTacDungTongKenhGiao ?? 0) > 0
            ? Math.Round(((total.SanLuongTacDungTongKenhGiao - total.PreviousSanLuongTacDungTongKenhGiao!.Value) / total.PreviousSanLuongTacDungTongKenhGiao.Value) * 100.0, 1)
            : null;

        total.DeltaKenhNhan = Math.Round(total.SanLuongTacDungTongKenhNhan - (total.PreviousSanLuongTacDungTongKenhNhan ?? 0), 3);
        total.DeltaPercentageKenhNhan = (total.PreviousSanLuongTacDungTongKenhNhan ?? 0) > 0
            ? Math.Round(((total.SanLuongTacDungTongKenhNhan - total.PreviousSanLuongTacDungTongKenhNhan!.Value) / total.PreviousSanLuongTacDungTongKenhNhan.Value) * 100.0, 1)
            : null;

        return total;
    }
    #endregion
}
