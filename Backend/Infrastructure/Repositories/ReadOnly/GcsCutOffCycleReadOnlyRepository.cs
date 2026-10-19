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
                var lastDayOfMonth = DateTime.DaysInMonth(cycle.Year, cycle.Month);
                cycle.PreviousDay = cycle.PreviousDay != 0 ? cycle.PreviousDay : cycle.Gcs.DayPeriod.Last() == 999 ? lastDayOfMonth : cycle.Gcs.DayPeriod.Last();
                cycle.PreviousMonth = firstDate.AddMonths(-1).Month;
                cycle.PreviousYear = firstDate.AddMonths(-1).Year;
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

        if (lastGcsCutOffCycle == null && (request.Month != existGcs.CreatedTime.Value.Month || request.Year != existGcs.CreatedTime.Value.Year || request.PeriodNumber != 1))
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
        var dayToUse = existGcs.DayPeriod[request.PeriodNumber - 1] == 999 
            ? lastDayOfMonth 
            : existGcs.DayPeriod[request.PeriodNumber - 1];

        var datetime = DateTime.ParseExact(
            $"{dayToUse.ToString().PadLeft(2, '0')}/{request.Month.ToString().PadLeft(2, '0')}/{request.Year}",
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture);

        if (datetime > DateTime.Now)
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

            var baseUrl = _configuration["BaseUrlGetDataFromServerIoT"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                return RequestResult<CursorPaginationResponse<GetDataLatchResponse>>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = LocalizationString.Common.UnknownFieldName,
                        Error = "Cần config url để lấy dữ liệu từ serverIO."
                    }
                });
            }
            var apiResponses = new List<ReadResult>();
            const int batchSize = 20;

            var relevantDescriptions = new List<string>
            {
                "CHI_SO_CHOT_DIEN_NANG_AP_GIO_BT",
                "CHI_SO_CHOT_DIEN_NANG_AP_GIO_CD",
                "CHI_SO_CHOT_DIEN_NANG_AP_GIO_TD",
                "CHI_SO_CHOT_DIEN_NANG_AP_TONG",
                "CHI_SO_CHOT_DIEN_NANG_AQ_TONG",
                "PMAX_KY_TRUOC",
            };

            var gcsBatches = gcsWithMeasurementPoints
                .Select((name, index) => new { name, index })
                .GroupBy(x => x.index / batchSize)
                .Select(group => group
                    .Where(x => x.name != null)
                    .Select(x => x.name)
                    .ToList()
                );

            foreach (var batch in gcsBatches)
            {
                var queryParams = string.Join("&", batch
                    .SelectMany(measurementPoint =>
                        measurementPoint.TagNames
                            .Where(tag => relevantDescriptions.Contains(tag.Description) && tag.TagName != "" && tag.TagName != null)
                            .Select(tag => $"ids={tag.TagName}")
                    )
                );

                var fullUrl = $"{baseUrl}?{queryParams}";

                var response = await CallApiAsync(fullUrl);
                if (response != null)
                {
                    try 
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            ReferenceHandler = ReferenceHandler.IgnoreCycles,
                            NumberHandling = JsonNumberHandling.AllowReadingFromString
                        };
    
                        var readResult = JsonSerializer.Deserialize<ReadResultWrapper>(response, options);
                        if (readResult?.ReadResults != null)
                        {
                            apiResponses.AddRange(readResult.ReadResults);
                        }
                    }
                    catch (JsonException ex)
                    { 
                        // Log the error
                        Console.WriteLine($"Error deserializing response: {ex.Message}");
                        // Continue with empty list instead of failing
                        apiResponses.AddRange(new List<ReadResult>());
                    }

                }
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
                foreach (var readingType in meterReadingSetMapping)
                {
                    var unreliableEntry = gcs.FirstOrDefault(x =>
                        x.MeasurementPointId == measurementPoint.Id &&
                        x.MeterReadingSet == readingType.Value);
                    
                    var isPmax = readingType.Key == "PMAX_KY_TRUOC";

                    var existingDetail = monthlyDetailsWithPoints.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && x.MeterReadingSet == readingType.Value);
                    if (unreliableEntry != null)
                    {
                        if (unreliableEntry.NewCurrentReading != null)
                        {
                            foreach (var tag in measurementPoint.TagNames)
                            {
                                if (string.Equals(tag.Description, readingType.Key, StringComparison.OrdinalIgnoreCase))
                                {
                                    var correspondingApiResponse = apiResponses.FirstOrDefault(r => r.Id == tag.TagName);
                                    if (correspondingApiResponse != null && correspondingApiResponse.S)
                                    {
                                        unreliableEntry.NewCurrentReading = null;
                                        if (isPmax)
                                        {
                                            unreliableEntry.CurrentReading = Math.Round(correspondingApiResponse.V, 3, MidpointRounding.AwayFromZero); 
                                            unreliableEntry.Consumption = null;
                                        }
                                        else
                                        {
                                            unreliableEntry.CurrentReading = correspondingApiResponse.V;
                                            var currentReading = existingDetail != null ? existingDetail.CurrentReading : measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentReading : 0;
                                            unreliableEntry.Consumption = (correspondingApiResponse.V - currentReading) * measurementPoint.MultiplierFactor;
                                        }
                                        unreliableEntry.ReadingTime = DateTimeOffset.FromUnixTimeMilliseconds(correspondingApiResponse.T);
                                        unreliableEntry.Quanlity = correspondingApiResponse.S;
                                        await _gcsTemporaryReadWriteRepository.UpdateAsync(unreliableEntry, cancellationToken);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var tag in measurementPoint.TagNames)
                            {
                                if (string.Equals(tag.Description, readingType.Key, StringComparison.OrdinalIgnoreCase))
                                {
                                    var correspondingApiResponse = apiResponses.FirstOrDefault(r => r.Id == tag.TagName);
                                    if (correspondingApiResponse != null)
                                    {
                                        unreliableEntry.NewCurrentReading = null;
                                        if (isPmax)
                                        {
                                            unreliableEntry.CurrentReading = Math.Round(correspondingApiResponse.V, 3, MidpointRounding.AwayFromZero);
                                            unreliableEntry.Consumption = null;
                                        }
                                        else
                                        {
                                            unreliableEntry.CurrentReading = correspondingApiResponse.V;
                                            var currentReading = existingDetail != null ? existingDetail.CurrentReading : measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentReading : 0;
                                            unreliableEntry.Consumption = (correspondingApiResponse.V - currentReading) * measurementPoint.MultiplierFactor;
                                        }
                                        unreliableEntry.ReadingTime = DateTimeOffset.FromUnixTimeMilliseconds(correspondingApiResponse.T);
                                        unreliableEntry.Quanlity = correspondingApiResponse.S;
                                        await _gcsTemporaryReadWriteRepository.UpdateAsync(unreliableEntry, cancellationToken);
                                    }
                                }
                            }
                        }
                        continue;
                    }

                    var matchFound = false;
                    foreach (var tag in measurementPoint.TagNames)
                    {
                        if (string.Equals(tag.Description, readingType.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            var correspondingApiResponse = apiResponses.FirstOrDefault(r => r.Id == tag.TagName);
                            if (correspondingApiResponse != null)
                            {
                                if (existingDetail != null)
                                {
                                    matchFound = true;
                                    await _gcsTemporaryReadWriteRepository.AddAsync(new GcsTemporaryEntity()
                                    {
                                        Id = await _snowflakeIdService.GenerateId(cancellationToken),
                                        GcsId = request.GcsId,
                                        MeasurementPointId = measurementPoint.Id,
                                        Day = selectedDayPeriod,
                                        Month = request.Month,
                                        Year = request.Year,
                                        ReadingTime = DateTimeOffset.FromUnixTimeMilliseconds(correspondingApiResponse.T),
                                        PeriodNumber = request.PeriodNumber,
                                        PreviousDay = maxCycle?.Day ?? (existGcs.DayPeriod.Last() == 999 ? DateTime.DaysInMonth(DateTimeOffset.Now.AddMonths(-1).Year, DateTimeOffset.Now.AddMonths(-1).Month) : existGcs.DayPeriod.Last()),
                                        PreviousMonth = maxCycle?.Month ?? DateTimeOffset.Now.AddMonths(-1).Month,
                                        PreviousYear = maxCycle?.Year ?? DateTimeOffset.Now.AddMonths(-1).Year,
                                        PreviousPeriodNumber = maxCycle?.PeriodNumber ?? existGcs.PeriodNumber,
                                        MeterReadingSet = readingType.Value,
                                        PreviousReading = existingDetail.NewCurrentReading ?? existingDetail.CurrentReading,
                                        CurrentReading = isPmax ? Math.Round(correspondingApiResponse.V, 3, MidpointRounding.AwayFromZero) : correspondingApiResponse.V,
                                        MultiplierFactor = measurementPoint.MultiplierFactor,
                                        Consumption = isPmax ? null : (correspondingApiResponse.V - existingDetail.CurrentReading) * measurementPoint.MultiplierFactor,
                                        Quanlity = correspondingApiResponse.S,
                                    }, cancellationToken);
                                }
                                else
                                {
                                    matchFound = true;
                                    await _gcsTemporaryReadWriteRepository.AddAsync(new GcsTemporaryEntity()
                                    {
                                        Id = await _snowflakeIdService.GenerateId(cancellationToken),
                                        GcsId = request.GcsId,
                                        MeasurementPointId = measurementPoint.Id,
                                        Day = selectedDayPeriod,
                                        Month = request.Month,
                                        Year = request.Year,
                                        ReadingTime = DateTimeOffset.FromUnixTimeMilliseconds(correspondingApiResponse.T),
                                        PeriodNumber = request.PeriodNumber,
                                        PreviousDay = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentDay : null,
                                        PreviousMonth = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentMonth : null,
                                        PreviousYear = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentYear : null,
                                        PreviousPeriodNumber = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentPeriodNumber : null,
                                        MeterReadingSet = readingType.Value,
                                        PreviousReading = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentReading ?? 0 : 0,
                                        CurrentReading = isPmax ? Math.Round(correspondingApiResponse.V, 3, MidpointRounding.AwayFromZero) : correspondingApiResponse.V,
                                        MultiplierFactor = measurementPoint.MultiplierFactor,
                                        Consumption = isPmax ? null : (correspondingApiResponse.V - (measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentReading ?? 0 : 0)) * measurementPoint.MultiplierFactor,
                                        Quanlity = correspondingApiResponse.S,
                                    }, cancellationToken);
                                }
                            }
                        }
                    }
                    if (!matchFound)
                    {
                        if (existingDetail != null)
                        {
                            await _gcsTemporaryReadWriteRepository.AddAsync(new GcsTemporaryEntity()
                            {
                                Id = await _snowflakeIdService.GenerateId(cancellationToken),
                                GcsId = request.GcsId,
                                MeasurementPointId = measurementPoint.Id,
                                Day = selectedDayPeriod,
                                Month = request.Month,
                                Year = request.Year,
                                PeriodNumber = request.PeriodNumber,
                                PreviousDay = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentDay : maxCycle?.Day ?? null,
                                PreviousMonth = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentMonth : maxCycle?.Month ?? null,
                                PreviousYear = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentYear : maxCycle?.Year ?? null,
                                PreviousPeriodNumber = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentPeriodNumber : maxCycle?.PeriodNumber ?? null,
                                MeterReadingSet = readingType.Value,
                                PreviousReading = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentReading ?? 0 : existingDetail != null ? existingDetail.CurrentReading : null,
                                CurrentReading = null,
                                MultiplierFactor = measurementPoint.MultiplierFactor,
                                Consumption = null,
                                Quanlity = null,
                            }, cancellationToken);
                        }
                        else
                        {
                            await _gcsTemporaryReadWriteRepository.AddAsync(new GcsTemporaryEntity()
                            {
                                Id = await _snowflakeIdService.GenerateId(cancellationToken),
                                GcsId = request.GcsId,
                                MeasurementPointId = measurementPoint.Id,
                                Day = selectedDayPeriod,
                                Month = request.Month,
                                Year = request.Year,
                                PeriodNumber = request.PeriodNumber,
                                PreviousDay = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentDay : null,
                                PreviousMonth = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentMonth : null,
                                PreviousYear = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentYear : null,
                                PreviousPeriodNumber = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentPeriodNumber : null,
                                MeterReadingSet = readingType.Value,
                                PreviousReading = measurementPoint.IsNewChange ? listCurrentReading.FirstOrDefault(x => x.MeasurementPointId == measurementPoint.Id && readingType.Value == x.MeterReadingSet)?.CurrentReading ?? 0 : existingDetail != null ? existingDetail.CurrentReading : null,
                                CurrentReading = null,
                                MultiplierFactor = measurementPoint.MultiplierFactor,
                                Consumption = null,
                                Quanlity = null,
                            }, cancellationToken);
                        }
                        
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

    private async Task<string> CallApiAsync(string url)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }

    public class ReadResultWrapper
    {
        public List<ReadResult> ReadResults { get; set; }
    }

    public class ReadResult
    {
        public string Id { get; set; }
        public bool S { get; set; }
        public string R { get; set; }
        [JsonConverter(typeof(ValueConverter))]
        public double V { get; set; }
        public long T { get; set; }
    }

    public class ValueConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                    return 0;
                
                if (double.TryParse(stringValue, out double result))
                    return result;
                
                return 0;
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }
        
            return 0;
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    
    public class FlexibleValueConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    // Try to parse string as number, return 0 if parsing fails
                    if (double.TryParse(stringValue, out double number))
                        return number;
                    return 0;
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long l))
                        return l;
                    return reader.GetDouble();
                case JsonTokenType.True:
                case JsonTokenType.False:
                    return reader.GetBoolean();
                case JsonTokenType.Null:
                    return 0;
                default:
                    try 
                    {
                        var element = JsonDocument.ParseValue(ref reader).RootElement;
                        if (element.ValueKind == JsonValueKind.String)
                        {
                            var str = element.GetString();
                            if (double.TryParse(str, out double val))
                                return val;
                            return 0;
                        }
                        return 0;
                    }
                    catch
                    {
                        return 0;
                    }
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is string stringValue)
            {
                if (double.TryParse(stringValue, out double number))
                    writer.WriteNumberValue(number);
                else
                    writer.WriteNumberValue(0);
            }
            else if (value is long longValue)
                writer.WriteNumberValue(longValue);
            else if (value is double doubleValue)
                writer.WriteNumberValue(doubleValue);
            else if (value is bool boolValue)
                writer.WriteBooleanValue(boolValue);
            else
                writer.WriteNumberValue(0);
        }
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
