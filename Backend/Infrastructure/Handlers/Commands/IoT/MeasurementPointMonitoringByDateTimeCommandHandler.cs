using System.Globalization;
using System.Text.Json;
using Application.Commands.IoT;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.IoT;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Handlers.Commands.IoT;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Infrastructure.Interfaces;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.IoT;

public class MeasurementPointMonitoringByDateTimeCommandHandler : IMeasurementPointMonitoringByDateTimeCommandHandlers
{
    private static readonly HashSet<string> CurrentClockTags = new(StringComparer.OrdinalIgnoreCase)
    {
        ListDataMeasurementPoint.NgayHienTai,
        ListDataMeasurementPoint.ThangHienTai,
        ListDataMeasurementPoint.NamHienTai,
        ListDataMeasurementPoint.GioHienTai,
        ListDataMeasurementPoint.PhutHienTai,
        ListDataMeasurementPoint.GiayHienTai
    };

    private static readonly HashSet<string> CutoffDateTimeTags = new(StringComparer.OrdinalIgnoreCase)
    {
        ListDataMeasurementPoint.NgayChotGanNhat,
        ListDataMeasurementPoint.ThangChotGanNhat,
        ListDataMeasurementPoint.NamChotGanNhat,
        ListDataMeasurementPoint.GioChotGanNhat,
        ListDataMeasurementPoint.PhutChotGanNhat,
        ListDataMeasurementPoint.GiayChotGanNhat
    };

    private static readonly HashSet<string> PmaxCurrDateTimeTags = new(StringComparer.OrdinalIgnoreCase)
    {
        ListDataMeasurementPoint.NamPmaxKyHienTai,
        ListDataMeasurementPoint.ThangPmaxKyHienTai,
        ListDataMeasurementPoint.NgayPmaxKyHienTai,
        ListDataMeasurementPoint.GioPmaxKyHienTai,
        ListDataMeasurementPoint.PhutPmaxKyHienTai,
        ListDataMeasurementPoint.GiayPmaxKyHienTai
    };

    private static readonly HashSet<string> PmaxPrevDateTimeTags = new(StringComparer.OrdinalIgnoreCase)
    {
        ListDataMeasurementPoint.NamPmaxKyTruoc,
        ListDataMeasurementPoint.ThangPmaxKyTruoc,
        ListDataMeasurementPoint.NgayPmaxKyTruoc,
        ListDataMeasurementPoint.GioPmaxKyTruoc,
        ListDataMeasurementPoint.PhutPmaxKyTruoc,
        ListDataMeasurementPoint.GiayPmaxKyTruoc
    };

    private readonly ILoggerService _loggerService;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IHubContext<DetailNotificationDateTimeHub> _detailHubContext;
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;
    private readonly IMeasurementPointMonitoringReadWriteByDateTimeRepository _measurementPointMonitoringReadWriteByDateTimeRepository;

    public MeasurementPointMonitoringByDateTimeCommandHandler(
        ILoggerService loggerService,
        ISnowflakeIdService snowflakeIdService,
        IDateTimeService dateTimeService,
        IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository,
        IMeasurementPointMonitoringReadWriteByDateTimeRepository measurementPointMonitoringReadWriteByDateTimeRepository,
        IHubContext<DetailNotificationDateTimeHub> detailHubContext)
    {
        _loggerService = loggerService;
        _snowflakeIdService = snowflakeIdService;
        _dateTimeService = dateTimeService;
        _measurementPointsReadOnlyRepository = measurementPointsReadOnlyRepository;
        _measurementPointMonitoringReadWriteByDateTimeRepository = measurementPointMonitoringReadWriteByDateTimeRepository;
        _detailHubContext = detailHubContext;
    }

