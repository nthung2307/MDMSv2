using System.Globalization;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Domain.Helpers;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class MeasurementPointMonitoringHourlyReadOnlyRepository :
    RestfulRepository<MeasurementPointHalfHourOperatingReportEntity, AppReadOnlyDbContext>,
    IMeasurementPointMonitoringHourlyReadOnlyRepository
{
    private readonly AppReadOnlyDbContext _context;
    private readonly DbSet<MeasurementPointHalfHourOperatingReportEntity> _operatingReportEntities;
    private readonly DbSet<MeasurementPointTimeSeriesEntity> _timeSeriesEntities;
    private readonly DbSet<MeasurementPointHalfHourEnergyReportEntity> _halfHourEnergyEntities;
    private readonly DbSet<MeasurementPointDailyEnergyReportEntity> _dailyEnergyEntities;
    private readonly DbSet<GcsCutoffCycleEntity> _gcsCutoffCycleEntities;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserCustomerReadWriteRepository _userCustomerReadWriteRepository;

    public MeasurementPointMonitoringHourlyReadOnlyRepository(
        AppReadOnlyDbContext applicationDbContext,
        ISnowflakeIdService snowflakeIdService,
        ICurrentUserService currentUserService,
        IUserReadOnlyRepository userReadOnlyRepository,
        IUserCustomerReadWriteRepository userCustomerReadWriteRepository) : base(applicationDbContext, snowflakeIdService)
    {
        _context = applicationDbContext;
        _operatingReportEntities = applicationDbContext.Set<MeasurementPointHalfHourOperatingReportEntity>();
        _timeSeriesEntities = applicationDbContext.Set<MeasurementPointTimeSeriesEntity>();
        _halfHourEnergyEntities = applicationDbContext.Set<MeasurementPointHalfHourEnergyReportEntity>();
        _dailyEnergyEntities = applicationDbContext.Set<MeasurementPointDailyEnergyReportEntity>();
        _gcsCutoffCycleEntities = applicationDbContext.Set<GcsCutoffCycleEntity>();
        _currentUserService = currentUserService;
        _userReadOnlyRepository = userReadOnlyRepository;
        _userCustomerReadWriteRepository = userCustomerReadWriteRepository;
    }

    public async Task<LoadChartResponse?> GetLoadChartAsync(LoadChartRequest request, CancellationToken cancellationToken)
    {
        var (currentTime, startUtc, endUtc, prevStartUtc, prevEndUtc) = ResolveChartWindows(request.Date, request.TimeSelector);
        var response = new LoadChartResponse
        {
            Date = request.TimeSelector == "Daily"
                ? currentTime
                : (request.TimeSelector == "Monthly" ? request.Date.AddHours(7) : request.Date),
            PreviousPeriodDate = request.TimeSelector switch
            {
                "Daily" => currentTime.AddDays(-1),
                "Monthly" => request.Date.AddHours(7).AddMonths(-1),
                "Yearly" => request.Date.AddYears(-1),
                _ => request.Date
            }
        };

        var configuredUnits = await OperatingReportFallbackHelper.GetConfiguredUnitsAsync(
            _context, request.MeasurementPointId, cancellationToken);
        var powerUnit = request.Type == "CongSuatTacDung" ? configuredUnits.ActivePower : configuredUnits.ReactivePower;

        var currentRows = await QueryOperatingPowerWindowAsync(request.CustomerId, request.MeasurementPointId, startUtc, endUtc, cancellationToken);
        var prevRows = await QueryOperatingPowerWindowAsync(request.CustomerId, request.MeasurementPointId, prevStartUtc, prevEndUtc, cancellationToken);

        response.CurrentPeriodData = AggregateLoadPoints(currentRows, request.TimeSelector, request.Type, currentTime.Year, currentTime.Month, powerUnit);

        var prevRef = request.TimeSelector == "Monthly" ? currentTime.AddMonths(-1) : currentTime;
        response.PreviousPeriodData = AggregateLoadPoints(prevRows, request.TimeSelector, request.Type, prevRef.Year, prevRef.Month, powerUnit);

        return response;
    }

    public async Task<VoltageChartResponse?> GetVoltageChartAsync(VoltageChartRequest request, CancellationToken cancellationToken)
    {
        var (currentTime, startUtc, endUtc, _, _) = ResolveChartWindows(request.Date, request.TimeSelector);
        var response = new VoltageChartResponse { Date = request.Date };
        var configuredUnits = await OperatingReportFallbackHelper.GetConfiguredUnitsAsync(
            _context, request.MeasurementPointId, cancellationToken);

        var rows = await _operatingReportEntities.AsNoTracking()
            .Where(x => x.MeasurementPoint.CustomerId == request.CustomerId &&
                        x.MeasurementPointId == request.MeasurementPointId &&
                        x.CreatedTime.HasValue &&
                        x.CreatedTime >= startUtc &&
                        x.CreatedTime < endUtc)
            .OrderBy(x => x.CreatedTime)
            .Select(x => new
            {
                CreatedTime = x.CreatedTime!.Value,
                x.DienApPhaA,
                x.DienApPhaB,
                x.DienApPhaC,
                x.DongDienPhaA,
                x.DongDienPhaB,
                x.DongDienPhaC
            })
            .ToListAsync(cancellationToken);

        var totalPeriods = request.TimeSelector == "Daily"
            ? 24
            : (request.TimeSelector == "Monthly"
                ? DateTime.DaysInMonth(currentTime.Year, currentTime.Month)
                : throw new ArgumentException("Invalid TimeSelector"));

        var isVoltage = request.Type == "DienAp";
        var unitA = isVoltage ? configuredUnits.VoltageA : configuredUnits.CurrentA;
        var unitB = isVoltage ? configuredUnits.VoltageB : configuredUnits.CurrentB;
        var unitC = isVoltage ? configuredUnits.VoltageC : configuredUnits.CurrentC;

        var map = new Dictionary<int, VoltageDataPoint>();
        var grouped = rows.GroupBy(r =>
        {
            var vn = r.CreatedTime.AddHours(7);
            if (request.TimeSelector == "Daily")
            {
                return vn.Hour == 0 ? 24 : vn.Hour;
            }
            return vn.Day;
        });

        foreach (var g in grouped)
        {
            double Avg(IEnumerable<double?> source)
            {
                var valid = source.Where(v => v.HasValue).Select(v => v!.Value).ToList();
                return valid.Count > 0 ? valid.Average() : 0;
            }

            map[g.Key] = new VoltageDataPoint
            {
                Label = g.Key.ToString(CultureInfo.InvariantCulture),
                PhaA = isVoltage ? Avg(g.Select(x => x.DienApPhaA)) : Avg(g.Select(x => x.DongDienPhaA)),
                PhaB = isVoltage ? Avg(g.Select(x => x.DienApPhaB)) : Avg(g.Select(x => x.DongDienPhaB)),
                PhaC = isVoltage ? Avg(g.Select(x => x.DienApPhaC)) : Avg(g.Select(x => x.DongDienPhaC)),
                PhaAUnit = unitA,
                PhaBUnit = unitB,
                PhaCUnit = unitC
            };
        }

        response.CurrentPeriodData = Enumerable.Range(1, totalPeriods).Select(period => new VoltageDataPoint
        {
            Label = period.ToString(CultureInfo.InvariantCulture),
            PhaA = map.TryGetValue(period, out var pt) ? pt.PhaA : 0,
            PhaAUnit = pt != null ? pt.PhaAUnit : (isVoltage ? unitA : string.Empty),
            PhaB = pt?.PhaB ?? 0,
            PhaBUnit = pt != null ? pt.PhaBUnit : (isVoltage ? unitB : string.Empty),
            PhaC = pt?.PhaC ?? 0,
            PhaCUnit = pt != null ? pt.PhaCUnit : (isVoltage ? unitC : string.Empty)
        }).ToList();

        return response;
    }

    public async Task<PowerChartResponse?> GetPowerChartAsync(PowerChartRequest request, CancellationToken cancellationToken)
    {
        var (currentTime, startUtc, endUtc, _, _) = ResolveChartWindows(request.Date, request.TimeSelector);
        var response = new PowerChartResponse { Date = request.Date };

        var totalPeriods = request.TimeSelector == "Daily"
            ? 24
            : (request.TimeSelector == "Monthly"
                ? DateTime.DaysInMonth(currentTime.Year, currentTime.Month)
                : throw new ArgumentException("Invalid TimeSelector"));

        // Verify ownership first cleanly
        var pointExists = await _context.Set<MeasurementPointsEntity>()
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.MeasurementPointId && p.CustomerId == request.CustomerId && !p.Deleted, cancellationToken);

        if (!pointExists)
        {
            response.CurrentPeriodData = BuildEmptyPowerChart(totalPeriods);
            return response;
        }

        var isActivePower = request.Type == "CongSuatTacDung";
        var configuredUnits = await OperatingReportFallbackHelper.GetConfiguredUnitsAsync(
            _context, request.MeasurementPointId, cancellationToken);
        var configuredUnit = isActivePower ? configuredUnits.ActivePower : configuredUnits.ReactivePower;

        var minYm = MeteringValueHelper.ComputePartitionYm(startUtc);
        var maxYm = MeteringValueHelper.ComputePartitionYm(endUtc);

        // Primary: SQL MAX/MIN over 12-month partitioned MeasurementPointTimeSeries
        var periodDataMap = new Dictionary<int, (double MaxValue, double MinValue, string Unit)>();

        try
        {
            var conn = _context.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await conn.OpenAsync(cancellationToken);
            }

            var powerColumn = isActivePower ? "CongSuatTacDungTong3Pha" : "CongSuatPhanKhangTong3Pha";
            var groupExpr = request.TimeSelector == "Daily"
                ? "HOUR(DATE_ADD(`RecordedAt`, INTERVAL 7 HOUR))"
                : "DAY(DATE_ADD(`RecordedAt`, INTERVAL 7 HOUR))";

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                SELECT {groupExpr} AS PeriodKey,
                       MAX(`{powerColumn}`) AS MaxVal,
                       MIN(`{powerColumn}`) AS MinVal
                FROM `MeasurementPointTimeSeries`
                WHERE `MeasurementPointId` = @pointId
                  AND `PartitionYm` BETWEEN @minYm AND @maxYm
                  AND `RecordedAt` >= @startUtc
                  AND `RecordedAt` < @endUtc
                  AND `{powerColumn}` IS NOT NULL
                GROUP BY {groupExpr};";

            void AddP(string name, object val)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = name;
                p.Value = val;
                cmd.Parameters.Add(p);
            }

            AddP("@pointId", request.MeasurementPointId);
            AddP("@minYm", minYm);
            AddP("@maxYm", maxYm);
            AddP("@startUtc", startUtc.UtcDateTime);
            AddP("@endUtc", endUtc.UtcDateTime);

            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                if (reader.IsDBNull(0) || reader.IsDBNull(1) || reader.IsDBNull(2)) continue;
                var rawPeriod = Convert.ToInt32(reader.GetValue(0), CultureInfo.InvariantCulture);
                var period = request.TimeSelector == "Daily" && rawPeriod == 0 ? 24 : rawPeriod;
                var maxVal = Convert.ToDouble(reader.GetValue(1), CultureInfo.InvariantCulture);
                var minVal = Convert.ToDouble(reader.GetValue(2), CultureInfo.InvariantCulture);
                periodDataMap[period] = (maxVal, minVal, configuredUnit);
            }
        }
        catch
        {
            // Fallback to half-hour operating reports below if TimeSeries table is unavailable
        }

        // Fallback (> 12 months history after partition drop): compute MAX/MIN from MeasurementPointHalfHourOperatingReports
        if (periodDataMap.Count == 0)
        {
            var opRows = await QueryOperatingPowerWindowAsync(request.CustomerId, request.MeasurementPointId, startUtc, endUtc, cancellationToken);
            var fallbackGroups = opRows.GroupBy(r =>
            {
                var vn = r.CreatedTime.AddHours(7);
                return request.TimeSelector == "Daily" ? (vn.Hour == 0 ? 24 : vn.Hour) : vn.Day;
            });

            foreach (var g in fallbackGroups)
            {
                var vals = g.Select(x => isActivePower ? x.P : x.Q).Where(v => v.HasValue).Select(v => v!.Value).ToList();
                if (vals.Count > 0)
                {
                    periodDataMap[g.Key] = (vals.Max(), vals.Min(), configuredUnit);
                }
            }
        }

        response.CurrentPeriodData = Enumerable.Range(1, totalPeriods).Select(period => new DataPointPowerChart
        {
            LabelMax = period.ToString(CultureInfo.InvariantCulture),
            LabelMin = period.ToString(CultureInfo.InvariantCulture),
            ValueMax = periodDataMap.TryGetValue(period, out var pt) ? pt.MaxValue : 0,
            ValueMin = periodDataMap.TryGetValue(period, out pt) ? pt.MinValue : 0,
            Unit = periodDataMap.TryGetValue(period, out pt) ? pt.Unit : string.Empty
        }).ToList();

        return response;
    }

    public async Task<ConsumptionOutputChartResponse?> GetConsumptionOutputChartAsync(ConsumptionOutputChartRequest request, CancellationToken cancellationToken)
    {
        var (currentTime, startUtc, endUtc, prevStartUtc, prevEndUtc) = ResolveChartWindows(request.Date, request.TimeSelector);
        var response = new ConsumptionOutputChartResponse
        {
            Date = request.Date,
            Unit = "kWh"
        };

        var pointOwned = await _context.Set<MeasurementPointsEntity>()
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.MeasurementPointId && p.CustomerId == request.CustomerId && !p.Deleted, cancellationToken);

        if (!pointOwned)
        {
            return response;
        }

        if (request.TimeSelector == "Daily")
        {
            var curRows = await _halfHourEnergyEntities.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId &&
                            m.CreatedTime.HasValue &&
                            m.CreatedTime >= startUtc.AddHours(-1) &&
                            m.CreatedTime <= endUtc.AddHours(1))
                .OrderBy(m => m.CreatedTime)
                .ToListAsync(cancellationToken);

            var prevRows = await _halfHourEnergyEntities.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId &&
                            m.CreatedTime.HasValue &&
                            m.CreatedTime >= prevStartUtc.AddHours(-1) &&
                            m.CreatedTime <= prevEndUtc.AddHours(1))
                .OrderBy(m => m.CreatedTime)
                .ToListAsync(cancellationToken);

            response.CurrentPeriodData = ComputeHourlyConsumptionFromHalfHour(curRows, currentTime.Year, currentTime.Month, currentTime.Day);
            var prevDay = currentTime.AddDays(-1);
            response.PreviousPeriodData = ToPreviousOutput(ComputeHourlyConsumptionFromHalfHour(prevRows, prevDay.Year, prevDay.Month, prevDay.Day));
        }
        else if (request.TimeSelector == "Monthly")
        {
            var prevMonth = currentTime.AddMonths(-1);
            var curRows = await _dailyEnergyEntities.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId &&
                            m.CreatedTime.HasValue &&
                            m.CreatedTime >= startUtc.AddDays(-1) &&
                            m.CreatedTime <= endUtc.AddDays(2))
                .OrderBy(m => m.CreatedTime)
                .ToListAsync(cancellationToken);

            var prevRows = await _dailyEnergyEntities.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId &&
                            m.CreatedTime.HasValue &&
                            m.CreatedTime >= prevStartUtc.AddDays(-1) &&
                            m.CreatedTime <= prevEndUtc.AddDays(2))
                .OrderBy(m => m.CreatedTime)
                .ToListAsync(cancellationToken);

            response.CurrentPeriodData = ComputeDailyConsumptionFromDaily(curRows, currentTime.Year, currentTime.Month);
            response.PreviousPeriodData = ToPreviousOutput(ComputeDailyConsumptionFromDaily(prevRows, prevMonth.Year, prevMonth.Month));
        }
        else if (request.TimeSelector == "Yearly")
        {
            var prevYear = currentTime.AddYears(-1);
            var curRows = await _dailyEnergyEntities.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId &&
                            m.CreatedTime.HasValue &&
                            m.CreatedTime >= startUtc.AddDays(-1) &&
                            m.CreatedTime <= endUtc.AddDays(2))
                .OrderBy(m => m.CreatedTime)
                .ToListAsync(cancellationToken);

            var prevRows = await _dailyEnergyEntities.AsNoTracking()
                .Where(m => m.MeasurementPointId == request.MeasurementPointId &&
                            m.CreatedTime.HasValue &&
                            m.CreatedTime >= prevStartUtc.AddDays(-1) &&
                            m.CreatedTime <= prevEndUtc.AddDays(2))
                .OrderBy(m => m.CreatedTime)
                .ToListAsync(cancellationToken);

            response.CurrentPeriodData = ComputeYearlyConsumptionFromDaily(curRows, currentTime.Year);
            response.PreviousPeriodData = ToPreviousOutput(ComputeYearlyConsumptionFromDaily(prevRows, prevYear.Year));
        }

        return response;
    }

    public async Task<FinalizedOutputElectricityChartResponse?> GetFinalizedOutputElectricityChartAsync(FinalizedOutputElectricityChartRequest request, CancellationToken cancellationToken)
    {
        var currentTime = new DateTimeOffset(request.Date.AddHours(7).Year, request.Date.AddHours(7).Month, request.Date.AddHours(7).Day, 0, 0, 0, TimeSpan.Zero);
        var response = new FinalizedOutputElectricityChartResponse();

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

        if (!dataElectricityDataPoints.Any())
        {
            response.CurrentPeriodData = new List<FinalizedOutputElectricityDataPoint>();
            response.PreviousPeriodData = new List<PreviousFinalizedOutputElectricityDataPoint>();
            response.Date = request.Date;
            response.Unit = "kWh";
            return response;
        }

        bool shouldRemoveFirstRecord = dataElectricityDataPoints.First().PeriodNumber == 1 && !hasPreviousMonthData;
        var processedData = new List<FinalizedOutputElectricityDataPoint>();

        if (dataElectricityDataPoints.Any() && !shouldRemoveFirstRecord)
        {
            var firstRecord = dataElectricityDataPoints.First();
            if (firstRecord.PeriodNumber == 1)
            {
                var previousDate = currentTime.AddMonths(-1);
                firstRecord.MonthPrevious = previousDate.Month;
                firstRecord.YearPrevious = previousDate.Year;
                if (hasPreviousMonthData && previousMonthLastPeriod != null)
                {
                    firstRecord.DayPrevious = previousMonthLastPeriod.Day;
                    firstRecord.PeriodNumberPrevious = previousMonthLastPeriod.PeriodNumber;
                }
            }
            else
            {
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

        for (int i = 1; i < dataElectricityDataPoints.Count; i++)
        {
            var current = dataElectricityDataPoints[i];
            var previous = dataElectricityDataPoints[i - 1];
            current.MonthPrevious = previous.Month ?? 0;
            current.YearPrevious = previous.Year ?? 0;
            current.DayPrevious = previous.Day ?? 0;
            current.PeriodNumberPrevious = previous.PeriodNumber ?? 0;
            processedData.Add(current);
        }

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

        var measurementPointsData = await query
            .Where(x =>
                ((x.Year == request.Year && x.Month == request.Month && x.PeriodNumber > 1) ||
                 (x.Year == nextYear && x.Month == nextMonth && x.PeriodNumber == 1)))
            .SelectMany(x => x.GcsMonthlyDetail)
            .GroupBy(gmd => gmd.MeasurementPointId)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);

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

        return await baseQuery
            .GroupBy(gmd => new { gmd.MeasurementPointId })
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
    }

    #region Private Aggregation Helpers

    private sealed record OperatingPowerRow(
        DateTimeOffset CreatedTime,
        double? P,
        double? Q);

    private static (DateTimeOffset CurrentVn, DateTimeOffset StartUtc, DateTimeOffset EndUtc, DateTimeOffset PrevStartUtc, DateTimeOffset PrevEndUtc) ResolveChartWindows(
        DateTimeOffset requestDate,
        string timeSelector)
    {
        var vnDate = requestDate.ToOffset(TimeSpan.FromHours(7));
        var vnMidnight = new DateTimeOffset(vnDate.Year, vnDate.Month, vnDate.Day, 0, 0, 0, TimeSpan.FromHours(7));

        if (timeSelector == "Daily")
        {
            var end = vnMidnight.AddDays(1);
            var prevStart = vnMidnight.AddDays(-1);
            return (new DateTimeOffset(vnDate.Year, vnDate.Month, vnDate.Day, 0, 0, 0, TimeSpan.Zero),
                vnMidnight.UtcDateTime, end.UtcDateTime, prevStart.UtcDateTime, vnMidnight.UtcDateTime);
        }

        if (timeSelector == "Monthly")
        {
            var start = new DateTimeOffset(vnDate.Year, vnDate.Month, 1, 0, 0, 0, TimeSpan.FromHours(7));
            var end = start.AddMonths(1);
            var prevStart = start.AddMonths(-1);
            return (new DateTimeOffset(vnDate.Year, vnDate.Month, vnDate.Day, 0, 0, 0, TimeSpan.Zero),
                start.UtcDateTime, end.UtcDateTime, prevStart.UtcDateTime, start.UtcDateTime);
        }

        var yStart = new DateTimeOffset(vnDate.Year, 1, 1, 0, 0, 0, TimeSpan.FromHours(7));
        var yEnd = yStart.AddYears(1);
        var prevYStart = yStart.AddYears(-1);
        return (new DateTimeOffset(vnDate.Year, vnDate.Month, vnDate.Day, 0, 0, 0, TimeSpan.Zero),
            yStart.UtcDateTime, yEnd.UtcDateTime, prevYStart.UtcDateTime, yStart.UtcDateTime);
    }

    private async Task<List<OperatingPowerRow>> QueryOperatingPowerWindowAsync(
        long customerId,
        long measurementPointId,
        DateTimeOffset startUtc,
        DateTimeOffset endUtc,
        CancellationToken cancellationToken)
    {
        return await _operatingReportEntities.AsNoTracking()
            .Where(x => x.MeasurementPoint.CustomerId == customerId &&
                        x.MeasurementPointId == measurementPointId &&
                        x.CreatedTime.HasValue &&
                        x.CreatedTime >= startUtc &&
                        x.CreatedTime < endUtc)
            .OrderBy(x => x.CreatedTime)
            .Select(x => new OperatingPowerRow(
                x.CreatedTime!.Value,
                x.CongSuatTacDungTong3Pha,
                x.CongSuatPhanKhangTong3Pha))
            .ToListAsync(cancellationToken);
    }

    private static List<DataPoint> AggregateLoadPoints(
        List<OperatingPowerRow> rows,
        string timeSelector,
        string type,
        int year,
        int month,
        string configuredUnit)
    {
        var totalPeriods = timeSelector switch
        {
            "Daily" => 24,
            "Monthly" => DateTime.DaysInMonth(year, month),
            "Yearly" => 12,
            _ => throw new ArgumentException("Invalid TimeSelector")
        };

        var isActive = type == "CongSuatTacDung";
        var map = new Dictionary<int, (double Value, string Unit)>();

        var grouped = rows.GroupBy(r =>
        {
            var vn = r.CreatedTime.AddHours(7);
            return timeSelector switch
            {
                "Daily" => vn.Hour == 0 ? 24 : vn.Hour,
                "Monthly" => vn.Day,
                _ => vn.Month
            };
        });

        foreach (var g in grouped)
        {
            var vals = g.Select(x => isActive ? x.P : x.Q).Where(v => v.HasValue).Select(v => v!.Value).ToList();
            if (vals.Count > 0)
            {
                map[g.Key] = (vals.Average(), configuredUnit);
            }
        }

        return Enumerable.Range(1, totalPeriods).Select(period => new DataPoint
        {
            Label = period.ToString(CultureInfo.InvariantCulture),
            Value = map.TryGetValue(period, out var pt) ? pt.Value : 0,
            Unit = map.TryGetValue(period, out pt) ? pt.Unit : string.Empty
        }).ToList();
    }

    private static List<DataPointPowerChart> BuildEmptyPowerChart(int totalPeriods) =>
        Enumerable.Range(1, totalPeriods).Select(p => new DataPointPowerChart
        {
            LabelMax = p.ToString(CultureInfo.InvariantCulture),
            LabelMin = p.ToString(CultureInfo.InvariantCulture),
            ValueMax = 0,
            ValueMin = 0,
            Unit = string.Empty
        }).ToList();

    private static double? ComputeSafeDiff(decimal? curr, decimal? prev)
    {
        if (!curr.HasValue || !prev.HasValue) return null;
        var diff = curr.Value < prev.Value ? curr.Value : curr.Value - prev.Value;
        return Math.Round((double)diff, 2);
    }

    private static List<ConsumptionOutputDataPoint> ComputeHourlyConsumptionFromHalfHour(
        List<MeasurementPointHalfHourEnergyReportEntity> records,
        int year,
        int month,
        int day)
    {
        var dayStartVn = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.FromHours(7));

        MeasurementPointHalfHourEnergyReportEntity? FindBoundary(DateTimeOffset targetVn)
        {
            return records
                .Where(r => r.CreatedTime.HasValue && Math.Abs((r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7)) - targetVn).TotalMinutes) <= 20)
                .OrderBy(r => Math.Abs((r.CreatedTime!.Value.ToOffset(TimeSpan.FromHours(7)) - targetVn).TotalMinutes))
                .FirstOrDefault();
        }

        var result = new List<ConsumptionOutputDataPoint>(24);
        for (var hour = 1; hour <= 24; hour++)
        {
            var slotStart = dayStartVn.AddHours(hour - 1);
            var slotEnd = dayStartVn.AddHours(hour);
            var prev = FindBoundary(slotStart);
            var curr = FindBoundary(slotEnd);

            result.Add(new ConsumptionOutputDataPoint
            {
                Label = hour.ToString(CultureInfo.InvariantCulture),
                Bt = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao),
                Cd = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao),
                Td = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao),
                Sg = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPTongKenhGiao),
                Vc = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, prev?.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao)
            });
        }

        return result;
    }

    private static List<ConsumptionOutputDataPoint> ComputeDailyConsumptionFromDaily(
        List<MeasurementPointDailyEnergyReportEntity> records,
        int year,
        int month)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var byDay = records
            .Where(r => r.CreatedTime.HasValue)
            .GroupBy(r => r.CreatedTime!.Value.ToOffset(TimeSpan.FromHours(7)).Date)
            .ToDictionary(g => g.Key, g => g.OrderBy(r => r.CreatedTime).First());

        var result = new List<ConsumptionOutputDataPoint>(daysInMonth);
        for (var d = 1; d <= daysInMonth; d++)
        {
            var dayStart = new DateTime(year, month, d);
            var dayEnd = dayStart.AddDays(1);
            byDay.TryGetValue(dayStart, out var prev);
            byDay.TryGetValue(dayEnd, out var curr);

            result.Add(new ConsumptionOutputDataPoint
            {
                Label = d.ToString(CultureInfo.InvariantCulture),
                Bt = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao),
                Cd = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao),
                Td = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao),
                Sg = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPTongKenhGiao),
                Vc = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, prev?.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao)
            });
        }

        return result;
    }

    private static List<ConsumptionOutputDataPoint> ComputeYearlyConsumptionFromDaily(
        List<MeasurementPointDailyEnergyReportEntity> records,
        int year)
    {
        MeasurementPointDailyEnergyReportEntity? FindBoundary(DateTime targetDate)
        {
            var exact = records.FirstOrDefault(r => r.CreatedTime.HasValue && r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7)).Date == targetDate.Date);
            if (exact != null) return exact;

            return records
                .Where(r => r.CreatedTime.HasValue && r.CreatedTime.Value.ToOffset(TimeSpan.FromHours(7)).Date <= targetDate.Date)
                .OrderByDescending(r => r.CreatedTime)
                .FirstOrDefault();
        }

        var result = new List<ConsumptionOutputDataPoint>(12);
        for (var m = 1; m <= 12; m++)
        {
            var startDate = new DateTime(year, m, 1);
            var endDate = startDate.AddMonths(1);
            var prev = FindBoundary(startDate);
            var curr = FindBoundary(endDate);

            result.Add(new ConsumptionOutputDataPoint
            {
                Label = m.ToString(CultureInfo.InvariantCulture),
                Bt = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPBieuGioBTKenhGiao),
                Cd = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPBieuGioCDKenhGiao),
                Td = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPBieuGioTDKenhGiao),
                Sg = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangTacDungAPTongKenhGiao, prev?.ChiSoTichLuyDienNangTacDungAPTongKenhGiao),
                Vc = ComputeSafeDiff(curr?.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao, prev?.ChiSoTichLuyDienNangPhanKhangAQTongKenhGiao)
            });
        }

        return result;
    }

    private static List<PreviousConsumptionOutputDataPoint> ToPreviousOutput(List<ConsumptionOutputDataPoint> items) =>
        items.Select(x => new PreviousConsumptionOutputDataPoint
        {
            Label = x.Label,
            PreviousBt = x.Bt,
            PreviousCd = x.Cd,
            PreviousTd = x.Td,
            PreviousSg = x.Sg,
            PreviousVc = x.Vc
        }).ToList();

    #endregion
}
