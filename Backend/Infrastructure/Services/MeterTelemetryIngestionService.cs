using System.Collections.Concurrent;
using System.Globalization;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.IoT;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Helpers;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Domain.Helpers;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.SignalR;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Services;

public sealed class MeterTelemetryIngestionService : IMeterTelemetryIngestionService
{
    private readonly ITagRoutingCacheService _tagRoutingCacheService;
    private readonly IBufferedTelemetryIngestionService _bufferedIngestionService;
    private readonly ILoggerService _loggerService;
    private readonly IMeasurementPointEventReadWriteRepository _measurementPointEventReadWriteRepository;
    private readonly IMeasurementPointAlertReadWriteRepository _measurementPointAlertReadWriteRepository;
    private readonly ITagNameReadWriteRepository _tagNameReadWriteRepository;
    private readonly IMeasurementPointAlertReadOnlyRepository _measurementPointAlertReadOnlyRepository;
    private readonly IHubContext<NotificationEventHub> _notificationEventHub;
    private readonly ISnowflakeIdService _snowflakeIdService;

    // In-memory state cache to avoid querying MySQL on every periodic packet when status tags are unchanged
    private static readonly ConcurrentDictionary<(long PointId, string ParamCode), bool> _activeAlarmStateCache = new();
    private static readonly ConcurrentDictionary<long, DateTimeOffset> _pointStateLoadedAt = new();
    private static readonly ConcurrentDictionary<long, DateTimeOffset> _statusSnapshotFlushedAt = new();
    private static readonly SemaphoreSlim _statusDbConcurrencyGate = new(4, 4);

    public MeterTelemetryIngestionService(
        ITagRoutingCacheService tagRoutingCacheService,
        IBufferedTelemetryIngestionService bufferedIngestionService,
        ILoggerService loggerService,
        IMeasurementPointEventReadWriteRepository measurementPointEventReadWriteRepository,
        IMeasurementPointAlertReadWriteRepository measurementPointAlertReadWriteRepository,
        ITagNameReadWriteRepository tagNameReadWriteRepository,
        IMeasurementPointAlertReadOnlyRepository measurementPointAlertReadOnlyRepository,
        IHubContext<NotificationEventHub> notificationEventHub,
        ISnowflakeIdService snowflakeIdService)
    {
        _tagRoutingCacheService = tagRoutingCacheService;
        _bufferedIngestionService = bufferedIngestionService;
        _loggerService = loggerService;
        _measurementPointEventReadWriteRepository = measurementPointEventReadWriteRepository;
        _measurementPointAlertReadWriteRepository = measurementPointAlertReadWriteRepository;
        _tagNameReadWriteRepository = tagNameReadWriteRepository;
        _measurementPointAlertReadOnlyRepository = measurementPointAlertReadOnlyRepository;
        _notificationEventHub = notificationEventHub;
        _snowflakeIdService = snowflakeIdService;
    }