    public async Task<RequestResult<MeasurementPointMonitoringResponse>> Handle(MeasurementPointMonitoringByDateTimeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Request?.Values == null)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
            }

            var viewDetailNotifications = new List<MeasurementPointMonitoringByDateTimeDto>();
            var cultureInfo = CultureInfo.InvariantCulture;

            var measurementPoints = await _measurementPointsReadOnlyRepository.GetListMeasurementPointAsync(cancellationToken);
            var measurementPointIds = measurementPoints.Select(x => x.Id).ToList();
            var groupId = await _snowflakeIdService.GenerateId(cancellationToken);

            // Smart Upsert: Lấy bản ghi mới nhất của từng cặp (MeasurementPointId, TagNameDescription)
            var latestDateTimeMap = await _measurementPointMonitoringReadWriteByDateTimeRepository.GetLatestDateTimeRecordsAsync(
                measurementPointIds,
                cancellationToken);

            var measurementPointEntityToInsert = new List<MeasurementPointMonitoringByDateTimeEntity>();

            foreach (var measurementPoint in measurementPoints)
            {
                if (measurementPoint.TagNames == null || measurementPoint.TagNames.Count == 0)
                {
                    continue;
                }

                var dto = new MeasurementPointMonitoringByDateTimeDto()
                {
                    MeasurementPointsId = measurementPoint.Id,
                    MeasurementPointCode = measurementPoint.Code,
                    MeasurementPointDescription = measurementPoint.Description,
                    CustomerCode = measurementPoint.Customer?.CustomerCode ?? string.Empty,
                    CustomerName = measurementPoint.Customer?.FullName ?? string.Empty,
                    GroupId = groupId,
                };

                var createdTime = _dateTimeService.Now();

                // Thu thập các giá trị hợp lệ của điểm đo trong gói tin hiện tại
                var incomingItems = new List<(TagNameEntity Tag, string ConvertedValue, bool Quality, DateTimeOffset RecordTime)>();
                foreach (var tagName in measurementPoint.TagNames)
                {
                    if (string.IsNullOrEmpty(tagName.Description)) continue;
                    var iotDataValues = request.Request.Values.Where(v => v != null && v.Id == tagName.TagName);
                    foreach (var iotDataValue in iotDataValues)
                    {
                        var convertedValue = ConvertToString(iotDataValue.V);
                        if (string.IsNullOrEmpty(convertedValue) || convertedValue == "null")
                        {
                            continue;
                        }

                        var q = ParseQuality(iotDataValue.Q);
                        var t = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T);
                        incomingItems.Add((tagName, convertedValue, q, t));
                    }
                }

                if (incomingItems.Count == 0)
                {
                    continue;
                }

                // Kiểm tra sự thay đổi theo cụm thời gian đóng băng (Cutoff, PmaxCurr, PmaxPrev)
                bool cutoffClusterChanged = HasClusterChanged(measurementPoint.Id, CutoffDateTimeTags, incomingItems, latestDateTimeMap);
                bool pmaxCurrClusterChanged = HasClusterChanged(measurementPoint.Id, PmaxCurrDateTimeTags, incomingItems, latestDateTimeMap);
                bool pmaxPrevClusterChanged = HasClusterChanged(measurementPoint.Id, PmaxPrevDateTimeTags, incomingItems, latestDateTimeMap);

                var touchedDescriptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var item in incomingItems)
                {
                    var desc = item.Tag.Description!;
                    touchedDescriptions.Add(desc);

                    bool isClockTag = CurrentClockTags.Contains(desc);
                    bool isCutoffTag = CutoffDateTimeTags.Contains(desc);
                    bool isPmaxCurrTag = PmaxCurrDateTimeTags.Contains(desc);
                    bool isPmaxPrevTag = PmaxPrevDateTimeTags.Contains(desc);

                    bool hasExisting = latestDateTimeMap.TryGetValue((measurementPoint.Id, desc), out var existingRec) && existingRec != null;

                    if (isClockTag && hasExisting)
                    {
                        // Đồng hồ hiện tại của công tơ: luôn Update tại chỗ để không làm phình bảng
                        existingRec!.Value = item.ConvertedValue;
                        existingRec.Quanlity = item.Quality;
                        existingRec.GroupId = groupId;
                        existingRec.TagNameId = item.Tag.Id;
                        existingRec.CreatedTime = item.RecordTime;
                    }
                    else if ((isCutoffTag && !cutoffClusterChanged && hasExisting)
                             || (isPmaxCurrTag && !pmaxCurrClusterChanged && hasExisting)
                             || (isPmaxPrevTag && !pmaxPrevClusterChanged && hasExisting))
                    {
                        // Cụm thời điểm chốt / Pmax không thay đổi trong kỳ: chỉ Update GroupId & Quality tại chỗ
                        existingRec!.GroupId = groupId;
                        existingRec.Quanlity = item.Quality;
                        existingRec.TagNameId = item.Tag.Id;
                    }
                    else
                    {
                        // Chưa có bản ghi HOẶC sang kỳ chốt mới / Pmax mới: Insert bản ghi mới
                        var measurementDataId = await _snowflakeIdService.GenerateId(cancellationToken);
                        var measurementPointData = new MeasurementPointMonitoringByDateTimeEntity()
                        {
                            Id = measurementDataId,
                            MeasurementPointId = measurementPoint.Id,
                            TagNameId = item.Tag.Id,
                            TagNameDescription = desc,
                            Value = item.ConvertedValue,
                            Quanlity = item.Quality,
                            GroupId = groupId,
                            CreatedTime = item.RecordTime
                        };

                        measurementPointEntityToInsert.Add(measurementPointData);
                        latestDateTimeMap[(measurementPoint.Id, desc)] = measurementPointData;
                    }

                    PopulateDtoField(dto, desc, item.ConvertedValue, item.Quality, cultureInfo);
                }

                // Đồng bộ GroupId cho các tag DateTime khác của cùng điểm đo chưa xuất hiện trong gói tin này
                var allDateTimeTags = CurrentClockTags
                    .Concat(CutoffDateTimeTags)
                    .Concat(PmaxCurrDateTimeTags)
                    .Concat(PmaxPrevDateTimeTags);

                foreach (var tagDesc in allDateTimeTags)
                {
                    if (!touchedDescriptions.Contains(tagDesc)
                        && latestDateTimeMap.TryGetValue((measurementPoint.Id, tagDesc), out var untouchedRec)
                        && untouchedRec != null)
                    {
                        untouchedRec.GroupId = groupId;
                    }
                }

                dto.CreatedTime = createdTime;
                viewDetailNotifications.Add(dto);
            }

            if (measurementPointEntityToInsert.Count > 0)
            {
                await _measurementPointMonitoringReadWriteByDateTimeRepository.AddRangeAsync(measurementPointEntityToInsert, cancellationToken);
            }
            await _measurementPointMonitoringReadWriteByDateTimeRepository.SaveChangesAsync(cancellationToken);

            foreach (var item in viewDetailNotifications)
            {
                var measurementPointCode = item.MeasurementPointCode;
                if (!string.IsNullOrEmpty(measurementPointCode) && _detailHubContext?.Clients != null)
                {
                    await _detailHubContext.Clients.Group(measurementPointCode).SendAsync("ReceiveDetailDateTimeUpdate", item, cancellationToken: cancellationToken);
                }
            }

            return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }

    private static bool HasClusterChanged(
        long measurementPointId,
        HashSet<string> clusterTags,
        List<(TagNameEntity Tag, string ConvertedValue, bool Quality, DateTimeOffset RecordTime)> incomingItems,
        Dictionary<(long MeasurementPointId, string TagNameDescription), MeasurementPointMonitoringByDateTimeEntity> latestMap)
    {
        foreach (var item in incomingItems)
        {
            var desc = item.Tag.Description;
            if (string.IsNullOrEmpty(desc) || !clusterTags.Contains(desc)) continue;

            if (!latestMap.TryGetValue((measurementPointId, desc), out var existing) || existing == null)
            {
                return true;
            }

            if (!AreValuesEqual(existing.Value, item.ConvertedValue))
            {
                return true;
            }
        }

        return false;
    }

    private static bool AreValuesEqual(string? oldVal, string? newVal)
    {
        if (string.Equals(oldVal?.Trim(), newVal?.Trim(), StringComparison.OrdinalIgnoreCase))
            return true;

        if (double.TryParse(oldVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var d1) &&
            double.TryParse(newVal, NumberStyles.Float, CultureInfo.InvariantCulture, out var d2))
        {
            return Math.Abs(d1 - d2) < 1e-6;
        }

        return false;
    }

    private static void PopulateDtoField(
        MeasurementPointMonitoringByDateTimeDto dto,
        string description,
        string value,
        bool quality,
        CultureInfo cultureInfo)
    {
        var formatted = double.TryParse(value, NumberStyles.Float, cultureInfo, out var d) ? d.ToString(cultureInfo) : value;
        switch (description)
        {
            case ListDataMeasurementPoint.NgayHienTai:
                dto.CurrentDate = formatted;
                dto.CurrentDateQuanlity = quality;
                break;
            case ListDataMeasurementPoint.ThangHienTai:
                dto.CurrentMonth = formatted;
                dto.CurrentMonthQuanlity = quality;
                break;
            case ListDataMeasurementPoint.NamHienTai:
                dto.CurrentYear = formatted;
                dto.CurrentYearQuanlity = quality;
                break;
            case ListDataMeasurementPoint.GioHienTai:
                dto.CurrentTime = formatted;
                dto.CurrentTimeQuanlity = quality;
                break;
            case ListDataMeasurementPoint.PhutHienTai:
                dto.CurrentMinute = formatted;
                dto.CurrentMinuteQuanlity = quality;
                break;
            case ListDataMeasurementPoint.GiayHienTai:
                dto.CurrentSecond = formatted;
                dto.CurrentSecondQuanlity = quality;
                break;
            case ListDataMeasurementPoint.NgayChotGanNhat:
                dto.LatestClosingDate = formatted;
                dto.LatestClosingDateQuanlity = quality;
                break;
            case ListDataMeasurementPoint.ThangChotGanNhat:
                dto.LatestClosingMonth = formatted;
                dto.LatestClosingMonthQuanlity = quality;
                break;
            case ListDataMeasurementPoint.NamChotGanNhat:
                dto.LatestClosingYear = formatted;
                dto.LatestClosingYearQuanlity = quality;
                break;
            case ListDataMeasurementPoint.GioChotGanNhat:
                dto.LatestClosingTime = formatted;
                dto.LatestClosingTimeQuanlity = quality;
                break;
            case ListDataMeasurementPoint.PhutChotGanNhat:
                dto.LatestClosingMinute = formatted;
                dto.LatestClosingMinuteQuanlity = quality;
                break;
            case ListDataMeasurementPoint.GiayChotGanNhat:
                dto.LatestClosingSecond = formatted;
                dto.LatestClosingSecondQuanlity = quality;
                break;
            case ListDataMeasurementPoint.NamPmaxKyHienTai:
                dto.PmaxCurrentPeriodYear = formatted;
                dto.PmaxCurrentPeriodYearQuanlity = quality;
                break;
            case ListDataMeasurementPoint.ThangPmaxKyHienTai:
                dto.PmaxCurrentPeriodMonth = formatted;
                dto.PmaxCurrentPeriodMonthQuanlity = quality;
                break;
            case ListDataMeasurementPoint.NgayPmaxKyHienTai:
                dto.PmaxCurrentPeriodDate = formatted;
                dto.PmaxCurrentPeriodDateQuanlity = quality;
                break;
            case ListDataMeasurementPoint.GioPmaxKyHienTai:
                dto.PmaxCurrentPeriodTime = formatted;
                dto.PmaxCurrentPeriodTimeQuanlity = quality;
                break;
            case ListDataMeasurementPoint.PhutPmaxKyHienTai:
                dto.PmaxCurrentPeriodMinute = formatted;
                dto.PmaxCurrentPeriodMinuteQuanlity = quality;
                break;
            case ListDataMeasurementPoint.GiayPmaxKyHienTai:
                dto.PmaxCurrentPeriodSecond = formatted;
                dto.PmaxCurrentPeriodSecondQuanlity = quality;
                break;
            case ListDataMeasurementPoint.NamPmaxKyTruoc:
                dto.PmaxPreviousPeriodYear = formatted;
                dto.PmaxPreviousPeriodYearQuanlity = quality;
                break;
            case ListDataMeasurementPoint.ThangPmaxKyTruoc:
                dto.PmaxPreviousPeriodMonth = formatted;
                dto.PmaxPreviousPeriodMonthQuanlity = quality;
                break;
            case ListDataMeasurementPoint.NgayPmaxKyTruoc:
                dto.PmaxPreviousPeriodDate = formatted;
                dto.PmaxPreviousPeriodDateQuanlity = quality;
                break;
            case ListDataMeasurementPoint.GioPmaxKyTruoc:
                dto.PmaxPreviousPeriodTime = formatted;
                dto.PmaxPreviousPeriodTimeQuanlity = quality;
                break;
            case ListDataMeasurementPoint.PhutPmaxKyTruoc:
                dto.PmaxPreviousPeriodMinute = formatted;
                dto.PmaxPreviousPeriodMinuteQuanlity = quality;
                break;
            case ListDataMeasurementPoint.GiayPmaxKyTruoc:
                dto.PmaxPreviousPeriodSecond = formatted;
                dto.PmaxPreviousPeriodSecondQuanlity = quality;
                break;
        }
    }

    private static string ConvertToString(object value)
    {
        if (value == null)
        {
            return "null";
        }

        try
        {
            var jsonElement = JsonSerializer.SerializeToElement(value);

            if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                if (jsonElement.TryGetDouble(out double d))
                {
                    return Math.Round(d, 0).ToString("F0", CultureInfo.InvariantCulture);
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.String)
            {
                string strValue = jsonElement.GetString();
                return TryConvertStringToNumber(strValue);
            }
            else if (jsonElement.ValueKind == JsonValueKind.True || jsonElement.ValueKind == JsonValueKind.False)
            {
                return jsonElement.GetBoolean().ToString();
            }
            else if (jsonElement.ValueKind == JsonValueKind.Object || jsonElement.ValueKind == JsonValueKind.Array)
            {
                return jsonElement.ToString();
            }
        }
        catch (JsonException)
        {
            return value switch
            {
                double d => Math.Round(d, 0).ToString("F0", CultureInfo.InvariantCulture),
                float f => Math.Round(f, 0).ToString("F0", CultureInfo.InvariantCulture),
                long l => l.ToString(),
                bool b => b.ToString(),
                string s => s,
                _ => value.ToString()
            } ?? string.Empty;
        }

        return value.ToString();
    }

    private static string TryConvertStringToNumber(string s)
    {
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedDouble))
        {
            return Math.Round(parsedDouble, 0).ToString("F0", CultureInfo.InvariantCulture);
        }

        return s;
    }

    private static bool ParseQuality(string? q)
    {
        if (string.IsNullOrWhiteSpace(q)) return true; // Mặc định Good nếu không gửi cờ Q
        if (bool.TryParse(q, out var bVal)) return bVal;
        var trimmed = q.Trim();
        if (trimmed == "1" || trimmed == "192" || trimmed.Equals("Good", StringComparison.OrdinalIgnoreCase)) return true;
        if (trimmed == "0" || trimmed.Equals("Bad", StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }
}
