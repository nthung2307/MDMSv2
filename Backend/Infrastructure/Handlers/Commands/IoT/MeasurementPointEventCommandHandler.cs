using Application.Commands.IoT;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.IoT;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Handlers.Commands.IoT;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Infrastructure.Helpers;
using Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.IoT;

public class MeasurementPointEventCommandHandler : IMeasurementPointEventCommandHandlers
{
    private readonly ITagRoutingCacheService _tagRoutingCacheService;
    private readonly IBufferedTelemetryIngestionService _bufferedIngestionService;
    private readonly ILoggerService _loggerService;
    private readonly IMeasurementPointEventReadWriteRepository _measurementPointEventReadWriteRepository;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IMeasurementPointAlertReadWriteRepository _measurementPointAlertReadWriteRepository;
    private readonly ITagNameReadWriteRepository _tagNameReadWriteRepository;
    private readonly IHubContext<NotificationEventHub> _notificationEventHub;
    private readonly IMeasurementPointAlertReadOnlyRepository _measurementPointAlertReadOnlyRepository;

    public MeasurementPointEventCommandHandler(
        ITagRoutingCacheService tagRoutingCacheService,
        IBufferedTelemetryIngestionService bufferedIngestionService,
        ILoggerService loggerService,
        IMeasurementPointEventReadWriteRepository measurementPointEventReadWriteRepository,
        ISnowflakeIdService snowflakeIdService,
        IMeasurementPointAlertReadWriteRepository measurementPointAlertReadWriteRepository,
        ITagNameReadWriteRepository tagNameReadWriteRepository,
        IHubContext<NotificationEventHub> notificationEventHub,
        IMeasurementPointAlertReadOnlyRepository measurementPointAlertReadOnlyRepository)
    {
        _tagRoutingCacheService = tagRoutingCacheService;
        _bufferedIngestionService = bufferedIngestionService;
        _loggerService = loggerService;
        _measurementPointEventReadWriteRepository = measurementPointEventReadWriteRepository;
        _snowflakeIdService = snowflakeIdService;
        _measurementPointAlertReadWriteRepository = measurementPointAlertReadWriteRepository;
        _tagNameReadWriteRepository = tagNameReadWriteRepository;
        _notificationEventHub = notificationEventHub;
        _measurementPointAlertReadOnlyRepository = measurementPointAlertReadOnlyRepository;
    }

