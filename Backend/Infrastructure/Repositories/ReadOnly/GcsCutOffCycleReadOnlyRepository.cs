using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.GcsCutOffCycle;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.GcsCutOffCycle;
using Application.DataTransferObjects.Responses.IoT;
using Application.Extensions;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Database.Repository;
using Nobisoft.Core.Interfaces;

namespace Infrastructure.Repositories.ReadOnly;

public class GcsCutOffCycleReadOnlyRepository: RestfulRepository<GcsCutoffCycleEntity, AppReadOnlyDbContext>, IGcsCutOffCycleReadOnlyRepository
{
    private readonly DbSet<GcsCutoffCycleEntity> _gcsCutoffCycleEntities;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IGcsTemporaryReadWriteRepository _gcsTemporaryReadWriteRepository;
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;
    private readonly IGcsMonthlyDetailReadOnlyRepository _gcsMonthlyDetailReadOnlyRepository;
    private readonly IMapper _mapper;
    private readonly IGcsReadOnlyRepository _gcsReadOnlyRepository;
    private readonly IConfiguration _configuration;

    public GcsCutOffCycleReadOnlyRepository(AppReadOnlyDbContext applicationDbContext, ISnowflakeIdService snowflakeIdService, IGcsTemporaryReadWriteRepository gcsTemporaryReadWriteRepository, IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository, IGcsMonthlyDetailReadOnlyRepository gcsMonthlyDetailReadOnlyRepository, IMapper mapper, IGcsReadOnlyRepository gcsReadOnlyRepository, IConfiguration configuration) : base(applicationDbContext, snowflakeIdService)
    {
        _snowflakeIdService = snowflakeIdService;
        _gcsTemporaryReadWriteRepository = gcsTemporaryReadWriteRepository;
        _measurementPointsReadOnlyRepository = measurementPointsReadOnlyRepository;
        _gcsMonthlyDetailReadOnlyRepository = gcsMonthlyDetailReadOnlyRepository;
        _mapper = mapper;
        _gcsReadOnlyRepository = gcsReadOnlyRepository;
        _configuration = configuration;
        _gcsCutoffCycleEntities = applicationDbContext.Set<GcsCutoffCycleEntity>();
    }

    public async Task<bool> GetExistGcsCutOff(long gcsId, int month, int year, int periodNumber, CancellationToken cancellationToken)
    {
        return await _gcsCutoffCycleEntities
            .AsNoTracking()
            .Include(x => x.GcsMonthlyDetail)
            .AnyAsync(x => x.GcsId == gcsId && x.Month == month && x.Year == year && x.PeriodNumber == periodNumber, cancellationToken);
    }

    public async Task<CursorPaginationResponse<GcsCutOffCycleResponse>> GetListGcsCutOffCyclesAsync(GetListGcsCutOffCycleRequest request, CancellationToken cancellationToken)
    {
        var gcs = _gcsCutoffCycleEntities.AsNoTracking();
        if (request.Month != 0)
            gcs = gcs.Where(x => x.Month == request.Month);
        if(request.Year != 0)
            gcs = gcs.Where(x => 
                (x.Year == request.Year && !(x.PeriodNumber == 1 && x.Month == 1)) || 
                (x.Year == request.Year + 1 && x.PeriodNumber == 1 && x.Month == 1)
            );


        var result = await gcs
            .Where(x => x.GcsId == request.GcsId)
            .PaginateWithAndAsync<GcsCutoffCycleEntity, GcsCutOffCycleResponse>(request, _mapper, cancellationToken);

        // For each cycle in the result, fetch and attach the previous cycle info
        foreach (var cycle in result.Data)
        {
            var previousCycle = await GetPreviousGcsCutOffCycle(
                request.GcsId,
                cycle.Year,
                cycle.Month,
                cycle.PeriodNumber,
                cancellationToken
            );

            if (previousCycle != null)
            {
                cycle.PreviousDay = previousCycle.Day;
                cycle.PreviousMonth = previousCycle.Month;
                cycle.PreviousYear = previousCycle.Year;
                cycle.PreviousPeriodNumber = previousCycle.PeriodNumber;
            }
            else
            {
                var firstDate = new DateTimeOffset(
                    cycle.Year,
                    cycle.Month,
                    cycle.Day,
                    0, 0, 0,
                    TimeSpan.Zero
                );
                var prevDate = firstDate.AddMonths(-1);
                var prevYear = prevDate.Year;
                var prevMonth = prevDate.Month;
                var lastDayOfPrevMonth = DateTime.DaysInMonth(prevYear, prevMonth);
                cycle.PreviousDay = cycle.PreviousDay != 0 ? cycle.PreviousDay : cycle.Gcs.DayPeriod.Last() == 999 ? lastDayOfPrevMonth : Math.Min(cycle.Gcs.DayPeriod.Last(), lastDayOfPrevMonth);
                cycle.PreviousMonth = prevMonth;
                cycle.PreviousYear = prevYear;
                cycle.PreviousPeriodNumber = cycle.Gcs.PeriodNumber;
            }
        }

        return new CursorPaginationResponse<GcsCutOffCycleResponse>()
        {
            Data = result.Data,
            HasNext = result.HasNext,
            HasPrevious = result.HasPrevious,
            TotalCount = result.TotalCount,
        };
    }

