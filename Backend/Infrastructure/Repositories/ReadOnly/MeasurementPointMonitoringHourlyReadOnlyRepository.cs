using System.Globalization;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class MeasurementPointMonitoringHourlyReadOnlyRepository : RestfulRepository<MeasurementPointMonitoringHourlyEntity, AppReadOnlyDbContext>, IMeasurementPointMonitoringHourlyReadOnlyRepository
{
    private readonly DbSet<MeasurementPointMonitoringHourlyEntity> _measurementPointMonitoringHourlyEntities;
    private readonly DbSet<GcsCutoffCycleEntity> _gcsCutoffCycleEntities;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserCustomerReadWriteRepository _userCustomerReadWriteRepository;

    public MeasurementPointMonitoringHourlyReadOnlyRepository(AppReadOnlyDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService, ICurrentUserService currentUserService, IUserReadOnlyRepository userReadOnlyRepository, IUserCustomerReadWriteRepository userCustomerReadWriteRepository) : base(applicationDbContext, snowflakeIdService)
    {
        _currentUserService = currentUserService;
        _userReadOnlyRepository = userReadOnlyRepository;
        _userCustomerReadWriteRepository = userCustomerReadWriteRepository;
        _measurementPointMonitoringHourlyEntities = applicationDbContext.Set<MeasurementPointMonitoringHourlyEntity>();
        _gcsCutoffCycleEntities = applicationDbContext.Set<GcsCutoffCycleEntity>();
    }

    public async Task<LoadChartResponse?> GetLoadChartAsync(LoadChartRequest request, CancellationToken cancellationToken)
    {
        var currentTime = new DateTimeOffset(request.Date.AddHours(7).Year, request.Date.AddHours(7).Month, request.Date.AddHours(7).Day, 0, 0, 0, TimeSpan.Zero);
        var startTime = new DateTimeOffset();
        var endTime = new DateTimeOffset();
        var previousStartTime = new DateTimeOffset();
        var previousEndTime = new DateTimeOffset();
        var response = new LoadChartResponse();
        if (request.TimeSelector == "Daily")
        {
            var previousTime = currentTime.AddDays(-1);
            startTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0, currentTime.Offset);
            endTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, 23, 59, 59, currentTime.Offset);
            previousStartTime = new DateTimeOffset(previousTime.Year, previousTime.Month, previousTime.Day, 0, 0, 0, previousTime.Offset);
            previousEndTime = new DateTimeOffset(previousTime.Year, previousTime.Month, previousTime.Day, 23, 59, 59, previousTime.Offset);
        }
        if (request.TimeSelector == "Monthly")
        {
            var previousTime = currentTime.AddMonths(-1);
            startTime = new DateTimeOffset(currentTime.Year, currentTime.Month, 1, 0, 0, 0, currentTime.Offset);
            endTime = new DateTimeOffset(currentTime.Year, currentTime.Month, DateTime.DaysInMonth(currentTime.Year, currentTime.Month), 23, 59, 59, currentTime.Offset);
            previousStartTime = new DateTimeOffset(previousTime.Year, previousTime.Month, 1, 0, 0, 0, previousTime.Offset);
            previousEndTime = new DateTimeOffset(previousTime.Year, previousTime.Month, DateTime.DaysInMonth(previousTime.Year, previousTime.Month), 23, 59, 59, previousTime.Offset);
        }
        if (request.TimeSelector == "Yearly")
        {
            var previousTime = currentTime.AddYears(-1);
            startTime = new DateTimeOffset(currentTime.Year, 1, 1, 0, 0, 0, currentTime.Offset);
            endTime = new DateTimeOffset(currentTime.Year, 12, DateTime.DaysInMonth(currentTime.Year, 12), 23, 59, 59, currentTime.Offset);
            previousStartTime = new DateTimeOffset(previousTime.Year, 1, 1, 0, 0, 0, previousTime.Offset);
            previousEndTime = new DateTimeOffset(previousTime.Year, 12, DateTime.DaysInMonth(previousTime.Year, 12), 23, 59, 59, previousTime.Offset);
        }

        var dateNow = _measurementPointMonitoringHourlyEntities
            .AsNoTracking()
            .Where(x => x.CreatedTime < endTime.AddHours(-7) && x.CreatedTime > startTime.AddHours(-7) && x.MeasurementPoint.Customer.Id == request.CustomerId && x.MeasurementPoint.Id == request.MeasurementPointId);
        var previousDate = _measurementPointMonitoringHourlyEntities
            .AsNoTracking()
            .Where(x => x.CreatedTime < previousEndTime.AddHours(-7) && x.CreatedTime > previousStartTime.AddHours(-7)  && x.MeasurementPoint.Customer.Id == request.CustomerId && x.MeasurementPoint.Id == request.MeasurementPointId);
        if (request.TimeSelector == "Daily")
        {
            response.Date = currentTime;
            response.PreviousPeriodDate = currentTime.AddDays(-1);
            var dailyData = await dateNow.ToListAsync(cancellationToken);
            var dailyDataPrevious = await previousDate.ToListAsync(cancellationToken);
            response.CurrentPeriodData = CreatePeriodData(dailyData, "Daily", request.Type, currentTime.Year, currentTime.Month);
            response.PreviousPeriodData = CreatePeriodData(dailyDataPrevious, "Daily", request.Type, currentTime.Year, currentTime.Month, false);
        }
        if (request.TimeSelector == "Monthly")
        {
            response.Date = request.Date.AddHours(7);
            response.PreviousPeriodDate = request.Date.AddHours(7).AddMonths(-1);
            var dailyData = dateNow
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { Day = x.CreatedTime.Value.AddHours(7).Day, x.MeasurementPointId })
                // .Where(x => x.Count() >= 24)
                .Select(x => new MeasurementPointMonitoringHourlyEntity
                {
                    Id = x.Key.MeasurementPointId,
                    CreatedTime = x.Min(e => e.CreatedTime),
                    TrungBinhCongSuatTacDungTong3Pha = x.Average(x => double.Parse(x.TrungBinhCongSuatTacDungTong3Pha, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhCongSuatPhanKhangTong3Pha = x.Average(x => double.Parse(x.TrungBinhCongSuatPhanKhangTong3Pha, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhCongSuatPhanKhangTong3PhaUnit = x.FirstOrDefault()?.TrungBinhCongSuatPhanKhangTong3PhaUnit ?? string.Empty,
                })
                .ToList();
            var dailyDataPrevious = previousDate
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { Day = x.CreatedTime.Value.AddHours(7).Day, x.MeasurementPointId })
                .Select(x => new MeasurementPointMonitoringHourlyEntity
                {
                    Id = x.Key.MeasurementPointId,
                    CreatedTime = x.Min(e => e.CreatedTime),
                    TrungBinhCongSuatTacDungTong3Pha = x.Average(x => double.Parse(x.TrungBinhCongSuatTacDungTong3Pha, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhCongSuatPhanKhangTong3Pha = x.Average(x => double.Parse(x.TrungBinhCongSuatPhanKhangTong3Pha, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhCongSuatPhanKhangTong3PhaUnit = x.FirstOrDefault()?.TrungBinhCongSuatPhanKhangTong3PhaUnit ?? string.Empty,
                })
                .ToList();
            response.CurrentPeriodData = CreatePeriodData(dailyData, "Monthly", request.Type, currentTime.Year, currentTime.Month);
            response.PreviousPeriodData = CreatePeriodData(dailyDataPrevious, "Monthly", request.Type, currentTime.AddMonths(-1).Year, currentTime.AddMonths(-1).Month, false);
        }

        if (request.TimeSelector == "Yearly")
        {
            response.Date = request.Date;
            response.PreviousPeriodDate = request.Date.AddYears(-1);
            var dailyData = dateNow
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { Day = x.CreatedTime.Value.AddHours(7).Month, x.MeasurementPointId })
                .Select(x => new MeasurementPointMonitoringHourlyEntity
                {
                    Id = x.Key.MeasurementPointId,
                    CreatedTime = x.Min(e => e.CreatedTime),
                    TrungBinhCongSuatTacDungTong3Pha = x.Average(x => double.Parse(x.TrungBinhCongSuatTacDungTong3Pha, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhCongSuatPhanKhangTong3Pha = x.Average(x => double.Parse(x.TrungBinhCongSuatPhanKhangTong3Pha, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhCongSuatPhanKhangTong3PhaUnit = x.FirstOrDefault()?.TrungBinhCongSuatPhanKhangTong3PhaUnit ?? string.Empty,
                })
                .ToList();
            var dailyDataPrevious = previousDate
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { Day = x.CreatedTime.Value.AddHours(7).Month, x.MeasurementPointId })
                .Select(x => new MeasurementPointMonitoringHourlyEntity
                {
                    Id = x.Key.MeasurementPointId,
                    CreatedTime = x.Min(e => e.CreatedTime),
                    TrungBinhCongSuatTacDungTong3Pha = x.Average(x => double.Parse(x.TrungBinhCongSuatTacDungTong3Pha, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhCongSuatPhanKhangTong3Pha = x.Average(x => double.Parse(x.TrungBinhCongSuatPhanKhangTong3Pha, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhCongSuatPhanKhangTong3PhaUnit = x.FirstOrDefault()?.TrungBinhCongSuatPhanKhangTong3PhaUnit ?? string.Empty,
                })
                .ToList();
            response.CurrentPeriodData = CreatePeriodData(dailyData, "Yearly", request.Type, currentTime.Year, currentTime.Month);
            response.PreviousPeriodData = CreatePeriodData(dailyDataPrevious, "Yearly", request.Type, currentTime.AddMonths(-1).Year, currentTime.AddMonths(-1).Month, false);
        }

        return response;
    }

    private List<DataPoint> CreatePeriodData(List<MeasurementPointMonitoringHourlyEntity> data, string timeSelector, string type, int currentYear, int currentMonth, bool isPrevious = false)
    {
        var periodDataMap = new Dictionary<int, (double Value, string Unit)>();
        int totalPeriods;

        if (timeSelector == "Daily")
        {
            totalPeriods = 24; // 24 hours in a day
            foreach (var item in data)
            {
                // Get the hour, adjusting for previous period if needed
                var hour = item.CreatedTime.Value.AddHours(7).Hour;
                hour = hour == 0 ? 24 : hour;
                // Determine the correct value and unit based on the type
                var value = type == "CongSuatTacDung"
                    ? (double.TryParse(item.TrungBinhCongSuatTacDungTong3Pha, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue) ? parsedValue : 0)
                    : (double.TryParse(item.TrungBinhCongSuatPhanKhangTong3Pha, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedValue) ? parsedValue : 0);

                var unit = type == "CongSuatTacDung"
                    ? item.CongSuatTacDungTong3PhaUnit
                    : item.TrungBinhCongSuatPhanKhangTong3PhaUnit;

                periodDataMap[hour] = (value, unit);
            }
        }
        else if (timeSelector == "Monthly")
        {
            totalPeriods = DateTime.DaysInMonth(currentYear, currentMonth); // Get number of days in the current month
            foreach (var item in data)
            {
                // Get the day of the month
                int day = item.CreatedTime.Value.AddHours(7).Day;

                // Determine the correct value and unit based on the type
                var value = type == "CongSuatTacDung"
                    ? (double.TryParse(item.TrungBinhCongSuatTacDungTong3Pha, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue) ? parsedValue : 0)
                    : (double.TryParse(item.TrungBinhCongSuatPhanKhangTong3Pha, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedValue) ? parsedValue : 0);

                var unit = type == "CongSuatTacDung"
                    ? item.CongSuatTacDungTong3PhaUnit
                    : item.TrungBinhCongSuatPhanKhangTong3PhaUnit;

                periodDataMap[day] = (value, unit);
            }
        }
        else if (timeSelector == "Yearly")
        {
            totalPeriods = 12; // 12 months in a year
            foreach (var item in data)
            {
                // Get the month
                int month = item.CreatedTime.Value.AddHours(7).Month;

                // Determine the correct value and unit based on the type
                var value = type == "CongSuatTacDung"
                    ? (double.TryParse(item.TrungBinhCongSuatTacDungTong3Pha, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue) ? parsedValue : 0)
                    : (double.TryParse(item.TrungBinhCongSuatPhanKhangTong3Pha, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedValue) ? parsedValue : 0);

                var unit = type == "CongSuatTacDung"
                    ? item.CongSuatTacDungTong3PhaUnit
                    : item.TrungBinhCongSuatPhanKhangTong3PhaUnit;

                periodDataMap[month] = (value, unit);
            }
        }
        else
        {
            throw new ArgumentException("Invalid TimeSelector");
        }

        // Generate the full period data points (hourly for daily, days for monthly, months for yearly)
        return Enumerable.Range(1, totalPeriods).Select(period => new DataPoint
        {
            Label = period.ToString(CultureInfo.InvariantCulture),
            Value = periodDataMap.ContainsKey(period) ? periodDataMap[period].Value : 0,
            Unit = periodDataMap.ContainsKey(period) ? periodDataMap[period].Unit : string.Empty
        }).ToList();
    }

    public async Task<VoltageChartResponse?> GetVoltageChartAsync(VoltageChartRequest request, CancellationToken cancellationToken)
    {
        var currentTime = new DateTimeOffset(request.Date.AddHours(7).Year, request.Date.AddHours(7).Month, request.Date.AddHours(7).Day, 0, 0, 0, TimeSpan.Zero);
        var startTime = new DateTimeOffset();
        var endTime = new DateTimeOffset();
        var response = new VoltageChartResponse();
        if (request.TimeSelector == "Daily")
        {
            startTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0, currentTime.Offset);
            endTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, 23, 59, 59, currentTime.Offset);
        }
        if (request.TimeSelector == "Monthly")
        {
            startTime = new DateTimeOffset(currentTime.Year, currentTime.Month, 1, 0, 0, 0, currentTime.Offset);
            endTime = new DateTimeOffset(currentTime.Year, currentTime.Month, DateTime.DaysInMonth(currentTime.Year, currentTime.Month), 23, 59, 59, currentTime.Offset);
        }

        var dateNow = _measurementPointMonitoringHourlyEntities
            .AsNoTracking()
            .Where(x => x.CreatedTime < endTime.AddHours(-7) && x.CreatedTime > startTime.AddHours(-7) &&
                        x.MeasurementPoint.Customer.Id == request.CustomerId &&
                        x.MeasurementPoint.Id == request.MeasurementPointId);

        if (request.TimeSelector == "Daily")
        {
            response.Date = request.Date;
            var dailyData = await dateNow.ToListAsync(cancellationToken);
            response.CurrentPeriodData = CreatePeriodVoltageData(dailyData, "Daily", request.Type, currentTime.Year, currentTime.Month);
        }
        if (request.TimeSelector == "Monthly")
        {
            response.Date = request.Date;
            var monthlyData = dateNow
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { Day = x.CreatedTime.Value.AddHours(7).Day, x.MeasurementPointId })
                // .Where(g => g.Count() >= 24)
                .Select(x => new MeasurementPointMonitoringHourlyEntity
                {
                    Id = x.Key.MeasurementPointId,
                    CreatedTime = x.Min(e => e.CreatedTime),
                    TrungBinhDienApPhaA = x.Average(e => double.Parse(e.TrungBinhDienApPhaA, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhDienApPhaB = x.Average(e => double.Parse(e.TrungBinhDienApPhaB, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhDienApPhaC = x.Average(e => double.Parse(e.TrungBinhDienApPhaC, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhDongDienPhaA = x.Average(e => double.Parse(e.TrungBinhDongDienPhaA, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhDongDienPhaB = x.Average(e => double.Parse(e.TrungBinhDongDienPhaB, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhDongDienPhaC = x.Average(e => double.Parse(e.TrungBinhDongDienPhaC, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    TrungBinhDienApPhaAUnit = x.FirstOrDefault()?.TrungBinhDienApPhaAUnit ?? string.Empty,
                    TrungBinhDienApPhaBUnit = x.FirstOrDefault()?.TrungBinhDienApPhaBUnit ?? string.Empty,
                    TrungBinhDienApPhaCUnit = x.FirstOrDefault()?.TrungBinhDienApPhaCUnit ?? string.Empty,
                    TrungBinhDongDienPhaAUnit = x.FirstOrDefault()?.TrungBinhDongDienPhaAUnit ?? string.Empty,
                    TrungBinhDongDienPhaBUnit = x.FirstOrDefault()?.TrungBinhDongDienPhaBUnit ?? string.Empty,
                    TrungBinhDongDienPhaCUnit = x.FirstOrDefault()?.TrungBinhDongDienPhaCUnit ?? string.Empty,
                })
                .ToList();
            response.CurrentPeriodData = CreatePeriodVoltageData(monthlyData, "Monthly", request.Type, currentTime.Year, currentTime.Month);
        }
        return response;
    }

    private List<VoltageDataPoint> CreatePeriodVoltageData(List<MeasurementPointMonitoringHourlyEntity> data, string timeSelector, string type, int currentYear, int currentMonth)
    {
        var periodDataMap = new Dictionary<int, VoltageDataPoint>();
        int totalPeriods;

        if (timeSelector == "Daily")
        {
            totalPeriods = 24;
            foreach (var item in data)
            {
                var hour = item.CreatedTime.Value.AddHours(7).Hour;
                hour = hour == 0 ? 24 : hour;

                if (!periodDataMap.ContainsKey(hour))
                {
                    periodDataMap[hour] = new VoltageDataPoint
                    {
                        Label = hour.ToString(CultureInfo.InvariantCulture)
                    };
                }

                // Assuming you have PhaA, PhaB, PhaC fields for each data point
                periodDataMap[hour].PhaA = type == "DienAp" ? double.Parse(item.TrungBinhDienApPhaA, CultureInfo.InvariantCulture) : double.Parse(item.TrungBinhDongDienPhaA, CultureInfo.InvariantCulture);
                periodDataMap[hour].PhaAUnit = type == "DienAp" ? item.TrungBinhDienApPhaAUnit : item.TrungBinhDongDienPhaAUnit;
                periodDataMap[hour].PhaB = type == "DienAp" ? double.Parse(item.TrungBinhDienApPhaB, CultureInfo.InvariantCulture) : double.Parse(item.TrungBinhDongDienPhaB, CultureInfo.InvariantCulture);
                periodDataMap[hour].PhaBUnit = type == "DienAp" ? item.TrungBinhDienApPhaBUnit : item.TrungBinhDongDienPhaBUnit;
                periodDataMap[hour].PhaC = type == "DienAp" ? double.Parse(item.TrungBinhDienApPhaC, CultureInfo.InvariantCulture) : double.Parse(item.TrungBinhDongDienPhaC, CultureInfo.InvariantCulture);
                periodDataMap[hour].PhaCUnit = type == "DienAp" ? item.TrungBinhDienApPhaCUnit : item.TrungBinhDongDienPhaCUnit;
            }
        }
        else if (timeSelector == "Monthly")
        {
            totalPeriods = DateTime.DaysInMonth(currentYear, currentMonth); // Number of days in the current month
            foreach (var item in data)
            {
                int day = item.CreatedTime.Value.AddHours(7).Day;

                if (!periodDataMap.ContainsKey(day))
                {
                    periodDataMap[day] = new VoltageDataPoint
                    {
                        Label = day.ToString(CultureInfo.InvariantCulture)
                    };
                }

                periodDataMap[day].PhaA = type == "DienAp" ? double.Parse(item.TrungBinhDienApPhaA, CultureInfo.InvariantCulture) : double.Parse(item.TrungBinhDongDienPhaA, CultureInfo.InvariantCulture);
                periodDataMap[day].PhaAUnit = type == "DienAp" ? item.TrungBinhDienApPhaAUnit : item.TrungBinhDongDienPhaAUnit;
                periodDataMap[day].PhaB = type == "DienAp" ? double.Parse(item.TrungBinhDienApPhaB, CultureInfo.InvariantCulture) : double.Parse(item.TrungBinhDongDienPhaB, CultureInfo.InvariantCulture);
                periodDataMap[day].PhaBUnit = type == "DienAp" ? item.TrungBinhDienApPhaBUnit : item.TrungBinhDongDienPhaBUnit;
                periodDataMap[day].PhaC = type == "DienAp" ? double.Parse(item.TrungBinhDienApPhaC, CultureInfo.InvariantCulture) : double.Parse(item.TrungBinhDongDienPhaC, CultureInfo.InvariantCulture);
                periodDataMap[day].PhaCUnit = type == "DienAp" ? item.TrungBinhDienApPhaCUnit : item.TrungBinhDongDienPhaCUnit;
            }
        }
        else
        {
            throw new ArgumentException("Invalid TimeSelector");
        }

        // Return a list of VoltageDataPoints
        return Enumerable.Range(1, totalPeriods).Select(period => new VoltageDataPoint
        {
            Label = period.ToString(CultureInfo.InvariantCulture),
            PhaA = periodDataMap.ContainsKey(period) ? periodDataMap[period].PhaA : 0,
            PhaAUnit = periodDataMap.ContainsKey(period) ? periodDataMap[period].PhaAUnit : string.Empty,
            PhaB = periodDataMap.ContainsKey(period) ? periodDataMap[period].PhaB : 0,
            PhaBUnit = periodDataMap.ContainsKey(period) ? periodDataMap[period].PhaBUnit : string.Empty,
            PhaC = periodDataMap.ContainsKey(period) ? periodDataMap[period].PhaC : 0,
            PhaCUnit = periodDataMap.ContainsKey(period) ? periodDataMap[period].PhaCUnit : string.Empty,
        }).ToList();
    }

    public async Task<PowerChartResponse?> GetPowerChartAsync(PowerChartRequest request, CancellationToken cancellationToken)
    {
        var currentTime = new DateTimeOffset(request.Date.AddHours(7).Year, request.Date.AddHours(7).Month, request.Date.AddHours(7).Day, 0, 0, 0, TimeSpan.Zero);
        var startTime = new DateTimeOffset();
        var endTime = new DateTimeOffset();
        var response = new PowerChartResponse();
        if (request.TimeSelector == "Daily")
        {
            startTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0, currentTime.Offset);
            endTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, 23, 59, 59, currentTime.Offset);
        }
        if (request.TimeSelector == "Monthly")
        {
            startTime = new DateTimeOffset(currentTime.Year, currentTime.Month, 1, 0, 0, 0, currentTime.Offset);
            endTime = new DateTimeOffset(currentTime.Year, currentTime.Month, DateTime.DaysInMonth(currentTime.Year, currentTime.Month), 23, 59, 59, currentTime.Offset);
        }

        var dateNow = _measurementPointMonitoringHourlyEntities
            .AsNoTracking()
            .Where(x => x.CreatedTime < endTime.AddHours(-7) && x.CreatedTime > startTime.AddHours(-7) &&
                        x.MeasurementPoint.Customer.Id == request.CustomerId &&
                        x.MeasurementPoint.Id == request.MeasurementPointId);

        if (request.TimeSelector == "Daily")
        {
            response.Date = request.Date;
            var dailyData = await dateNow.ToListAsync(cancellationToken);
            response.CurrentPeriodData = CreatePeriodPowerData(dailyData, "Daily", request.Type, currentTime.Year, currentTime.Month);
        }
        if (request.TimeSelector == "Monthly")
        {
            response.Date = request.Date;
            var monthlyData = dateNow
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { Day = x.CreatedTime.Value.AddHours(7).Day, x.MeasurementPointId })
                .Select(x => new MeasurementPointMonitoringHourlyEntity
                {
                    Id = x.Key.MeasurementPointId,
                    CreatedTime = x.Min(e => e.CreatedTime),
                    CongSuatTacDungTong3PhaCucDai = x.Max(e => double.Parse(e.CongSuatTacDungTong3PhaCucDai, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    CongSuatPhanKhangTong3PhaCucDai = x.Max(e => double.Parse(e.CongSuatPhanKhangTong3PhaCucDai, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    CongSuatPhanKhangTong3PhaCucTieu = x.Min(e => double.Parse(e.CongSuatPhanKhangTong3PhaCucTieu, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    CongSuatTacDungTong3PhaCucTieu = x.Min(e => double.Parse(e.CongSuatTacDungTong3PhaCucTieu, CultureInfo.InvariantCulture)).ToString(CultureInfo.InvariantCulture),
                    CongSuatTacDungTong3PhaUnit = x.FirstOrDefault()?.CongSuatTacDungTong3PhaUnit ?? string.Empty,
                    TrungBinhCongSuatPhanKhangTong3PhaUnit = x.FirstOrDefault()?.TrungBinhCongSuatPhanKhangTong3PhaUnit ?? string.Empty,
                })
                .ToList();
            response.CurrentPeriodData = CreatePeriodPowerData(monthlyData, "Monthly", request.Type, currentTime.Year, currentTime.Month);
        }
        return response;
    }

    private List<DataPointPowerChart> CreatePeriodPowerData(List<MeasurementPointMonitoringHourlyEntity> data, string timeSelector, string type, int currentYear, int currentMonth, bool isPrevious = false)
{
    var periodDataMap = new Dictionary<int, (double MaxValue, double MinValue, string Unit)>();
    int totalPeriods;

    if (timeSelector == "Daily")
    {
        totalPeriods = 24;
        foreach (var item in data)
        {
            var hour = item.CreatedTime.Value.AddHours(7).Hour;
            hour = hour == 0 ? 24 : hour;

            (double maxValue, double minValue, string unit) = type switch
            {
                "CongSuatTacDung" => (
                    double.Parse(item.CongSuatTacDungTong3PhaCucDai, CultureInfo.InvariantCulture),
                    double.Parse(item.CongSuatTacDungTong3PhaCucTieu, CultureInfo.InvariantCulture),
                    item.CongSuatTacDungTong3PhaUnit
                ),
                "CongSuatPhanKhang" => (
                    double.Parse(item.CongSuatPhanKhangTong3PhaCucDai, CultureInfo.InvariantCulture),
                    double.Parse(item.CongSuatPhanKhangTong3PhaCucTieu, CultureInfo.InvariantCulture),
                    item.TrungBinhCongSuatPhanKhangTong3PhaUnit
                ),
                _ => throw new ArgumentException("Invalid type")
            };

            periodDataMap[hour] = (maxValue, minValue, unit);
        }
    }
    else if (timeSelector == "Monthly")
    {
        totalPeriods = DateTime.DaysInMonth(currentYear, currentMonth);
        foreach (var item in data)
        {
            var day = item.CreatedTime.Value.AddHours(7).Day;

            (double maxValue, double minValue, string unit) = type switch
            {
                "CongSuatTacDung" => (
                    double.Parse(item.CongSuatTacDungTong3PhaCucDai, CultureInfo.InvariantCulture),
                    double.Parse(item.CongSuatTacDungTong3PhaCucTieu, CultureInfo.InvariantCulture),
                    item.CongSuatTacDungTong3PhaUnit
                ),
                "CongSuatPhanKhang" => (
                    double.Parse(item.CongSuatPhanKhangTong3PhaCucDai, CultureInfo.InvariantCulture),
                    double.Parse(item.CongSuatPhanKhangTong3PhaCucTieu, CultureInfo.InvariantCulture),
                    item.TrungBinhCongSuatPhanKhangTong3PhaUnit
                ),
                _ => throw new ArgumentException("Invalid type")
            };

            periodDataMap[day] = (maxValue, minValue, unit);
        }
    }
    else
    {
        throw new ArgumentException("Invalid TimeSelector");
    }

    // Generate the full period data points
    return Enumerable.Range(1, totalPeriods).Select(period => new DataPointPowerChart
    {
        LabelMax = period.ToString(CultureInfo.InvariantCulture),
        LabelMin = period.ToString(CultureInfo.InvariantCulture),
        ValueMax = periodDataMap.ContainsKey(period) ? periodDataMap[period].MaxValue : 0,
        ValueMin = periodDataMap.ContainsKey(period) ? periodDataMap[period].MinValue : 0,
        Unit = periodDataMap.ContainsKey(period) ? periodDataMap[period].Unit : string.Empty
    }).ToList();
}

    public async Task<ConsumptionOutputChartResponse?> GetConsumptionOutputChartAsync(ConsumptionOutputChartRequest request, CancellationToken cancellationToken)
    {
        var currentTime = new DateTimeOffset(request.Date.AddHours(7).Year, request.Date.AddHours(7).Month, request.Date.AddHours(7).Day, 0, 0, 0, TimeSpan.Zero);
        var startTime = new DateTimeOffset();
        var endTime = new DateTimeOffset();
        var previousStartTime = new DateTimeOffset();
        var previousEndTime = new DateTimeOffset();
        var response = new ConsumptionOutputChartResponse();
        if (request.TimeSelector == "Daily")
        {
            var previousTime = currentTime.AddDays(-1);
            startTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0, currentTime.Offset);
            endTime = new DateTimeOffset(currentTime.Year, currentTime.Month, currentTime.Day, 23, 59, 59, currentTime.Offset);
            previousStartTime = new DateTimeOffset(previousTime.Year, previousTime.Month, previousTime.Day, 0, 0, 0, previousTime.Offset);
            previousEndTime = new DateTimeOffset(previousTime.Year, previousTime.Month, previousTime.Day, 23, 59, 59, previousTime.Offset);
        }
        if (request.TimeSelector == "Monthly")
        {
            var previousTime = currentTime.AddMonths(-1);
            startTime = new DateTimeOffset(currentTime.Year, currentTime.Month, 1, 0, 0, 0, currentTime.Offset);
            endTime = new DateTimeOffset(currentTime.Year, currentTime.Month, DateTime.DaysInMonth(currentTime.Year, currentTime.Month), 23, 59, 59, currentTime.Offset);
            previousStartTime = new DateTimeOffset(previousTime.Year, previousTime.Month, 1, 0, 0, 0, previousTime.Offset);
            previousEndTime = new DateTimeOffset(previousTime.Year, previousTime.Month, DateTime.DaysInMonth(previousTime.Year, previousTime.Month), 23, 59, 59, previousTime.Offset);
        }
        if (request.TimeSelector == "Yearly")
        {
            var previousTime = currentTime.AddYears(-1);
            startTime = new DateTimeOffset(currentTime.Year, 1, 1, 0, 0, 0, currentTime.Offset);
            endTime = new DateTimeOffset(currentTime.Year, 12, DateTime.DaysInMonth(currentTime.Year, 12), 23, 59, 59, currentTime.Offset);
            previousStartTime = new DateTimeOffset(previousTime.Year, 1, 1, 0, 0, 0, previousTime.Offset);
            previousEndTime = new DateTimeOffset(previousTime.Year, 12, DateTime.DaysInMonth(previousTime.Year, 12), 23, 59, 59, previousTime.Offset);
        }

        var dateNow = _measurementPointMonitoringHourlyEntities
            .AsNoTracking()
            .Where(x => x.CreatedTime < endTime.AddHours(-7) && x.CreatedTime > startTime.AddHours(-7) && x.MeasurementPoint.Customer.Id == request.CustomerId && x.MeasurementPoint.Id == request.MeasurementPointId);
        var previousDate = _measurementPointMonitoringHourlyEntities
            .AsNoTracking()
            .Where(x => x.CreatedTime < previousEndTime.AddHours(-7) && x.CreatedTime > previousStartTime.AddHours(-7)  && x.MeasurementPoint.Customer.Id == request.CustomerId && x.MeasurementPoint.Id == request.MeasurementPointId);
        if (request.TimeSelector == "Daily")
        {
            response.Date = request.Date;
            var dailyData = await dateNow.ToListAsync(cancellationToken);
            var dailyDataPrevious = await previousDate.ToListAsync(cancellationToken);
            response.CurrentPeriodData = CreateConsumtionOutputPeriodData(dailyData, "Daily", currentTime.Year, currentTime.Month);
            response.PreviousPeriodData = CreatePreviousConsumptionOutputDataPoint(dailyDataPrevious, "Daily", currentTime.Year, currentTime.Month, false);
        }
        if (request.TimeSelector == "Monthly")
        {
            response.Date = request.Date;
            var dailyData = dateNow
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { Day = x.CreatedTime.Value.AddHours(7).Day, x.MeasurementPointId })
                // .Where(g => g.Count() >= 24)
                .Select(x => new MeasurementPointMonitoringHourlyEntity
                {
                    Id = x.Key.MeasurementPointId,
                    CreatedTime = x.Min(e => e.CreatedTime),
                    ChiSoTichLuyDienNangTacDungAPBieuGioBT = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioBT == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioBT, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPBieuGioCD = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioCD == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioCD, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPBieuGioTD = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioTD == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioTD, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPTong = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPTong == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPTong, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                })
                .ToList();
            var dailyDataPrevious = previousDate
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { Day = x.CreatedTime.Value.AddHours(7).Day, x.MeasurementPointId })
                .Where(g => g.Count() >= 30)
                .Select(x => new MeasurementPointMonitoringHourlyEntity
                {
                    Id = x.Key.MeasurementPointId,
                    CreatedTime = x.Min(e => e.CreatedTime),
                    ChiSoTichLuyDienNangTacDungAPBieuGioBT = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioBT == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioBT, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPBieuGioCD = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioCD == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioCD, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPBieuGioTD = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioTD == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioTD, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPTong = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPTong == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPTong, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                })
                .ToList();
            response.CurrentPeriodData = CreateConsumtionOutputPeriodData(dailyData, "Monthly", currentTime.Year, currentTime.Month);
            response.PreviousPeriodData = CreatePreviousConsumptionOutputDataPoint(dailyDataPrevious, "Monthly", currentTime.AddMonths(-1).Year, currentTime.AddMonths(-1).Month, false);
        }

        if (request.TimeSelector == "Yearly")
        {
            response.Date = request.Date;
            var dailyData = dateNow
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { Day = x.CreatedTime.Value.AddHours(7).Month, x.MeasurementPointId })
                // .Where(g => g.Count() >= 24)
                .Select(x => new MeasurementPointMonitoringHourlyEntity
                {
                    Id = x.Key.MeasurementPointId,
                    CreatedTime = x.Min(e => e.CreatedTime),
                    ChiSoTichLuyDienNangTacDungAPBieuGioBT = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioBT == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioBT, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPBieuGioCD = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioCD == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioCD, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPBieuGioTD = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioTD == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioTD, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPTong = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPTong == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPTong, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                })
                .ToList();
            var dailyDataPrevious = previousDate
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => new { Day = x.CreatedTime.Value.AddHours(7).Month, x.MeasurementPointId })
                .Select(x => new MeasurementPointMonitoringHourlyEntity
                {
                    Id = x.Key.MeasurementPointId,
                    CreatedTime = x.Min(e => e.CreatedTime),
                    ChiSoTichLuyDienNangTacDungAPBieuGioBT = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioBT == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioBT, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPBieuGioCD = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioCD == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioCD, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPBieuGioTD = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPBieuGioTD == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPBieuGioTD, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                    ChiSoTichLuyDienNangTacDungAPTong = x.Sum(x => Math.Round(
                        x.ChiSoTichLuyDienNangTacDungAPTong == null ? 
                            0 : 
                            double.Parse(x.ChiSoTichLuyDienNangTacDungAPTong, CultureInfo.InvariantCulture), 
                        2)).ToString(),
                })
                .ToList();
            response.CurrentPeriodData = CreateConsumtionOutputPeriodData(dailyData, "Yearly", currentTime.Year, currentTime.Month);
            response.PreviousPeriodData = CreatePreviousConsumptionOutputDataPoint(dailyDataPrevious, "Yearly", currentTime.AddMonths(-1).Year, currentTime.AddMonths(-1).Month, false);
        }

        return response;
    }

    private List<ConsumptionOutputDataPoint> CreateConsumtionOutputPeriodData(List<MeasurementPointMonitoringHourlyEntity> data, string timeSelector, int currentYear, int currentMonth)
    {
        var periodDataMap = new Dictionary<int, ConsumptionOutputDataPoint>();
        int totalPeriods;
        switch (timeSelector)
        {
            case "Daily":
            {
                totalPeriods = 24;
                foreach (var item in data)
                {
                    var hour = item.CreatedTime.Value.AddHours(7).Hour;
                    hour = hour == 0 ? 24 : hour;

                    if (!periodDataMap.ContainsKey(hour))
                    {
                        periodDataMap[hour] = new ConsumptionOutputDataPoint
                        {
                            Label = hour.ToString(CultureInfo.InvariantCulture)
                        };
                    }

                    periodDataMap[hour].Bt = item.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioBT, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[hour].Cd = item.ChiSoTichLuyDienNangTacDungAPBieuGioCD != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioCD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[hour].Td = item.ChiSoTichLuyDienNangTacDungAPBieuGioTD != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioTD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[hour].Sg = item.ChiSoTichLuyDienNangTacDungAPTong != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPTong, CultureInfo.InvariantCulture), 2) : null;
                }

                break;
            }
            case "Monthly":
            {
                totalPeriods = DateTime.DaysInMonth(currentYear, currentMonth);
                foreach (var item in data)
                {
                    int day = item.CreatedTime.Value.AddHours(7).Day;

                    if (!periodDataMap.ContainsKey(day))
                    {
                        periodDataMap[day] = new ConsumptionOutputDataPoint
                        {
                            Label = day.ToString(CultureInfo.InvariantCulture)
                        };
                    }

                    periodDataMap[day].Bt = item.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioBT, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[day].Cd = item.ChiSoTichLuyDienNangTacDungAPBieuGioCD != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioCD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[day].Td = item.ChiSoTichLuyDienNangTacDungAPBieuGioTD != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioTD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[day].Sg = item.ChiSoTichLuyDienNangTacDungAPTong != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPTong, CultureInfo.InvariantCulture), 2) : null;
                }

                break;
            }
            case "Yearly":
            {
                totalPeriods = 12;
                foreach (var item in data)
                {
                    int day = item.CreatedTime.Value.AddHours(7).Month;

                    if (!periodDataMap.ContainsKey(day))
                    {
                        periodDataMap[day] = new ConsumptionOutputDataPoint
                        {
                            Label = day.ToString(CultureInfo.InvariantCulture)
                        };
                    }

                    periodDataMap[day].Bt = item.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioBT, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[day].Cd = item.ChiSoTichLuyDienNangTacDungAPBieuGioCD != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioCD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[day].Td = item.ChiSoTichLuyDienNangTacDungAPBieuGioTD != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioTD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[day].Sg = item.ChiSoTichLuyDienNangTacDungAPTong != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPTong, CultureInfo.InvariantCulture), 2) : null;
                }

                break;
            }
            default:
                throw new ArgumentException("Invalid TimeSelector");
        }

        return Enumerable.Range(1, totalPeriods).Select(period => new ConsumptionOutputDataPoint
        {
            Label = period.ToString(CultureInfo.InvariantCulture),
            Cd = periodDataMap.ContainsKey(period) ? periodDataMap[period].Cd : null,
            Td = periodDataMap.ContainsKey(period) ? periodDataMap[period].Td : null,
            Bt = periodDataMap.ContainsKey(period) ? periodDataMap[period].Bt : null,
            Sg = periodDataMap.ContainsKey(period) ? periodDataMap[period].Sg : null,
        }).ToList();
    }

    private List<PreviousConsumptionOutputDataPoint> CreatePreviousConsumptionOutputDataPoint(List<MeasurementPointMonitoringHourlyEntity> data, string timeSelector, int currentYear, int currentMonth, bool isPrevious = false)
    {
        var periodDataMap = new Dictionary<int, ConsumptionOutputDataPoint>();
        int totalPeriods;
        switch (timeSelector)
        {
            case "Daily":
            {
                totalPeriods = 24;
                foreach (var item in data)
                {
                    var hour = item.CreatedTime.Value.AddHours(7).Hour;
                    hour = hour == 0 ? 24 : hour;

                    if (!periodDataMap.ContainsKey(hour))
                    {
                        periodDataMap[hour] = new ConsumptionOutputDataPoint
                        {
                            Label = hour.ToString(CultureInfo.InvariantCulture)
                        };
                    }

                    periodDataMap[hour].Bt = item.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioBT, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[hour].Cd = item.ChiSoTichLuyDienNangTacDungAPBieuGioCD != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioCD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[hour].Td = item.ChiSoTichLuyDienNangTacDungAPBieuGioTD != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioTD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[hour].Sg = item.ChiSoTichLuyDienNangTacDungAPTong != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPTong, CultureInfo.InvariantCulture), 2) : null;
                }

                break;
            }
            case "Monthly":
            {
                totalPeriods = DateTime.DaysInMonth(currentYear, currentMonth);
                foreach (var item in data)
                {
                    int day = item.CreatedTime.Value.AddHours(7).Day;

                    if (!periodDataMap.ContainsKey(day))
                    {
                        periodDataMap[day] = new ConsumptionOutputDataPoint
                        {
                            Label = day.ToString(CultureInfo.InvariantCulture)
                        };
                    }

                    periodDataMap[day].Bt = item.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioBT, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[day].Cd = item.ChiSoTichLuyDienNangTacDungAPBieuGioCD != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioCD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[day].Td = item.ChiSoTichLuyDienNangTacDungAPBieuGioTD != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioTD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[day].Sg = item.ChiSoTichLuyDienNangTacDungAPTong != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPTong, CultureInfo.InvariantCulture), 2) : null;
                }

                break;
            }
            case "Yearly":
            {
                totalPeriods = 12;
                foreach (var item in data)
                {
                    int month = item.CreatedTime.Value.AddHours(7).Month;

                    if (!periodDataMap.ContainsKey(month))
                    {
                        periodDataMap[month] = new ConsumptionOutputDataPoint
                        {
                            Label = month.ToString(CultureInfo.InvariantCulture)
                        };
                    }

                    periodDataMap[month].Bt = item.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioBT, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[month].Cd = item.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioCD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[month].Td = item.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPBieuGioTD, CultureInfo.InvariantCulture), 2) : null;
                    periodDataMap[month].Sg = item.ChiSoTichLuyDienNangTacDungAPBieuGioBT != null ? Math.Round(double.Parse(item.ChiSoTichLuyDienNangTacDungAPTong, CultureInfo.InvariantCulture), 2) : null;
                }

                break;
            }
            default:
                throw new ArgumentException("Invalid TimeSelector");
        }

        return Enumerable.Range(1, totalPeriods).Select(period => new PreviousConsumptionOutputDataPoint
        {
            Label = period.ToString(CultureInfo.InvariantCulture),
            PreviousCd = periodDataMap.ContainsKey(period) ? periodDataMap[period].Cd : null,
            PreviousTd = periodDataMap.ContainsKey(period) ? periodDataMap[period].Td : null,
            PreviousBt = periodDataMap.ContainsKey(period) ? periodDataMap[period].Bt : null,
            PreviousSg = periodDataMap.ContainsKey(period) ? periodDataMap[period].Sg : null,
        }).ToList();
    }

    public async Task<FinalizedOutputElectricityChartResponse?> GetFinalizedOutputElectricityChartAsync(FinalizedOutputElectricityChartRequest request, CancellationToken cancellationToken)
    {
        var currentTime = new DateTimeOffset(request.Date.AddHours(7).Year, request.Date.AddHours(7).Month, request.Date.AddHours(7).Day, 0, 0, 0, TimeSpan.Zero);
        var response = new FinalizedOutputElectricityChartResponse();

        // Get the last period of previous month for boundary case
        var previousMonthLastPeriod = await _gcsCutoffCycleEntities
            .AsNoTracking()
            .Where(x => x.Year == currentTime.AddMonths(-1).Year && 
                        x.Month == currentTime.AddMonths(-1).Month)
            .OrderByDescending(x => x.PeriodNumber)
            .FirstOrDefaultAsync(cancellationToken);
        
        var hasPreviousMonthData = await _gcsCutoffCycleEntities
            .AsNoTracking()
            .Include(x => x.GcsMonthlyDetail)
            .ThenInclude(gmd => gmd.MeasurementPoint)
            .Where(x => x.Year == currentTime.AddMonths(-1).Year &&
                        x.Month == currentTime.AddMonths(-1).Month &&
                        x.GcsMonthlyDetail.Any(gmd => gmd.MeasurementPointId == request.MeasurementPointId &&
                                                      gmd.MeasurementPoint.CustomerId == request.CustomerId))
            .AnyAsync(cancellationToken);

        // Current period data query
        var dataElectricityDataPoints = await _gcsCutoffCycleEntities
            .AsNoTracking()
            .Include(x => x.GcsMonthlyDetail)
            .ThenInclude(gmd => gmd.MeasurementPoint)
            .Where(x => ((x.Year == currentTime.Year) || 
                         (x.Year == currentTime.Year + 1 && x.PeriodNumber == 1 && x.Month == 1)) &&
                        x.GcsMonthlyDetail.Any(gmd => gmd.MeasurementPointId == request.MeasurementPointId &&
                                                      gmd.MeasurementPoint.CustomerId == request.CustomerId))
            .GroupBy(x => new { x.Month, x.Year, x.PeriodNumber, x.Day })
            .Select(x => new FinalizedOutputElectricityDataPoint()
            {
                Month = x.Key.Month,
                Year = x.Key.Year,
                PeriodNumber = x.Key.PeriodNumber,
                Day = x.Key.Day,
                DayPrevious = x.FirstOrDefault().Gcs.DayPeriod.Last() == 999 ? DateTime.DaysInMonth(x.FirstOrDefault().Year, x.FirstOrDefault().Month) : x.FirstOrDefault().Gcs.DayPeriod.Last(),
                PeriodNumberPrevious = x.FirstOrDefault().Gcs.PeriodNumber,
                Bt = x.SelectMany(y => y.GcsMonthlyDetail.Where(gmd => gmd.MeterReadingSet == "BT" && gmd.MeasurementPointId == request.MeasurementPointId)).Sum(gmd => gmd.Consumption),
                Cd = x.SelectMany(y => y.GcsMonthlyDetail.Where(gmd => gmd.MeterReadingSet == "CD" && gmd.MeasurementPointId == request.MeasurementPointId)).Sum(gmd => gmd.Consumption),
                Td = x.SelectMany(y => y.GcsMonthlyDetail.Where(gmd => gmd.MeterReadingSet == "TD" && gmd.MeasurementPointId == request.MeasurementPointId)).Sum(gmd => gmd.Consumption),
                Sg = x.SelectMany(y => y.GcsMonthlyDetail.Where(gmd => gmd.MeterReadingSet == "SG" && gmd.MeasurementPointId == request.MeasurementPointId)).Sum(gmd => gmd.Consumption),
                Vc = x.SelectMany(y => y.GcsMonthlyDetail.Where(gmd => gmd.MeterReadingSet == "VC" && gmd.MeasurementPointId == request.MeasurementPointId)).Sum(gmd => gmd.Consumption),
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ThenBy(x => x.PeriodNumber)
            .ToListAsync(cancellationToken);

        // Nếu không có dữ liệu, trả về response rỗng
        if (!dataElectricityDataPoints.Any())
        {
            response.CurrentPeriodData = new List<FinalizedOutputElectricityDataPoint>();
            response.PreviousPeriodData = new List<PreviousFinalizedOutputElectricityDataPoint>();
            response.Date = request.Date;
            response.Unit = "kWh";
            return response;
        }

        // Xác định có cần loại bỏ record đầu tiên không
        bool shouldRemoveFirstRecord = dataElectricityDataPoints.First().PeriodNumber == 1 && !hasPreviousMonthData;
        
        // Nếu record đầu tiên là period đầu tiên và không có dữ liệu tháng trước

        // List để lưu dữ liệu đã được xử lý
        var processedData = new List<FinalizedOutputElectricityDataPoint>();

        // Nếu có record đầu tiên và nó không bị loại bỏ
        if (dataElectricityDataPoints.Any() && !shouldRemoveFirstRecord)
        {
            var firstRecord = dataElectricityDataPoints.First();
            
            if (firstRecord.PeriodNumber == 1)
            {
                // First period of month
                var previousDate = currentTime.AddMonths(-1);
                firstRecord.MonthPrevious = previousDate.Month;
                firstRecord.YearPrevious = previousDate.Year;
                
                if (hasPreviousMonthData)
                {
                    firstRecord.DayPrevious = previousMonthLastPeriod.Day;
                    firstRecord.PeriodNumberPrevious = previousMonthLastPeriod.PeriodNumber;
                }
            }
            else
            {
                // Not first period, might have previous in same month
                var previousPeriod = dataElectricityDataPoints
                    .FirstOrDefault(x => x.PeriodNumber == firstRecord.PeriodNumber - 1);
                
                if (previousPeriod != null)
                {
                    firstRecord.MonthPrevious = previousPeriod.Month ?? 0;
                    firstRecord.YearPrevious = previousPeriod.Year ?? 0;
                    firstRecord.DayPrevious = previousPeriod.Day ?? 0;
                    firstRecord.PeriodNumberPrevious = previousPeriod.PeriodNumber ?? 0;
                }
            }
            
            processedData.Add(firstRecord);
        }

        // Xử lý các record còn lại
        for (int i = shouldRemoveFirstRecord ? 1 : 1; i < dataElectricityDataPoints.Count; i++)
        {
            var current = dataElectricityDataPoints[i];
            var previous = dataElectricityDataPoints[i - 1];
            
            current.MonthPrevious = previous.Month ?? 0;
            current.YearPrevious = previous.Year ?? 0;
            current.DayPrevious = previous.Day ?? 0;
            current.PeriodNumberPrevious = previous.PeriodNumber ?? 0;
            
            processedData.Add(current);
        }

        // Response setup
        response.CurrentPeriodData = processedData
            .Where(x => ((x.Year == currentTime.Year && !(x.PeriodNumber == 1 && x.Month == 1)) || 
                         (x.Year == currentTime.Year + 1 && x.PeriodNumber == 1 && x.Month == 1)))
            .ToList();
        response.PreviousPeriodData = new List<PreviousFinalizedOutputElectricityDataPoint>();
        response.Date = request.Date;
        response.Unit = "kWh";

        return response;
    }

    public async Task<List<ListFinalizedOutputElectricityResponse>> GetListFinalizedOutputElectricityAsync(ListFinalizedOutputElectricityRequest request, CancellationToken cancellationToken)
    {
        var currentId = _currentUserService.UserId;
        var user = await _userReadOnlyRepository.GetUserDetailAsync(currentId, cancellationToken);
        var userCustomer = await _userCustomerReadWriteRepository.GetUserCustomerByUserIdAsync(currentId, cancellationToken);
        var listCustomer = userCustomer.Select(x => x.CustomerId).ToList();

        var nextMonth = request.Month.HasValue ? (request.Month.Value % 12) + 1 : 1;
        var nextYear = request.Year.HasValue ? (request.Month.Value == 12 ? request.Year.Value + 1 : request.Year.Value) : DateTime.Now.Year;

        var query = _gcsCutoffCycleEntities
            .AsNoTracking()
            .Include(x => x.GcsMonthlyDetail)
            .ThenInclude(gmd => gmd.MeasurementPoint) as IQueryable<GcsCutoffCycleEntity>;

        // Get all measurement points with their data in both months
        var measurementPointsData = await query
            .Where(x =>
                ((x.Year == request.Year && x.Month == request.Month && x.PeriodNumber > 1) ||
                 (x.Year == nextYear && x.Month == nextMonth && x.PeriodNumber == 1)))
            .SelectMany(x => x.GcsMonthlyDetail)
            .GroupBy(gmd => gmd.MeasurementPointId)
            .Select(g => new
            {
                MeasurementPointId = g.Key,
                // HasCurrentMonth = g.Any(gmd => gmd.GcsCutoffCycle.Year == request.Year &&
                //                              gmd.GcsCutoffCycle.Month == request.Month &&
                //                              gmd.GcsCutoffCycle.PeriodNumber > 1),
                // HasNextMonth = g.Any(gmd => gmd.GcsCutoffCycle.Year == nextYear &&
                //                           gmd.GcsCutoffCycle.Month == nextMonth &&
                //                           gmd.GcsCutoffCycle.PeriodNumber == 1),
            })
            // .Where(x => x.HasCurrentMonth && x.HasNextMonth)
            .Select(x => x.MeasurementPointId)
            .ToListAsync(cancellationToken);

        // Get the final data for valid measurement points
        var combinedQuery = query.Where(x =>
            ((x.Year == request.Year && x.Month == request.Month && x.PeriodNumber > 1) ||
             (x.Year == nextYear && x.Month == nextMonth && x.PeriodNumber == 1)) &&
            x.GcsMonthlyDetail.Any(gmd => measurementPointsData.Contains(gmd.MeasurementPointId)));

        if (request.MeasurementPointId.HasValue)
            combinedQuery = combinedQuery.Where(x => x.GcsMonthlyDetail.Any(gmd => gmd.MeasurementPointId == request.MeasurementPointId));
        if (request.CustomerId.HasValue)
            combinedQuery = combinedQuery.Where(x => x.GcsMonthlyDetail.Any(gmd => gmd.MeasurementPoint.CustomerId == request.CustomerId));

        var baseQuery = combinedQuery.SelectMany(x => x.GcsMonthlyDetail).Where(x => x.PreviousReading != 0);

        if (user.RoleName == DefaultRoles.Customer)
            baseQuery = baseQuery.Where(gmd => listCustomer.Contains(gmd.MeasurementPoint.CustomerId));
        if (request.CustomerId != null && user.RoleName != DefaultRoles.Customer)
            baseQuery = baseQuery.Where(x => x.MeasurementPoint.CustomerId == request.CustomerId);
        if (request.MeasurementPointId.HasValue)
            baseQuery = baseQuery.Where(x => x.MeasurementPointId == request.MeasurementPointId);

        var response = await baseQuery
            .GroupBy(gmd => new
            {
                gmd.MeasurementPointId
            })
            .Select(g => new ListFinalizedOutputElectricityResponse
            {
                MeasurementPointId = g.Key.MeasurementPointId,
                CustomerCode = g.FirstOrDefault().MeasurementPoint.Customer.CustomerCode,
                CustomerName = g.FirstOrDefault().MeasurementPoint.Customer.FullName,
                MeasurementPointCode = g.FirstOrDefault().MeasurementPoint.Code,
                MeasurementPointDescription = g.FirstOrDefault().MeasurementPoint.Description,
                Bt = g.Where(gmd => gmd.MeterReadingSet == "BT").Sum(gmd => gmd.Consumption),
                Cd = g.Where(gmd => gmd.MeterReadingSet == "CD").Sum(gmd => gmd.Consumption),
                Td = g.Where(gmd => gmd.MeterReadingSet == "TD").Sum(gmd => gmd.Consumption),
                Sg = g.Where(gmd => gmd.MeterReadingSet == "SG").Sum(gmd => gmd.Consumption),
                Vc = g.Where(gmd => gmd.MeterReadingSet == "VC").Sum(gmd => gmd.Consumption),
            })
            .ToListAsync(cancellationToken);

        return response;
    }
}
