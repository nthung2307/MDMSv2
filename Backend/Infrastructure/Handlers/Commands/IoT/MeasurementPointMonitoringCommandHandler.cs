using Application.Commands.IoT;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.IoT;
using Application.Handlers.Commands.IoT;
using Application.Services;
using Domain.Helpers;
using Infrastructure.Helpers;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.IoT;

public class MeasurementPointMonitoringCommandHandler : IMeasurementPointMonitoringCommandHandlers
{
    private readonly ITagRoutingCacheService _tagRoutingCacheService;
    private readonly IBufferedTelemetryIngestionService _bufferedIngestionService;
    private readonly ILoggerService _loggerService;

    public MeasurementPointMonitoringCommandHandler(
        ITagRoutingCacheService tagRoutingCacheService,
        IBufferedTelemetryIngestionService bufferedIngestionService,
        ILoggerService loggerService)
    {
        _tagRoutingCacheService = tagRoutingCacheService;
        _bufferedIngestionService = bufferedIngestionService;
        _loggerService = loggerService;
    }

    public async Task<RequestResult<MeasurementPointMonitoringResponse>> Handle(
        MeasurementPointMonitoringCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.Request?.Values == null || request.Request.Values.Count == 0)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
            }

            var tagMap = await _tagRoutingCacheService.GetTagMapAsync(cancellationToken);
            if (tagMap.Count == 0)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
            }

            var groupedSamples = new Dictionary<long, (string Code, DateTimeOffset LatestTs, List<TelemetryTagSample> Samples)>();

            foreach (var iotVal in request.Request.Values)
            {
                if (string.IsNullOrWhiteSpace(iotVal.Id) ||
                    !tagMap.TryGetValue(iotVal.Id.Trim(), out var tagEntry) ||
                    string.IsNullOrWhiteSpace(tagEntry.Description))
                {
                    continue;
                }

                string rawValue = IoTPacketParserHelper.ConvertToString((object?)iotVal.V);
                if (string.IsNullOrWhiteSpace(rawValue) || string.Equals(rawValue, "null", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                bool quality = IoTPacketParserHelper.ParseQuality((object?)iotVal.Q);
                var recordTime = iotVal.T > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(iotVal.T)
                    : DateTimeOffset.UtcNow;

                var (numericValue, _) = MeteringValueHelper.ParseTelemetryDecimal(rawValue);
                if (!numericValue.HasValue && TelemetryMetadataRegistry.ByDescription.TryGetValue(tagEntry.Description, out var spec) && spec.Kind == SnapshotFieldKind.NumericMetric)
                {
                    // Skip unparseable garbage for numeric channels
                    continue;
                }

                if (!groupedSamples.TryGetValue(tagEntry.MeasurementPointId, out var entry))
                {
                    entry = (tagEntry.MeasurementPointCode, recordTime, new List<TelemetryTagSample>());
                }
                else if (recordTime > entry.LatestTs)
                {
                    entry.LatestTs = recordTime;
                }

                entry.Samples.Add(new TelemetryTagSample
                {
                    Description = tagEntry.Description,
                    Unit = tagEntry.Unit,
                    NumericValue = numericValue,
                    SmallIntValue = MeteringValueHelper.ParseSmallInt(rawValue),
                    Quality = quality,
                    Timestamp = recordTime
                });

                groupedSamples[tagEntry.MeasurementPointId] = entry;
            }

            foreach (var kvp in groupedSamples)
            {
                await _bufferedIngestionService.EnqueueAsync(new TelemetryIngestionItem
                {
                    MeasurementPointId = kvp.Key,
                    MeasurementPointCode = kvp.Value.Code,
                    Timestamp = kvp.Value.LatestTs,
                    WriteTimeSeries = true,
                    SourcePacketType = 1,
                    Samples = kvp.Value.Samples
                }, cancellationToken);
            }

            return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, nameof(MeasurementPointMonitoringCommandHandler));
            throw;
        }
    }
}