    public async Task<RequestResult<MeasurementPointMonitoringResponse>> IngestAsync(
        IoTDataRequest? request,
        string? meterSerial,
        TelemetrySourceKind sourceKind = TelemetrySourceKind.Unified,
        CancellationToken cancellationToken = default)
    {
        if (request?.Values == null || request.Values.Count == 0)
        {
            return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
        }

        PointRoutingInfo? targetPointBySerial = null;
        IReadOnlyDictionary<string, TagRoutingEntry>? globalTagMap = null;
        var pointMap = await _tagRoutingCacheService.GetPointMapAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(meterSerial))
        {
            targetPointBySerial = await _tagRoutingCacheService.GetPointBySerialAsync(meterSerial, cancellationToken);
            if (targetPointBySerial == null || !string.Equals(targetPointBySerial.Status, EntityStatus.Active, StringComparison.OrdinalIgnoreCase))
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Fail(new[]
                {
                    new ErrorItem
                    {
                        FieldName = "X-Meter-Serial",
                        Error = $"Không tìm thấy điểm đo đang hoạt động với MeterSerial '{meterSerial.Trim()}'."
                    }
                });
            }
        }
        else
        {
            globalTagMap = await _tagRoutingCacheService.GetTagMapAsync(cancellationToken);
            if (globalTagMap.Count == 0)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
            }
        }

        var monitoringBuckets = new Dictionary<long, (string Code, DateTimeOffset LatestTs, List<TelemetryTagSample> Samples)>();
        var dateTimeBuckets = new Dictionary<long, (DateTimeOffset LatestTs, MeasurementPointMonitoringByDateTimeDto Notification, List<TelemetryTagSample> Samples)>();
        var statusEntries = new List<MatchedStatusEntry>();

        foreach (var iotVal in request.Values)
        {
            if (iotVal == null || string.IsNullOrWhiteSpace(iotVal.Id))
                continue;

            TagRoutingEntry? resolvedTag;
            if (targetPointBySerial != null)
            {
                resolvedTag = _tagRoutingCacheService.ResolveTagForPoint(targetPointBySerial, iotVal.Id);
            }
            else
            {
                globalTagMap!.TryGetValue(iotVal.Id.Trim(), out resolvedTag);
            }

            if (resolvedTag == null || string.IsNullOrWhiteSpace(resolvedTag.Description))
                continue;

            var rawValue = IoTPacketParserHelper.ConvertToString((object?)iotVal.V);
            if (string.IsNullOrWhiteSpace(rawValue) || string.Equals(rawValue, "null", StringComparison.OrdinalIgnoreCase))
                continue;

            var quality = IoTPacketParserHelper.ParseQuality((object?)iotVal.Q);
            var recordTime = iotVal.T > 0
                ? DateTimeOffset.FromUnixTimeMilliseconds(iotVal.T)
                : DateTimeOffset.UtcNow;

            // Route 1: Grouped Date / Time OBIS (0_9_1 .. 0_9_8)
            if (resolvedTag.CompositeDateTime != ObisCompositeDateTimeTarget.None)
            {
                if (!ObisTelemetryCatalog.TryDecomposeCompositeDateTime(resolvedTag.CompositeDateTime, rawValue, out var parts))
                {
                    continue;
                }

                var (p1Desc, p2Desc, p3Desc) = ObisTelemetryCatalog.GetSnapshotDateTimeComponentParameters(resolvedTag.CompositeDateTime);
                var bucket = GetOrCreateDateTimeBucket(dateTimeBuckets, resolvedTag, pointMap, recordTime);

                AddDateTimeSample(bucket, p1Desc, parts.P1, quality, recordTime);
                AddDateTimeSample(bucket, p2Desc, parts.P2, quality, recordTime);
                AddDateTimeSample(bucket, p3Desc, parts.P3, quality, recordTime);
                continue;
            }

            // Route 2: Boolean Status & Alerts (96_5_0, 97_97_1..10, Status.Efa*, Status.Error)
            var isStatusCategory = sourceKind == TelemetrySourceKind.LegacyAlert ||
                                   string.Equals(resolvedTag.Type, "WARNING", StringComparison.OrdinalIgnoreCase) ||
                                   string.Equals(resolvedTag.Type, "ALERT", StringComparison.OrdinalIgnoreCase) ||
                                   (TelemetryMetadataRegistry.ByDescription.TryGetValue(resolvedTag.Description, out var metaSpec) &&
                                    metaSpec.Kind == SnapshotFieldKind.StatusBool);

            if (isStatusCategory)
            {
                var parsedBool = IoTPacketParserHelper.TryGetBoolean((object?)iotVal.V);
                statusEntries.Add(new MatchedStatusEntry(resolvedTag, iotVal.T, recordTime, rawValue, quality, parsedBool));
                continue;
            }

            // Route 3: Legacy 24 individual DateTime parts (SmallInt)
            if (TelemetryMetadataRegistry.ByDescription.TryGetValue(resolvedTag.Description, out var dtSpec) &&
                dtSpec.Kind == SnapshotFieldKind.DateTimePart)
            {
                var parsedSmallInt = MeteringValueHelper.ParseSmallInt(rawValue);
                if (!parsedSmallInt.HasValue)
                    continue;

                var bucket = GetOrCreateDateTimeBucket(dateTimeBuckets, resolvedTag, pointMap, recordTime);
                ApplyDateTimeField(bucket.Notification, resolvedTag.Description, rawValue.Trim(), quality);
                bucket.Samples.Add(new TelemetryTagSample
                {
                    Description = resolvedTag.Description,
                    SmallIntValue = parsedSmallInt,
                    Quality = quality,
                    Timestamp = recordTime
                });
                continue;
            }

            // Route 4: Numeric Electrical / Energy Telemetry
            var (numericValue, _) = MeteringValueHelper.ParseTelemetryDecimal(rawValue);
            if (!numericValue.HasValue &&
                TelemetryMetadataRegistry.ByDescription.TryGetValue(resolvedTag.Description, out var numSpec) &&
                numSpec.Kind == SnapshotFieldKind.NumericMetric)
            {
                continue;
            }

            if (!monitoringBuckets.TryGetValue(resolvedTag.MeasurementPointId, out var monBucket))
            {
                monBucket = (resolvedTag.MeasurementPointCode, recordTime, new List<TelemetryTagSample>());
            }
            else if (recordTime > monBucket.LatestTs)
            {
                monBucket.LatestTs = recordTime;
            }

            monBucket.Samples.Add(new TelemetryTagSample
            {
                Description = resolvedTag.Description,
                Unit = resolvedTag.Unit,
                NumericValue = numericValue,
                SmallIntValue = MeteringValueHelper.ParseSmallInt(rawValue),
                Quality = quality,
                Timestamp = recordTime
            });
            monitoringBuckets[resolvedTag.MeasurementPointId] = monBucket;
        }

        // Flush numeric monitoring batches
        foreach (var kvp in monitoringBuckets)
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

        // Flush DateTime batches
        foreach (var kvp in dateTimeBuckets)
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

        // Process Status & Alerts (supports both manual TagNameId > 0 and Auto-Mapped TagNameId == 0)
        if (statusEntries.Count > 0)
        {
            await ProcessStatusEntriesAsync(statusEntries, cancellationToken);
        }

        return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
    }

    private sealed record MatchedStatusEntry(
        TagRoutingEntry Tag,
        long TimestampMs,
        DateTimeOffset RecordTime,
        string ConvertedVal,
        bool Quality,
        bool ParsedBool);

    private async Task ProcessStatusEntriesAsync(
        List<MatchedStatusEntry> matchedEntries,
        CancellationToken cancellationToken)
    {
        var orderedEntries = matchedEntries.Count > 1
            ? matchedEntries.OrderBy(e => e.TimestampMs).ToList()
            : matchedEntries;

        var statusBatches = new Dictionary<long, (string Code, DateTimeOffset LatestTs, Dictionary<string, TelemetryTagSample> LatestSamples)>();
        var nowUtc = DateTimeOffset.UtcNow;
        var requiresDatabaseCheck = false;

        foreach (var item in orderedEntries)
        {
            UpdateStatusBatch(statusBatches, item);

            if (requiresDatabaseCheck)
                continue;

            var pointId = item.Tag.MeasurementPointId;
            var channelKey = (pointId, item.Tag.Description.ToUpperInvariant());

            if (!_pointStateLoadedAt.TryGetValue(pointId, out var loadedAt))
            {
                requiresDatabaseCheck = true;
            }
            else if (_activeAlarmStateCache.GetValueOrDefault(channelKey, false) != item.ParsedBool)
            {
                requiresDatabaseCheck = true;
            }
            else
            {
                var staggeredTtlMinutes = 30.0 + Math.Abs(pointId % 300) / 60.0;
                if ((nowUtc - loadedAt).TotalMinutes >= staggeredTtlMinutes)
                {
                    requiresDatabaseCheck = true;
                }
            }
        }

        foreach (var kvp in statusBatches)
        {
            var shouldFlushStatusSnapshot = requiresDatabaseCheck ||
                                            !_statusSnapshotFlushedAt.TryGetValue(kvp.Key, out var lastFlushedAt) ||
                                            (nowUtc - lastFlushedAt).TotalMinutes >= 5;

            if (shouldFlushStatusSnapshot)
            {
                _statusSnapshotFlushedAt[kvp.Key] = nowUtc;
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
        }

        if (!requiresDatabaseCheck)
        {
            return;
        }

        await _statusDbConcurrencyGate.WaitAsync(cancellationToken);
        try
        {
            var pointIds = orderedEntries.Select(e => e.Tag.MeasurementPointId).Distinct().ToList();
            var tagEntities = await _tagNameReadWriteRepository.GetTagNamesForMeasurementPointsAsync(pointIds, cancellationToken);
            var tagEntityById = tagEntities.ToDictionary(t => t.Id, t => t);

            var alerts = await _measurementPointAlertReadWriteRepository.GetAlertsForMeasurementPointsAsync(pointIds, cancellationToken);
            var activeStateByChannel = new Dictionary<(long PointId, string ParamCode), bool>();

            foreach (var pid in pointIds)
            {
                _pointStateLoadedAt[pid] = nowUtc;
            }

            foreach (var item in orderedEntries)
            {
                _activeAlarmStateCache[(item.Tag.MeasurementPointId, item.Tag.Description.ToUpperInvariant())] = false;
            }

            // Track current state per channel before evaluating packet events
            foreach (var alert in alerts.Where(a => !string.IsNullOrWhiteSpace(a.ParameterCode)))
            {
                var key = (alert.MeasurementPointId, alert.ParameterCode!.ToUpperInvariant());
                activeStateByChannel[key] = true;
                _activeAlarmStateCache[key] = true;
            }
            foreach (var te in tagEntities.Where(t => t.WarningValue == true && !string.IsNullOrWhiteSpace(t.Description)))
            {
                var key = (te.MeasurementPointId, te.Description!.ToUpperInvariant());
                activeStateByChannel[key] = true;
                _activeAlarmStateCache[key] = true;
            }

            var pendingNewAlerts = new Dictionary<(long PointId, string ParamCode), MeasurementPointsAlertEntity>();
            var dirtyTags = new Dictionary<long, TagNameEntity>();

            var hasEvent = false;
            var hasAddedAlert = false;
            var hasRemovedAlert = false;

            foreach (var item in orderedEntries)
            {
                var channelKey = (item.Tag.MeasurementPointId, item.Tag.Description.ToUpperInvariant());
                tagEntityById.TryGetValue(item.Tag.TagNameId, out var tagEntity);

                var previousState = tagEntity != null
                    ? tagEntity.WarningValue == true
                    : activeStateByChannel.GetValueOrDefault(channelKey, false);

                var eventDisplayName = string.Equals(item.Tag.Description, ListDataMeasurementPoint.MatKetNoi, StringComparison.OrdinalIgnoreCase)
                    ? "Trạng thái kết nối"
                    : !string.IsNullOrWhiteSpace(item.Tag.CustomName) ? item.Tag.CustomName : item.Tag.Description;

                if (!item.ParsedBool)
                {
                    if (item.ParsedBool != previousState)
                    {
                        await AddEventAsync(item, tagEntity?.Id, eventDisplayName, "NORMAL", cancellationToken);
                        hasEvent = true;
                    }

                    activeStateByChannel[channelKey] = false;
                    _activeAlarmStateCache[channelKey] = false;
                    if (tagEntity != null && tagEntity.WarningValue == true)
                    {
                        tagEntity.WarningValue = false;
                        dirtyTags[tagEntity.Id] = tagEntity;
                    }

                    hasRemovedAlert |= await ClearMatchingAlertsAsync(alerts, pendingNewAlerts, tagEntity?.Id, item.Tag, channelKey, cancellationToken);
                }
                else if (item.ParsedBool != previousState)
                {
                    await AddEventAsync(item, tagEntity?.Id, eventDisplayName, "ALARM", cancellationToken);
                    hasEvent = true;

                    activeStateByChannel[channelKey] = true;
                    _activeAlarmStateCache[channelKey] = true;
                    var existingAlert = FindMatchingAlert(alerts, tagEntity?.Id, item.Tag);
                    if (existingAlert != null)
                    {
                        existingAlert.Value = "1";
                        existingAlert.Quanlity = item.Quality;
                        existingAlert.Status = "ALARM";
                        existingAlert.ParameterCode ??= item.Tag.Description;
                        existingAlert.CreatedTime = item.RecordTime;
                        if (!pendingNewAlerts.ContainsKey(channelKey))
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
                            TagNameId = tagEntity != null ? tagEntity.Id : null,
                            ParameterCode = item.Tag.Description,
                            EventName = eventDisplayName ?? item.Tag.Description,
                            Value = "1",
                            Quanlity = item.Quality,
                            Status = "ALARM",
                            CreatedTime = item.RecordTime
                        };
                        pendingNewAlerts[channelKey] = alertEntity;
                        alerts.Add(alertEntity);
                    }

                    if (tagEntity != null)
                    {
                        tagEntity.WarningValue = true;
                        dirtyTags[tagEntity.Id] = tagEntity;
                    }
                }
            }

            foreach (var pendingAlert in pendingNewAlerts.Values)
            {
                await _measurementPointAlertReadWriteRepository.AddAsync(pendingAlert, cancellationToken);
                hasAddedAlert = true;
            }

            foreach (var dirtyTag in dirtyTags.Values)
            {
                await _tagNameReadWriteRepository.UpdateAsync(dirtyTag, cancellationToken);
            }

            if (hasEvent)
            {
                await _measurementPointEventReadWriteRepository.SaveChangesAsync(cancellationToken);
            }

            if (hasAddedAlert || hasRemovedAlert)
            {
                await _measurementPointAlertReadWriteRepository.SaveChangesAsync(cancellationToken);
            }

            if (dirtyTags.Count > 0)
            {
                await _tagNameReadWriteRepository.SaveChangesAsync(cancellationToken);
            }

            if (hasEvent || hasAddedAlert || hasRemovedAlert)
            {
                await TryBroadcastNotificationsAsync(hasEvent, hasAddedAlert, hasRemovedAlert, cancellationToken);
            }
        }
        finally
        {
            _statusDbConcurrencyGate.Release();
        }
    }

    private static (DateTimeOffset LatestTs, MeasurementPointMonitoringByDateTimeDto Notification, List<TelemetryTagSample> Samples)
        GetOrCreateDateTimeBucket(
            Dictionary<long, (DateTimeOffset LatestTs, MeasurementPointMonitoringByDateTimeDto Notification, List<TelemetryTagSample> Samples)> grouped,
            TagRoutingEntry resolvedTag,
            IReadOnlyDictionary<long, PointRoutingInfo> pointMap,
            DateTimeOffset recordTime)
    {
        if (!grouped.TryGetValue(resolvedTag.MeasurementPointId, out var bucket))
        {
            pointMap.TryGetValue(resolvedTag.MeasurementPointId, out var ptInfo);
            var dto = new MeasurementPointMonitoringByDateTimeDto
            {
                MeasurementPointsId = resolvedTag.MeasurementPointId,
                MeasurementPointCode = resolvedTag.MeasurementPointCode,
                MeasurementPointDescription = ptInfo?.Description ?? resolvedTag.MeasurementPointDescription,
                CustomerCode = ptInfo?.CustomerCode ?? resolvedTag.CustomerCode,
                CustomerName = ptInfo?.CustomerName ?? resolvedTag.CustomerName,
                GroupId = resolvedTag.MeasurementPointId,
                CreatedTime = recordTime
            };
            bucket = (recordTime, dto, new List<TelemetryTagSample>());
            grouped[resolvedTag.MeasurementPointId] = bucket;
        }
        else if (recordTime > bucket.LatestTs)
        {
            bucket.LatestTs = recordTime;
            bucket.Notification.CreatedTime = recordTime;
            grouped[resolvedTag.MeasurementPointId] = bucket;
        }

        return bucket;
    }

    private static void AddDateTimeSample(
        (DateTimeOffset LatestTs, MeasurementPointMonitoringByDateTimeDto Notification, List<TelemetryTagSample> Samples) bucket,
        string componentDesc,
        short value,
        bool quality,
        DateTimeOffset recordTime)
    {
        if (string.IsNullOrEmpty(componentDesc)) return;

        var formatted = value.ToString(CultureInfo.InvariantCulture);
        ApplyDateTimeField(bucket.Notification, componentDesc, formatted, quality);
        bucket.Samples.Add(new TelemetryTagSample
        {
            Description = componentDesc,
            SmallIntValue = value,
            Quality = quality,
            Timestamp = recordTime
        });
    }

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
        long? tagEntityId,
        string? eventDisplayName,
        string status,
        CancellationToken cancellationToken)
    {
        var isAlarm = string.Equals(status, "ALARM", StringComparison.OrdinalIgnoreCase);
        var eventEntity = new MeasurementPointsEventEntity
        {
            Id = await _snowflakeIdService.GenerateId(cancellationToken),
            MeasurementPointId = item.Tag.MeasurementPointId,
            TagNameId = tagEntityId,
            ParameterCode = item.Tag.Description,
            EventName = eventDisplayName,
            Value = isAlarm ? "1" : "0",
            Quanlity = item.Quality,
            Status = isAlarm ? "ALARM" : "NORMAL",
            CreatedTime = item.RecordTime
        };
        await _measurementPointEventReadWriteRepository.AddAsync(eventEntity, cancellationToken);
    }

    private static MeasurementPointsAlertEntity? FindMatchingAlert(
        List<MeasurementPointsAlertEntity> alerts,
        long? tagEntityId,
        TagRoutingEntry tag)
    {
        return alerts.FirstOrDefault(a =>
            (tagEntityId.HasValue && a.TagNameId == tagEntityId.Value) ||
            (a.MeasurementPointId == tag.MeasurementPointId &&
             string.Equals(a.ParameterCode, tag.Description, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task<bool> ClearMatchingAlertsAsync(
        List<MeasurementPointsAlertEntity> alerts,
        Dictionary<(long PointId, string ParamCode), MeasurementPointsAlertEntity> pendingNewAlerts,
        long? tagEntityId,
        TagRoutingEntry tag,
        (long PointId, string ParamCode) channelKey,
        CancellationToken cancellationToken)
    {
        var matchingAlerts = alerts
            .Where(a =>
                (tagEntityId.HasValue && a.TagNameId == tagEntityId.Value) ||
                (a.MeasurementPointId == tag.MeasurementPointId &&
                 string.Equals(a.ParameterCode, tag.Description, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var removedAny = false;
        foreach (var alert in matchingAlerts)
        {
            if (pendingNewAlerts.TryGetValue(channelKey, out var pendingAlert) && ReferenceEquals(pendingAlert, alert))
            {
                pendingNewAlerts.Remove(channelKey);
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