    public async Task<RequestResult<MeasurementPointMonitoringResponse>> Handle(
        MeasurementPointEventCommand request,
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

            // Collect matched entries only; sort stably by timestamp so buffered Kepware packets evaluate in chronological order
            var matchedEntries = new List<MatchedStatusEntry>();
            var pointIds = new HashSet<long>();

            foreach (var iotVal in request.Request.Values)
            {
                if (iotVal == null ||
                    string.IsNullOrWhiteSpace(iotVal.Id) ||
                    !tagMap.TryGetValue(iotVal.Id.Trim(), out var tagEntry))
                {
                    continue;
                }

                var convertedValue = IoTPacketParserHelper.ConvertToString((object?)iotVal.V);
                if (string.IsNullOrEmpty(convertedValue) || string.Equals(convertedValue, "null", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var quality = IoTPacketParserHelper.ParseQuality((object?)iotVal.Q);
                var parsedBool = IoTPacketParserHelper.TryGetBoolean((object?)iotVal.V);
                var recordTime = iotVal.T > 0
                    ? DateTimeOffset.FromUnixTimeMilliseconds(iotVal.T)
                    : DateTimeOffset.UtcNow;

                matchedEntries.Add(new MatchedStatusEntry(tagEntry, iotVal.T, recordTime, convertedValue, quality, parsedBool));
                pointIds.Add(tagEntry.MeasurementPointId);
            }

            if (matchedEntries.Count == 0)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
            }

            var orderedEntries = matchedEntries.Count > 1
                ? matchedEntries.OrderBy(e => e.TimestampMs).ToList()
                : matchedEntries;

            var pointIdList = pointIds.ToList();
            var tagEntities = await _tagNameReadWriteRepository.GetTagNamesForMeasurementPointsAsync(pointIdList, cancellationToken);
            var tagEntityById = tagEntities.ToDictionary(t => t.Id, t => t);

            var alerts = await _measurementPointAlertReadWriteRepository.GetAlertsForMeasurementPointsAsync(pointIdList, cancellationToken);
            var pendingNewAlerts = new Dictionary<long, MeasurementPointsAlertEntity>();
            var dirtyTags = new Dictionary<long, TagNameEntity>();

            var hasEvent = false;
            var hasAddedAlert = false;
            var hasRemovedAlert = false;
            var statusBatches = new Dictionary<long, (string Code, DateTimeOffset LatestTs, Dictionary<string, TelemetryTagSample> LatestSamples)>();

            foreach (var item in orderedEntries)
            {
                UpdateStatusBatch(statusBatches, item);

                if (!tagEntityById.TryGetValue(item.Tag.TagNameId, out var tagEntity))
                {
                    continue;
                }

                var eventDisplayName = string.Equals(item.Tag.Description, ListDataMeasurementPoint.MatKetNoi, StringComparison.OrdinalIgnoreCase)
                    ? "Trạng thái kết nối"
                    : item.Tag.CustomName;

                if (!item.ParsedBool && item.ParsedBool != tagEntity.WarningValue)
                {
                    await AddEventAsync(item, eventDisplayName, "NORMAL", cancellationToken);
                    hasEvent = true;

                    tagEntity.WarningValue = false;
                    dirtyTags[tagEntity.Id] = tagEntity;

                    hasRemovedAlert |= await ClearMatchingAlertsAsync(alerts, pendingNewAlerts, tagEntity.Id, item.Tag, cancellationToken);
                }
                else if (!item.ParsedBool && item.ParsedBool == tagEntity.WarningValue)
                {
                    hasRemovedAlert |= await ClearMatchingAlertsAsync(alerts, pendingNewAlerts, tagEntity.Id, item.Tag, cancellationToken);
                }
                else if (item.ParsedBool && item.ParsedBool != tagEntity.WarningValue)
                {
                    await AddEventAsync(item, eventDisplayName, "ALARM", cancellationToken);
                    hasEvent = true;

                    var existingAlert = FindMatchingAlert(alerts, tagEntity.Id, item.Tag);
                    if (existingAlert != null)
                    {
                        existingAlert.Value = item.ConvertedVal;
                        existingAlert.Quanlity = item.Quality;
                        existingAlert.Status = "ALARM";
                        existingAlert.ParameterCode ??= item.Tag.Description;
                        existingAlert.CreatedTime = item.RecordTime;
                        if (!pendingNewAlerts.ContainsKey(tagEntity.Id))
                        {
                            await _measurementPointAlertReadWriteRepository.UpdateAsync(existingAlert, cancellationToken);
                        }
                    }
                    else
                    {
                        var alertEntity = new MeasurementPointsAlertEntity
                        {
                            Id = await _snowflakeIdService.GenerateId(cancellationToken),
                            MeasurementPointId = item.Tag.MeasurementPointId,
                            TagNameId = item.Tag.TagNameId,
                            ParameterCode = item.Tag.Description,
                            EventName = eventDisplayName ?? item.Tag.Description,
                            Value = item.ConvertedVal,
                            Quanlity = item.Quality,
                            Status = "ALARM",
                            CreatedTime = item.RecordTime
                        };
                        pendingNewAlerts[tagEntity.Id] = alertEntity;
                        alerts.Add(alertEntity);
                    }

                    tagEntity.WarningValue = true;
                    dirtyTags[tagEntity.Id] = tagEntity;
                }
            }

            // Persist only alerts that remained active at the end of the packet (avoids RestfulRepository Attach+Remove deleting unsaved rows)
            foreach (var pendingAlert in pendingNewAlerts.Values)
            {
                await _measurementPointAlertReadWriteRepository.AddAsync(pendingAlert, cancellationToken);
                hasAddedAlert = true;
            }

            foreach (var dirtyTag in dirtyTags.Values)
            {
                await _tagNameReadWriteRepository.UpdateAsync(dirtyTag, cancellationToken);
            }

            foreach (var kvp in statusBatches)
            {
                await _bufferedIngestionService.EnqueueAsync(new TelemetryIngestionItem
                {
                    MeasurementPointId = kvp.Key,
                    MeasurementPointCode = kvp.Value.Code,
                    Timestamp = kvp.Value.LatestTs,
                    WriteTimeSeries = false,
                    SourcePacketType = 2,
                    Samples = kvp.Value.LatestSamples.Values.ToList()
                }, cancellationToken);
            }

            await _measurementPointEventReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _measurementPointAlertReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _tagNameReadWriteRepository.SaveChangesAsync(cancellationToken);

            await TryBroadcastNotificationsAsync(hasEvent, hasAddedAlert, hasRemovedAlert, cancellationToken);

            return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, nameof(MeasurementPointEventCommandHandler));
            throw;
        }
    }

    private sealed record MatchedStatusEntry(
        TagRoutingEntry Tag,
        long TimestampMs,
        DateTimeOffset RecordTime,
        string ConvertedVal,
        bool Quality,
        bool ParsedBool);

    private static void UpdateStatusBatch(
        Dictionary<long, (string Code, DateTimeOffset LatestTs, Dictionary<string, TelemetryTagSample> LatestSamples)> statusBatches,
        MatchedStatusEntry item)
    {
        if (!statusBatches.TryGetValue(item.Tag.MeasurementPointId, out var statusBatch))
        {
            statusBatch = (
                item.Tag.MeasurementPointCode,
                item.RecordTime,
                new Dictionary<string, TelemetryTagSample>(StringComparer.OrdinalIgnoreCase));
        }
        else if (item.RecordTime > statusBatch.LatestTs)
        {
            statusBatch.LatestTs = item.RecordTime;
        }

        statusBatch.LatestSamples[item.Tag.Description] = new TelemetryTagSample
        {
            Description = item.Tag.Description,
            BoolValue = item.ParsedBool,
            Quality = item.Quality,
            Timestamp = item.RecordTime
        };
        statusBatches[item.Tag.MeasurementPointId] = statusBatch;
    }

    private async Task AddEventAsync(
        MatchedStatusEntry item,
        string? eventDisplayName,
        string status,
        CancellationToken cancellationToken)
    {
        var eventEntity = new MeasurementPointsEventEntity
        {
            Id = await _snowflakeIdService.GenerateId(cancellationToken),
            MeasurementPointId = item.Tag.MeasurementPointId,
            TagNameId = item.Tag.TagNameId,
            ParameterCode = item.Tag.Description,
            EventName = eventDisplayName,
            Value = item.ConvertedVal,
            Quanlity = item.Quality,
            Status = status,
            CreatedTime = item.RecordTime
        };
        await _measurementPointEventReadWriteRepository.AddAsync(eventEntity, cancellationToken);
    }

    private static MeasurementPointsAlertEntity? FindMatchingAlert(
        List<MeasurementPointsAlertEntity> alerts,
        long tagEntityId,
        TagRoutingEntry tag)
    {
        return alerts.FirstOrDefault(a =>
            a.TagNameId == tagEntityId ||
            (a.TagNameId == null &&
             a.MeasurementPointId == tag.MeasurementPointId &&
             string.Equals(a.ParameterCode, tag.Description, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task<bool> ClearMatchingAlertsAsync(
        List<MeasurementPointsAlertEntity> alerts,
        Dictionary<long, MeasurementPointsAlertEntity> pendingNewAlerts,
        long tagEntityId,
        TagRoutingEntry tag,
        CancellationToken cancellationToken)
    {
        var matchingAlerts = alerts
            .Where(a =>
                a.TagNameId == tagEntityId ||
                (a.TagNameId == null &&
                 a.MeasurementPointId == tag.MeasurementPointId &&
                 string.Equals(a.ParameterCode, tag.Description, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var removedAny = false;
        foreach (var alert in matchingAlerts)
        {
            if (pendingNewAlerts.TryGetValue(tagEntityId, out var pendingAlert) && ReferenceEquals(pendingAlert, alert))
            {
                pendingNewAlerts.Remove(tagEntityId);
            }
            else
            {
                await _measurementPointAlertReadWriteRepository.RemoveAsync(alert, cancellationToken);
                removedAny = true;
            }

            alerts.Remove(alert);
        }

        return removedAny;
    }

    private async Task TryBroadcastNotificationsAsync(
        bool hasEvent,
        bool hasAddedAlert,
        bool hasRemovedAlert,
        CancellationToken cancellationToken)
    {
        try
        {
            if (hasEvent)
                await _notificationEventHub.Clients.All.SendAsync("ReceiveEvent", "New Event", cancellationToken);
            if (hasAddedAlert)
                await _notificationEventHub.Clients.All.SendAsync("ReceiveAlert", "New Alert", cancellationToken);
            if (hasRemovedAlert)
                await _notificationEventHub.Clients.All.SendAsync("ReceiveAlert", "Clear Alert", cancellationToken);

            var alertCount = await _measurementPointAlertReadOnlyRepository.CountMeasurementPointAlert(cancellationToken);
            await _notificationEventHub.Clients.All.SendAsync("CountAlert", alertCount, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogWarning(ex, nameof(TryBroadcastNotificationsAsync));
        }
    }
}