    public async Task<List<int>> GetListYearGcsCutOffCyclesAsync(long gcsId, CancellationToken cancellationToken)
    {
        return await _gcsCutoffCycleEntities.AsNoTracking()
            .Where(x => x.GcsId == gcsId)
            .Select(x => x.Year)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<GcsCutoffCycleEntity> GetPreviousGcsCutOffCycle(
        long gcsId,
        int currentYear,
        int currentMonth,
        int currentPeriodNumber,
        CancellationToken cancellationToken)
    {
        // First try to get previous period in the same month
        if (currentPeriodNumber > 1)
        {
            return await _gcsCutoffCycleEntities
                .Where(x => x.GcsId == gcsId
                    && x.Year == currentYear
                    && x.Month == currentMonth
                    && x.PeriodNumber == currentPeriodNumber - 1)
                .FirstOrDefaultAsync(cancellationToken);
        }

        // If it was the first period, get the last period of the previous month
        var previousMonth = currentMonth - 1;
        var previousYear = currentYear;

        if (previousMonth == 0)
        {
            previousMonth = 12;
            previousYear--;
        }

        return await _gcsCutoffCycleEntities
            .Where(x => x.GcsId == gcsId
                && x.Year == previousYear
                && x.Month == previousMonth)
            .OrderByDescending(x => x.PeriodNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<RequestResult<CursorPaginationResponse<GetDataLatchResponse>>> GetDataLatchResponseAsync(GetDataLatchRequest request, CancellationToken cancellationToken)
    {
        var listCurrentReading = new List<GcsCurrentReadingDto>();
        var existGcs = await _gcsReadOnlyRepository.IsExistGcsAsync(request.GcsId, cancellationToken);
        if (existGcs == null)
        {
            return RequestResult<CursorPaginationResponse<GetDataLatchResponse>>.Fail(new[]
            {
                new ErrorItem()
                {
                    FieldName = LocalizationString.Common.UnknownFieldName,
                    Error = "Không tìm thấy sổ ghi chốt."
                }
            });
        }

        var gcsCutOffData = await GetExistGcsCutOff(request.GcsId, request.Month, request.Year, request.PeriodNumber, cancellationToken);
        if (gcsCutOffData)
        {
            return RequestResult<CursorPaginationResponse<GetDataLatchResponse>>.Fail(new[]
            {
                new ErrorItem()
                {
                    FieldName = LocalizationString.Common.UnknownFieldName,
                    Error = "Sổ GCS đã chốt."
                }
            });
        }

        var lastGcsCutOffCycle = await _gcsCutoffCycleEntities.AsNoTracking()
            .Where(x => x.GcsId == request.GcsId)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ThenByDescending(x => x.PeriodNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastGcsCutOffCycle != null)
        {
            var isValidSequence = IsValidCycleSequence(
                lastGcsCutOffCycle.Year,
                lastGcsCutOffCycle.Month,
                lastGcsCutOffCycle.PeriodNumber,
                request.Year,
                request.Month,
                request.PeriodNumber,
                existGcs.PeriodNumber,
                existGcs.IsNewChange
            );

            if (!isValidSequence)
            {
                return RequestResult<CursorPaginationResponse<GetDataLatchResponse>>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = "Sequence",
                        Error = "Vui lòng chốt kỳ theo đúng trình tự"
                    }
                });
            }
        }

        if (lastGcsCutOffCycle == null)
        {
            if (request.PeriodNumber < 1 || request.PeriodNumber > existGcs.PeriodNumber)
            {
                return RequestResult<CursorPaginationResponse<GetDataLatchResponse>>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = "Sequence",
                        Error = $"Kỳ chốt không hợp lệ. Vui lòng chọn kỳ từ 1 đến {existGcs.PeriodNumber}."
                    }
                });
            }
            // Đối với lần chốt đầu tiên của sổ mới hoặc hệ thống mới triển khai:
            // Cho phép chọn tháng/năm vận hành hiện tại (hoặc tháng/năm do người dùng chỉ định, ví dụ Tháng 10/2026) làm mốc chốt chỉ số đầu tiên.
        }

        if (existGcs.MeasurementPoints.All(x => x.Status != EntityStatus.Active))
        {
            return RequestResult<CursorPaginationResponse<GetDataLatchResponse>>.Fail(new[]
            {
                new ErrorItem()
                {
                    FieldName = LocalizationString.Common.UnknownFieldName,
                    Error = "Sổ không có điểm đo nào hoạt động."
                }
            });
        }

        if (request.PeriodNumber > existGcs.PeriodNumber || request.PeriodNumber < 1)
        {
            return RequestResult<CursorPaginationResponse<GetDataLatchResponse>>.Fail(new[]
            {
                new ErrorItem()
                {
                    FieldName = LocalizationString.Common.UnknownFieldName,
                    Error = "Hiện tại không có kì ghi chốt này."
                }
            });
        }
        var lastDayOfMonth = DateTime.DaysInMonth(request.Year, request.Month);
        int dayToUse = lastDayOfMonth;
        if (existGcs.DayPeriod != null && existGcs.DayPeriod.Count >= request.PeriodNumber)
        {
            var pDay = existGcs.DayPeriod[request.PeriodNumber - 1];
            dayToUse = pDay == 999 ? lastDayOfMonth : pDay;
        }
        if (dayToUse > lastDayOfMonth) dayToUse = lastDayOfMonth;
        if (dayToUse < 1) dayToUse = 1;

        var datetime = new DateTime(request.Year, request.Month, dayToUse);

        // Calculate next latch date for cycle boundary validation
        int nextPeriodNumber = request.PeriodNumber + 1;
        int nextMonth = request.Month;
        int nextYear = request.Year;
        if (nextPeriodNumber > existGcs.PeriodNumber)
        {
            nextPeriodNumber = 1;
            if (nextMonth == 12)
            {
                nextMonth = 1;
                nextYear++;
            }
            else
            {
                nextMonth++;
            }
        }
        int nextLastDay = DateTime.DaysInMonth(nextYear, nextMonth);
        int nextDayToUse = nextLastDay;
        if (existGcs.DayPeriod != null && existGcs.DayPeriod.Count >= nextPeriodNumber)
        {
            var pDay = existGcs.DayPeriod[nextPeriodNumber - 1];
            nextDayToUse = pDay == 999 ? nextLastDay : pDay;
        }
        if (nextDayToUse > nextLastDay) nextDayToUse = nextLastDay;
        if (nextDayToUse < 1) nextDayToUse = 1;
        var nextLatchDate = new DateTime(nextYear, nextMonth, nextDayToUse, 23, 59, 59);

        if (lastGcsCutOffCycle != null && datetime > DateTime.Now)
        {
            return RequestResult<CursorPaginationResponse<GetDataLatchResponse>>.Fail(new[]
            {
                new ErrorItem()
                {
                    FieldName = LocalizationString.Common.UnknownFieldName,
                    Error = "Chưa đến thời gian chốt của sổ."
                }
            });
        }

        var selectedDayPeriod = dayToUse;

        try
        {
            await _gcsTemporaryReadWriteRepository.EnsureColumnsExistAsync(cancellationToken);
        }
        catch { }

        var gcs = await _gcsTemporaryReadWriteRepository.GetListGcsTemporaryAsync(request, cancellationToken);
        if (gcs.Count == 0 || request.IsRefresh)
        {
            var gcsWithMeasurementPoints = await _measurementPointsReadOnlyRepository.GetListMeasurementPointGcsAsync(request.GcsId, cancellationToken);
            var validMeasurementPointIds = gcsWithMeasurementPoints.Select(mp => mp.Id).ToHashSet();
            var temporaryRecords = await _gcsTemporaryReadWriteRepository.GetListGcsTemporaryWithGcsAsync(request.GcsId, request.Month, request.Year, request.PeriodNumber, cancellationToken);
            var recordsToRemove = temporaryRecords.Where(temp => !validMeasurementPointIds.Contains(temp.MeasurementPointId)).ToList();

            foreach (var record in recordsToRemove)
            {
                await _gcsTemporaryReadWriteRepository.RemoveAsync(record, cancellationToken);
            }
            var oldGcsPointsGrouped = gcsWithMeasurementPoints
                .Where(x => x.IsNewChange)
                .GroupBy(x => x.OldGcsId)
                .Select(group => new
                {
                    OldGcsId = group.Key,
                    MeasurementPoints = group.Select(x => new
                    {
                        x.Id
                    }).ToList()
                }).ToList();

            foreach (var item in oldGcsPointsGrouped)
            {
                var listGcsCurrentReading = await _gcsReadOnlyRepository.GetGcsCurrentReadingAsync(item.OldGcsId ?? 0, item.MeasurementPoints.Select(x => x.Id).ToList(), cancellationToken);
                listCurrentReading.AddRange(listGcsCurrentReading);
            }

            var maxCycle = await GetGcsCutOffCycleAsync(request.GcsId, cancellationToken);

            var monthlyDetailsWithPoints = new List<GetDataLatchResponse>();
            if (maxCycle != null)
                monthlyDetailsWithPoints = await _gcsMonthlyDetailReadOnlyRepository.GetGcsMonthlyDetailAsync(maxCycle.Month, maxCycle.Year, maxCycle.Id, cancellationToken);

            var relevantDescriptions = new List<string>
            {
                "CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT",
                "CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD",
                "CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD",
                "CHI_SO_CHOT_DIEN_NANG_AP_TONG",
                "CHI_SO_CHOT_DIEN_NANG_AQ_TONG",
                "PMAX_KY_TRUOC",
            };

            // Query số liệu chỉ số chốt chiều giao do Kepware IoT REST API Client gửi đến (lưu trong MeasurementPointMonitoringEntity)
            var latestMonitoringMap = new Dictionary<(long, string), MeasurementPointMonitoringEntity>();
            var latestCutoffTimeMap = new Dictionary<long, DateTimeOffset>();
            var latestPmaxTimeMap = new Dictionary<long, DateTimeOffset>();
            try
            {
                var measurementPointIds = gcsWithMeasurementPoints.Select(x => x.Id).ToList();
                if (measurementPointIds.Count > 0)
                {
                    var dbMonitoringRecords = await DbContext.Set<MeasurementPointMonitoringEntity>()
                        .AsNoTracking()
                        .Where(x => measurementPointIds.Contains(x.MeasurementPointsId) && x.TagNameDescription != null && relevantDescriptions.Contains(x.TagNameDescription))
                        .OrderByDescending(x => x.CreatedTime)
                        .ToListAsync(cancellationToken);

                    latestMonitoringMap = dbMonitoringRecords
                        .GroupBy(x => (x.MeasurementPointsId, x.TagNameDescription!))
                        .ToDictionary(g => g.Key, g => g.First());

                    // Query thời điểm chốt gần nhất và thời điểm Pmax kỳ trước từ MeasurementPointMonitoringByDateTimeEntity
                    var dateTimeTags = new List<string>
                    {
                        ListDataMeasurementPoint.NamChotGanNhat,
                        ListDataMeasurementPoint.ThangChotGanNhat,
                        ListDataMeasurementPoint.NgayChotGanNhat,
                        ListDataMeasurementPoint.GioChotGanNhat,
                        ListDataMeasurementPoint.PhutChotGanNhat,
                        ListDataMeasurementPoint.GiayChotGanNhat,
                        ListDataMeasurementPoint.NamPmaxKyTruoc,
                        ListDataMeasurementPoint.ThangPmaxKyTruoc,
                        ListDataMeasurementPoint.NgayPmaxKyTruoc,
                        ListDataMeasurementPoint.GioPmaxKyTruoc,
                        ListDataMeasurementPoint.PhutPmaxKyTruoc,
                        ListDataMeasurementPoint.GiayPmaxKyTruoc
                    };

                    var dbDateTimeRecords = await DbContext.Set<MeasurementPointMonitoringByDateTimeEntity>()
                        .AsNoTracking()
                        .Where(x => measurementPointIds.Contains(x.MeasurementPointId) && x.TagNameDescription != null && dateTimeTags.Contains(x.TagNameDescription))
                        .OrderByDescending(x => x.CreatedTime)
                        .ToListAsync(cancellationToken);

                    var groupedDateByPoint = dbDateTimeRecords
                        .GroupBy(x => x.MeasurementPointId)
                        .ToDictionary(g => g.Key, g => g.GroupBy(d => d.TagNameDescription!).ToDictionary(sub => sub.Key, sub => sub.First().Value));

                    foreach (var kvp in groupedDateByPoint)
                    {
                        var ptId = kvp.Key;
                        var tags = kvp.Value;
                        try
                        {
                            if (tags.TryGetValue(ListDataMeasurementPoint.NamChotGanNhat, out var sYear) &&
                                tags.TryGetValue(ListDataMeasurementPoint.ThangChotGanNhat, out var sMonth) &&
                                tags.TryGetValue(ListDataMeasurementPoint.NgayChotGanNhat, out var sDay) &&
                                int.TryParse(sYear, out var yr) && int.TryParse(sMonth, out var mo) && int.TryParse(sDay, out var da) &&
                                yr >= 2000 && yr <= 2100 && mo >= 1 && mo <= 12 && da >= 1 && da <= 31)
                            {
                                int hr = 0, mi = 0, se = 0;
                                if (tags.TryGetValue(ListDataMeasurementPoint.GioChotGanNhat, out var sHour) && int.TryParse(sHour, out var parsedHr)) hr = parsedHr;
                                if (tags.TryGetValue(ListDataMeasurementPoint.PhutChotGanNhat, out var sMin) && int.TryParse(sMin, out var parsedMi)) mi = parsedMi;
                                if (tags.TryGetValue(ListDataMeasurementPoint.GiayChotGanNhat, out var sSec) && int.TryParse(sSec, out var parsedSe)) se = parsedSe;

                                if (hr < 0 || hr > 23) hr = 0;
                                if (mi < 0 || mi > 59) mi = 0;
                                if (se < 0 || se > 59) se = 0;

                                var cutoffDt = new DateTimeOffset(yr, mo, Math.Min(da, DateTime.DaysInMonth(yr, mo)), hr, mi, se, TimeSpan.FromHours(7));
                                latestCutoffTimeMap[ptId] = cutoffDt;
                            }
                        }
                        catch
                        {
                            // Bỏ qua lỗi parse ngày nếu format bất thường
                        }

                        try
                        {
                            if (tags.TryGetValue(ListDataMeasurementPoint.NamPmaxKyTruoc, out var pYear) &&
                                tags.TryGetValue(ListDataMeasurementPoint.ThangPmaxKyTruoc, out var pMonth) &&
                                tags.TryGetValue(ListDataMeasurementPoint.NgayPmaxKyTruoc, out var pDay) &&
                                int.TryParse(pYear, out var pyr) && int.TryParse(pMonth, out var pmo) && int.TryParse(pDay, out var pda) &&
                                pyr >= 2000 && pyr <= 2100 && pmo >= 1 && pmo <= 12 && pda >= 1 && pda <= 31)
                            {
                                int phr = 0, pmi = 0, pse = 0;
                                if (tags.TryGetValue(ListDataMeasurementPoint.GioPmaxKyTruoc, out var pHour) && int.TryParse(pHour, out var parsedPhr)) phr = parsedPhr;
                                if (tags.TryGetValue(ListDataMeasurementPoint.PhutPmaxKyTruoc, out var pMin) && int.TryParse(pMin, out var parsedPmi)) pmi = parsedPmi;
                                if (tags.TryGetValue(ListDataMeasurementPoint.GiayPmaxKyTruoc, out var pSec) && int.TryParse(pSec, out var parsedPse)) pse = parsedPse;

                                if (phr < 0 || phr > 23) phr = 0;
                                if (pmi < 0 || pmi > 59) pmi = 0;
                                if (pse < 0 || pse > 59) pse = 0;

                                var pmaxDt = new DateTimeOffset(pyr, pmo, Math.Min(pda, DateTime.DaysInMonth(pyr, pmo)), phr, pmi, pse, TimeSpan.FromHours(7));
                                latestPmaxTimeMap[ptId] = pmaxDt;
                            }
                        }
                        catch
                        {
                            // Bỏ qua lỗi parse thời điểm Pmax
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GcsCutOff] dbMonitoringRecords query error: {ex.Message}");
            }

            var meterReadingSetMapping = new Dictionary<string, string>
            {
                { "CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT", "BT" },
                { "CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD", "CD" },
                { "CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD", "TD" },
                { "CHI_SO_CHOT_DIEN_NANG_AP_TONG", "SG" },
                { "CHI_SO_CHOT_DIEN_NANG_AQ_TONG", "VC" },
                { "PMAX_KY_TRUOC", "Pmax"},
            };

            foreach (var measurementPoint in gcsWithMeasurementPoints)
            {
                var pointEntitiesToUpsert = new List<(GcsTemporaryEntity Entity, bool IsUpdate)>();

                foreach (var readingType in meterReadingSetMapping)
                {
                    var isPmax = readingType.Key == "PMAX_KY_TRUOC";
                    double? currentVal = null;
                    DateTimeOffset? readingTime = null;
                    bool? quality = null;

                    // Lấy số liệu từ bộ chỉ số chốt chiều giao do Kepware IoT Client gửi đến (MeasurementPointMonitoringEntity)
                    if (latestMonitoringMap.TryGetValue((measurementPoint.Id, readingType.Key), out var dbRec) && dbRec != null)
                    {
                        if (!string.IsNullOrWhiteSpace(dbRec.Value) && double.TryParse(dbRec.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedVal))
                        {
                            currentVal = parsedVal;
                            quality = dbRec.Quanlity ?? true;
                            if (latestCutoffTimeMap.TryGetValue(measurementPoint.Id, out var parsedCutoffTime))
                            {
                                readingTime = parsedCutoffTime;
                            }
                            else
                            {
                                readingTime = dbRec.CreatedTime;
                            }
                        }
                    }

                    // Resolve previous cycle reading and period info
                    var existingDetail = monthlyDetailsWithPoints.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && x.MeterReadingSet == readingType.Value);
                    double? previousReading = null;
                    int? prevDay = null;
                    int? prevMonth = null;
                    int? prevYear = null;
                    int? prevPeriod = null;

                    if (existingDetail != null)
                    {
                        previousReading = existingDetail.NewCurrentReading ?? existingDetail.CurrentReading;
                        prevDay = maxCycle?.Day ?? (existGcs.DayPeriod.Last() == 999 ? DateTime.DaysInMonth(DateTimeOffset.Now.AddMonths(-1).Year, DateTimeOffset.Now.AddMonths(-1).Month) : existGcs.DayPeriod.Last());
                        prevMonth = maxCycle?.Month ?? DateTimeOffset.Now.AddMonths(-1).Month;
                        prevYear = maxCycle?.Year ?? DateTimeOffset.Now.AddMonths(-1).Year;
                        prevPeriod = maxCycle?.PeriodNumber ?? existGcs.PeriodNumber;
                    }
                    else if (measurementPoint.IsNewChange)
                    {
                        var oldGcsReading = listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet);
                        previousReading = oldGcsReading?.CurrentReading ?? 0;
                        prevDay = oldGcsReading?.CurrentDay;
                        prevMonth = oldGcsReading?.CurrentMonth;
                        prevYear = oldGcsReading?.CurrentYear;
                        prevPeriod = oldGcsReading?.CurrentPeriodNumber;
                    }
                    else
                    {
                        previousReading = 0;
                        prevDay = maxCycle?.Day;
                        prevMonth = maxCycle?.Month;
                        prevYear = maxCycle?.Year;
                        prevPeriod = maxCycle?.PeriodNumber;
                    }

                    // Calculate final current reading and consumption
                    double? finalCurrentReading = currentVal.HasValue
                        ? (isPmax ? Math.Round(currentVal.Value, 3, MidpointRounding.AwayFromZero) : currentVal.Value)
                        : null;

                    double? consumption = null;
                    if (!isPmax && finalCurrentReading.HasValue)
                    {
                        consumption = (finalCurrentReading.Value - (previousReading ?? 0)) * measurementPoint.MultiplierFactor;
                    }

                    // Anomaly & Suspicious reading detection
                    bool isSuspicious = false;
                    string? suspiciousReason = null;

                    if (!finalCurrentReading.HasValue)
                    {
                        isSuspicious = true;
                        suspiciousReason = "Chưa có chỉ số chốt từ công tơ (chưa từng được cập nhật)";
                    }
                    else
                    {
                        if (quality == false)
                        {
                            isSuspicious = true;
                            suspiciousReason = "Tín hiệu đo đếm chất lượng kém hoặc mất kết nối";
                        }
                        else if (readingTime.HasValue && (readingTime.Value.Date < datetime.Date.AddDays(-2) || readingTime.Value > nextLatchDate))
                        {
                            isSuspicious = true;
                            suspiciousReason = $"Thời điểm chốt ({readingTime.Value:dd/MM/yyyy HH:mm}) nằm ngoài kỳ chốt ({datetime:dd/MM/yyyy} - {nextLatchDate:dd/MM/yyyy})";
                        }
                        else if (existingDetail?.ReadingTime != null && existingDetail.ReadingTime != DateTimeOffset.MinValue && readingTime.HasValue && readingTime.Value == existingDetail.ReadingTime)
                        {
                            isSuspicious = true;
                            suspiciousReason = "Thời điểm chốt chưa thay đổi so với kỳ trước (chưa có chỉ số mới)";
                        }
                        else if (!isPmax && consumption.HasValue)
                        {
                            if (consumption.Value < 0)
                            {
                                isSuspicious = true;
                                suspiciousReason = $"Sản lượng âm ({consumption.Value:N2}) - chỉ số mới nhỏ hơn chỉ số cũ";
                            }
                            else if (consumption.Value == 0 && (readingType.Value == "BT" || readingType.Value == "SG") && (existingDetail?.Consumption ?? 0) > 100)
                            {
                                isSuspicious = true;
                                suspiciousReason = $"Sản lượng bằng 0 bất thường (kỳ trước tiêu thụ {existingDetail?.Consumption:N2} kWh)";
                            }
                            else if (existingDetail?.Consumption != null && existingDetail.Consumption.Value >= 100)
                            {
                                var prevCons = existingDetail.Consumption.Value;
                                var ratio = consumption.Value / prevCons;
                                if (ratio < 0.2)
                                {
                                    var percent = ((consumption.Value - prevCons) / prevCons) * 100.0;
                                    isSuspicious = true;
                                    suspiciousReason = $"Sản lượng giảm đột biến ({percent:0.0}% so với kỳ trước: {consumption.Value:N2} vs {prevCons:N2} kWh)";
                                }
                                else if (ratio > 3.0)
                                {
                                    var percent = ((consumption.Value - prevCons) / prevCons) * 100.0;
                                    isSuspicious = true;
                                    suspiciousReason = $"Sản lượng tăng đột biến ({percent:+0.0}% so với kỳ trước: {consumption.Value:N2} vs {prevCons:N2} kWh)";
                                }
                            }
                        }
                    }

                    var effectiveReadingTime = readingTime ?? (latestCutoffTimeMap.TryGetValue(measurementPoint.Id, out var cutT) ? cutT : DateTimeOffset.Now);

                    var unreliableEntry = gcs.FirstOrDefault(x =>
                        x.MeasurementPointId == measurementPoint.Id &&
                        x.MeterReadingSet == readingType.Value);

                    if (unreliableEntry != null)
                    {
                        bool hasUserConfirmedEdit = unreliableEntry.NewCurrentReading != null || unreliableEntry.NewPreviousReading != null;
                        bool meterHasNewDifferentReading = finalCurrentReading.HasValue && unreliableEntry.CurrentReading != finalCurrentReading;
                        if (hasUserConfirmedEdit && !meterHasNewDifferentReading)
                        {
                            // Bảo toàn số liệu đã được người dùng nhập tay hoặc Import từ Excel khi công tơ chưa có chỉ số chốt mới khác biệt
                            var effCurr = unreliableEntry.NewCurrentReading ?? unreliableEntry.CurrentReading;
                            var effPrev = unreliableEntry.NewPreviousReading ?? unreliableEntry.PreviousReading ?? previousReading ?? 0;
                            unreliableEntry.Consumption = isPmax || !effCurr.HasValue
                                ? null
                                : Math.Round((effCurr.Value - effPrev) * measurementPoint.MultiplierFactor, 2, MidpointRounding.AwayFromZero);
                            unreliableEntry.Quanlity = true;
                            unreliableEntry.IsSuspicious = false;
                        }
                        else
                        {
                            unreliableEntry.NewCurrentReading = null;
                            unreliableEntry.CurrentReading = finalCurrentReading;
                            unreliableEntry.Consumption = consumption;
                            unreliableEntry.ReadingTime = effectiveReadingTime;
                            unreliableEntry.Quanlity = quality;
                            unreliableEntry.IsSuspicious = isSuspicious;
                            unreliableEntry.SuspiciousReason = suspiciousReason;
                        }
                        pointEntitiesToUpsert.Add((unreliableEntry, true));
                    }
                    else
                    {
                        var newTempEntity = new GcsTemporaryEntity()
                        {
                            Id = await _snowflakeIdService.GenerateId(cancellationToken),
                            GcsId = request.GcsId,
                            MeasurementPointId = measurementPoint.Id,
                            Day = selectedDayPeriod,
                            Month = request.Month,
                            Year = request.Year,
                            PeriodNumber = request.PeriodNumber,
                            PreviousDay = prevDay,
                            PreviousMonth = prevMonth,
                            PreviousYear = prevYear,
                            PreviousPeriodNumber = prevPeriod,
                            MeterReadingSet = readingType.Value,
                            PreviousReading = previousReading,
                            CurrentReading = finalCurrentReading,
                            MultiplierFactor = measurementPoint.MultiplierFactor,
                            Consumption = consumption,
                            ReadingTime = effectiveReadingTime,
                            Quanlity = quality,
                            IsSuspicious = isSuspicious,
                            SuspiciousReason = suspiciousReason,
                        };
                        pointEntitiesToUpsert.Add((newTempEntity, false));
                    }
                }

                // Tính toán CosPhi trung bình và gán PmaxTime cho toàn bộ các bộ chỉ số của điểm đo
                var sgCons = pointEntitiesToUpsert.FirstOrDefault(x => x.Entity.MeterReadingSet == "SG").Entity?.Consumption;
                var btCons = pointEntitiesToUpsert.FirstOrDefault(x => x.Entity.MeterReadingSet == "BT").Entity?.Consumption;
                var cdCons = pointEntitiesToUpsert.FirstOrDefault(x => x.Entity.MeterReadingSet == "CD").Entity?.Consumption;
                var tdCons = pointEntitiesToUpsert.FirstOrDefault(x => x.Entity.MeterReadingSet == "TD").Entity?.Consumption;
                var vcCons = pointEntitiesToUpsert.FirstOrDefault(x => x.Entity.MeterReadingSet == "VC").Entity?.Consumption;

                double? effectiveAp = sgCons;
                if ((!effectiveAp.HasValue || effectiveAp.Value <= 0) && (btCons.HasValue || cdCons.HasValue || tdCons.HasValue))
                {
                    var sum = (btCons ?? 0) + (cdCons ?? 0) + (tdCons ?? 0);
                    if (sum > 0) effectiveAp = sum;
                }

                var avgCosPhi = Infrastructure.Repositories.ReadWrite.GcsTemporaryReadWriteRepository.CalculateAverageCosPhi(effectiveAp, vcCons);
                var pointPmaxTime = latestPmaxTimeMap.TryGetValue(measurementPoint.Id, out var pTime) ? pTime : (DateTimeOffset?)null;

                foreach (var (ent, isUpd) in pointEntitiesToUpsert)
                {
                    ent.CosPhi = avgCosPhi;
                    ent.PmaxTime = pointPmaxTime;
                    if (isUpd)
                    {
                        await _gcsTemporaryReadWriteRepository.UpdateAsync(ent, cancellationToken);
                    }
                    else
                    {
                        await _gcsTemporaryReadWriteRepository.AddAsync(ent, cancellationToken);
                    }
                }
            }

            await _gcsTemporaryReadWriteRepository.SaveChangesAsync(cancellationToken);
        }

        var result = await _gcsTemporaryReadWriteRepository.GetListGcsTemporaryWithPaginationAsync(request, cancellationToken);
        return RequestResult<CursorPaginationResponse<GetDataLatchResponse>>.Succeed(result);
    }

    public async Task<GcsCutOffCycleDto?> GetGcsCutOffCycleAsync(long gcsId, CancellationToken cancellationToken)
    {
        return await _gcsCutoffCycleEntities
            .AsNoTracking()
            .Where(x => x.GcsId == gcsId)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ThenByDescending(x => x.PeriodNumber)
            .Select(x => new GcsCutOffCycleDto
            {
                Id = x.Id,
                Day = x.Day,
                Month = x.Month,
                Year = x.Year,
                GcsId = x.GcsId,
                PeriodNumber = x.PeriodNumber
            })
            .FirstOrDefaultAsync(cancellationToken);
    }



    private bool IsValidCycleSequence(
        int lastYear,
        int lastMonth,
        int lastPeriodNumber,
        int currentYear,
        int currentMonth,
        int currentPeriodNumber,
        int totalPeriodsInLastMonth,
        bool isNewChange)
    {
        if (currentYear < lastYear)
            return false;

        if (currentYear == lastYear)
        {
            if (currentMonth < lastMonth)
                return false;

            if (currentMonth > lastMonth && currentMonth != lastMonth + 1)
                return false;

            if (currentMonth == lastMonth)
            {
                if (currentPeriodNumber <= lastPeriodNumber)
                    return false;

                if (totalPeriodsInLastMonth > 1 && (currentPeriodNumber != lastPeriodNumber + 1 || currentPeriodNumber > totalPeriodsInLastMonth))
                    return false;

                if (isNewChange)
                    return false;
            }
            else if (currentMonth > lastMonth)
            {
                if (!isNewChange && lastPeriodNumber != totalPeriodsInLastMonth)
                    return false;

                if (currentPeriodNumber != 1)
                    return false;
            }
        }
        else if (currentYear > lastYear)
        {
            if (lastMonth != 12 && lastPeriodNumber == 1 && totalPeriodsInLastMonth == 1)
                return false;
            
            if (lastMonth != 12 && lastPeriodNumber != totalPeriodsInLastMonth)
                return false;

            if (lastPeriodNumber != totalPeriodsInLastMonth && !isNewChange)
                return false;

            if (currentPeriodNumber != 1)
                return false;
        }

        return true;
    }
    
    public async Task<bool> IsSendingMail(SendAllMailRequest request, CancellationToken cancellationToken)
    {
        return await _gcsCutoffCycleEntities.AsNoTracking()
            .Where(x => x.GcsId == request.GcsId && x.Month == request.Month && x.Year == request.Year && x.PeriodNumber == request.PeriodNumber && x.IsSendingMail)
            .AnyAsync(cancellationToken);
    }
}
