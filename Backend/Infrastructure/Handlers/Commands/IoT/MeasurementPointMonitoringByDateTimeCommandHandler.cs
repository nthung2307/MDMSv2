using Application.Commands.IoT;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.IoT;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Handlers.Commands.IoT;
using Application.Services;
using Domain.Helpers;
using Infrastructure.Helpers;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.IoT;

public class MeasurementPointMonitoringByDateTimeCommandHandler : IMeasurementPointMonitoringByDateTimeCommandHandlers
{
    private readonly ITagRoutingCacheService _tagRoutingCacheService;
    private readonly IBufferedTelemetryIngestionService _bufferedIngestionService;
    private readonly ILoggerService _loggerService;

    public MeasurementPointMonitoringByDateTimeCommandHandler(
        ITagRoutingCacheService tagRoutingCacheService,
        IBufferedTelemetryIngestionService bufferedIngestionService,
        ILoggerService loggerService)
    {
        _tagRoutingCacheService = tagRoutingCacheService;
        _bufferedIngestionService = bufferedIngestionService;
        _loggerService = loggerService;
    }

    public async Task<RequestResult<MeasurementPointMonitoringResponse>> Handle(
        MeasurementPointMonitoringByDateTimeCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.Request?.Values == null || request.Request.Values.Count == 0)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
            }

            var tagMap = await _tagRoutingCacheService.GetTagMapAsync(cancellationToken);
            var pointMap = await _tagRoutingCacheService.GetPointMapAsync(cancellationToken);
            if (tagMap.Count == 0)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
            }

            var grouped = new Dictionary<long, (DateTimeOffset LatestTs, MeasurementPointMonitoringByDateTimeDto Notification, List<TelemetryTagSample> Samples)>();

            foreach (var iotVal in request.Request.Values)
            {
                if (string.IsNullOrWhiteSpace(iotVal.Id) ||
                    !tagMap.TryGetValue(iotVal.Id.Trim(), out var tagEntry) ||
                    string.IsNullOrWhiteSpace(tagEntry.Description))
                {
                    continue;
                }

                var rawValue = IoTPacketParserHelper.ConvertToString(iotVal.V);
                if (string.IsNullOrWhiteSpace(rawValue) || string.Equals(rawValue, "null", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var quality = IoTPacketParserHelper.ParseQuality(iotVal.Q);
                var recordTime = iotVal.T > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(iotVal.T)
                    : DateTimeOffset.UtcNow;

                if (!grouped.TryGetValue(tagEntry.MeasurementPointId, out var bucket))
                {
                    pointMap.TryGetValue(tagEntry.MeasurementPointId, out var ptInfo);
                    var dto = new MeasurementPointMonitoringByDateTimeDto
                    {
                        MeasurementPointsId = tagEntry.MeasurementPointId,
                        MeasurementPointCode = tagEntry.MeasurementPointCode,
                        MeasurementPointDescription = ptInfo?.Description ?? tagEntry.MeasurementPointDescription,
                        CustomerCode = ptInfo?.CustomerCode ?? tagEntry.CustomerCode,
                        CustomerName = ptInfo?.CustomerName ?? tagEntry.CustomerName,
                        GroupId = tagEntry.MeasurementPointId,
                        CreatedTime = recordTime
                    };
                    bucket = (recordTime, dto, new List<TelemetryTagSample>());
                }
                else if (recordTime > bucket.LatestTs)
                {
                    bucket.LatestTs = recordTime;
                    bucket.Notification.CreatedTime = recordTime;
                }

                var parsedSmallInt = MeteringValueHelper.ParseSmallInt(rawValue);
                if (!parsedSmallInt.HasValue)
                {
                    continue;
                }

                ApplyDateTimeField(bucket.Notification, tagEntry.Description, rawValue.Trim(), quality);
                bucket.Samples.Add(new TelemetryTagSample
                {
                    Description = tagEntry.Description,
                    SmallIntValue = parsedSmallInt,
                    Quality = quality,
                    Timestamp = recordTime
                });

                grouped[tagEntry.MeasurementPointId] = bucket;
            }

            foreach (var kvp in grouped)
            {
                await _bufferedIngestionService.EnqueueAsync(new TelemetryIngestionItem
                {
                    MeasurementPointId = kvp.Key,
                    MeasurementPointCode = kvp.Value.Notification.MeasurementPointCode ?? string.Empty,
                    Timestamp = kvp.Value.LatestTs,
                    WriteTimeSeries = false,
                    SourcePacketType = 3,
                    Samples = kvp.Value.Samples,
                    DateTimeNotification = kvp.Value.Notification
                }, cancellationToken);
            }

            return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, nameof(MeasurementPointMonitoringByDateTimeCommandHandler));
            throw;
        }
    }

    private static void ApplyDateTimeField(MeasurementPointMonitoringByDateTimeDto dto, string description, string formatted, bool quality)
    {
        switch (description)
        {
            case ListDataMeasurementPoint.NamHienTai:
                dto.CurrentYear = formatted; dto.CurrentYearQuanlity = quality; break;
            case ListDataMeasurementPoint.ThangHienTai:
                dto.CurrentMonth = formatted; dto.CurrentMonthQuanlity = quality; break;
            case ListDataMeasurementPoint.NgayHienTai:
                dto.CurrentDate = formatted; dto.CurrentDateQuanlity = quality; break;
            case ListDataMeasurementPoint.GioHienTai:
                dto.CurrentTime = formatted; dto.CurrentTimeQuanlity = quality; break;
            case ListDataMeasurementPoint.PhutHienTai:
                dto.CurrentMinute = formatted; dto.CurrentMinuteQuanlity = quality; break;
            case ListDataMeasurementPoint.GiayHienTai:
                dto.CurrentSecond = formatted; dto.CurrentSecondQuanlity = quality; break;

            case ListDataMeasurementPoint.NamChotGanNhat:
                dto.LatestClosingYear = formatted; dto.LatestClosingYearQuanlity = quality; break;
            case ListDataMeasurementPoint.ThangChotGanNhat:
                dto.LatestClosingMonth = formatted; dto.LatestClosingMonthQuanlity = quality; break;
            case ListDataMeasurementPoint.NgayChotGanNhat:
                dto.LatestClosingDate = formatted; dto.LatestClosingDateQuanlity = quality; break;
            case ListDataMeasurementPoint.GioChotGanNhat:
                dto.LatestClosingTime = formatted; dto.LatestClosingTimeQuanlity = quality; break;
            case ListDataMeasurementPoint.PhutChotGanNhat:
                dto.LatestClosingMinute = formatted; dto.LatestClosingMinuteQuanlity = quality; break;
            case ListDataMeasurementPoint.GiayChotGanNhat:
                dto.LatestClosingSecond = formatted; dto.LatestClosingSecondQuanlity = quality; break;

            case ListDataMeasurementPoint.NamPmaxKyHienTai:
                dto.PmaxCurrentPeriodYear = formatted; dto.PmaxCurrentPeriodYearQuanlity = quality; break;
            case ListDataMeasurementPoint.ThangPmaxKyHienTai:
                dto.PmaxCurrentPeriodMonth = formatted; dto.PmaxCurrentPeriodMonthQuanlity = quality; break;
            case ListDataMeasurementPoint.NgayPmaxKyHienTai:
                dto.PmaxCurrentPeriodDate = formatted; dto.PmaxCurrentPeriodDateQuanlity = quality; break;
            case ListDataMeasurementPoint.GioPmaxKyHienTai:
                dto.PmaxCurrentPeriodTime = formatted; dto.PmaxCurrentPeriodTimeQuanlity = quality; break;
            case ListDataMeasurementPoint.PhutPmaxKyHienTai:
                dto.PmaxCurrentPeriodMinute = formatted; dto.PmaxCurrentPeriodMinuteQuanlity = quality; break;
            case ListDataMeasurementPoint.GiayPmaxKyHienTai:
                dto.PmaxCurrentPeriodSecond = formatted; dto.PmaxCurrentPeriodSecondQuanlity = quality; break;

            case ListDataMeasurementPoint.NamPmaxKyTruoc:
                dto.PmaxPreviousPeriodYear = formatted; dto.PmaxPreviousPeriodYearQuanlity = quality; break;
            case ListDataMeasurementPoint.ThangPmaxKyTruoc:
                dto.PmaxPreviousPeriodMonth = formatted; dto.PmaxPreviousPeriodMonthQuanlity = quality; break;
            case ListDataMeasurementPoint.NgayPmaxKyTruoc:
                dto.PmaxPreviousPeriodDate = formatted; dto.PmaxPreviousPeriodDateQuanlity = quality; break;
            case ListDataMeasurementPoint.GioPmaxKyTruoc:
                dto.PmaxPreviousPeriodTime = formatted; dto.PmaxPreviousPeriodTimeQuanlity = quality; break;
            case ListDataMeasurementPoint.PhutPmaxKyTruoc:
                dto.PmaxPreviousPeriodMinute = formatted; dto.PmaxPreviousPeriodMinuteQuanlity = quality; break;
            case ListDataMeasurementPoint.GiayPmaxKyTruoc:
                dto.PmaxPreviousPeriodSecond = formatted; dto.PmaxPreviousPeriodSecondQuanlity = quality; break;
        }
    }
}
