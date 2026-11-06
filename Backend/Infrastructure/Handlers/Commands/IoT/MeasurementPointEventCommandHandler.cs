using System.Text.Json;
using Application.Commands.IoT;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.IoT;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Handlers.Commands.IoT;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.IoT;

public class MeasurementPointEventCommandHandler : IMeasurementPointEventCommandHandlers
{
    private readonly ILoggerService _loggerService;
    private readonly IMeasurementPointEventReadWriteRepository _measurementPointEventReadWriteRepository;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;
    private readonly IMeasurementPointAlertReadWriteRepository _measurementPointAlertReadWriteRepository;
    private readonly ITagNameReadWriteRepository _tagNameReadWriteRepository;
    private readonly IHubContext<NotificationEventHub> _notificationEventHub;
    private readonly IMeasurementPointAlertReadOnlyRepository _measurementPointAlertReadOnlyRepository;
    private readonly IMeasurementPointByStatusReadWriteRepository _measurementPointByStatusReadWriteRepository;

    public MeasurementPointEventCommandHandler(
        ILoggerService loggerService,
        IMeasurementPointEventReadWriteRepository measurementPointEventReadWriteRepository,
        ISnowflakeIdService snowflakeIdService,
        IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository,
        IMeasurementPointAlertReadWriteRepository measurementPointAlertReadWriteRepository,
        ITagNameReadWriteRepository tagNameReadWriteRepository,
        IHubContext<NotificationEventHub> notificationEventHub,
        IMeasurementPointAlertReadOnlyRepository measurementPointAlertReadOnlyRepository,
        IMeasurementPointByStatusReadWriteRepository measurementPointByStatusReadWriteRepository)
    {
        _loggerService = loggerService;
        _measurementPointEventReadWriteRepository = measurementPointEventReadWriteRepository;
        _snowflakeIdService = snowflakeIdService;
        _measurementPointsReadOnlyRepository = measurementPointsReadOnlyRepository;
        _measurementPointAlertReadWriteRepository = measurementPointAlertReadWriteRepository;
        _tagNameReadWriteRepository = tagNameReadWriteRepository;
        _notificationEventHub = notificationEventHub;
        _measurementPointAlertReadOnlyRepository = measurementPointAlertReadOnlyRepository;
        _measurementPointByStatusReadWriteRepository = measurementPointByStatusReadWriteRepository;
    }

    public async Task<RequestResult<MeasurementPointMonitoringResponse>> Handle(MeasurementPointEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Request?.Values == null)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
            }

            var viewListEventDto = new List<MeasurementPointEventDto>();
            var viewListMeasurementPointAlertDto = new List<MeasurementPointAlertDto>();
            var hasRemovedAlert = false;
            var measurementPoints = await _measurementPointsReadOnlyRepository.GetListMeasurementPointAsync(cancellationToken);

            var measurementPointIds = measurementPoints.Select(mp => mp.Id).ToList();

            var tagNameEntities = await _tagNameReadWriteRepository
                .GetTagNamesForMeasurementPointsAsync(measurementPointIds, cancellationToken);

            var measurementPointsAlertEntities = await _measurementPointAlertReadWriteRepository
                .GetAlertsForMeasurementPointsAsync(measurementPointIds, cancellationToken);

            // Smart Upsert: Lấy bản ghi trạng thái hiện hành của từng cặp (MeasurementPointId, TagNameDescription)
            var latestStatusMap = await _measurementPointByStatusReadWriteRepository
                .GetLatestStatusRecordsAsync(measurementPointIds, cancellationToken);

            var tagNameDictionary = tagNameEntities
                .GroupBy(x => x.MeasurementPointId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var measurementPointAlertDictionary = measurementPointsAlertEntities
                .GroupBy(x => x.MeasurementPointId)
                .ToDictionary(g => g.Key, g => g.ToList());
            var groupId = await _snowflakeIdService.GenerateId(cancellationToken);

            foreach (var measurementPoint in measurementPoints)
            {
                var tagNames = tagNameDictionary.GetValueOrDefault(measurementPoint.Id) ?? new List<TagNameEntity>();
                var alerts = measurementPointAlertDictionary.GetValueOrDefault(measurementPoint.Id) ?? new List<MeasurementPointsAlertEntity>();

                foreach (var tagName in tagNames)
                {
                    var iotDataValue = request.Request.Values.OrderByDescending(x => x.T).FirstOrDefault(v => v != null && v.Id == tagName.TagName);

                    if (iotDataValue != null)
                    {
                        var convertedValue = ConvertToString(iotDataValue.V);
                        if (string.IsNullOrEmpty(convertedValue) || convertedValue == "null")
                        {
                            _loggerService.LogInformation($"Skipping event record with empty value for TagName: {tagName.TagName}, MeasurementPoint: {measurementPoint.Code}");
                            continue;
                        }

                        var parsedQuality = ParseQuality(iotDataValue.Q);
                        var recordTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T);

                        // Smart Upsert cho bảng trạng thái hiện hành (MeasurementPointMonitoringByStatusEntity)
                        if (!string.IsNullOrEmpty(tagName.Description)
                            && latestStatusMap.TryGetValue((measurementPoint.Id, tagName.Description), out var existingStatus)
                            && existingStatus != null)
                        {
                            existingStatus.Value = convertedValue;
                            existingStatus.Quanlity = parsedQuality;
                            existingStatus.GroupId = groupId;
                            existingStatus.TagNameId = tagName.Id;
                            existingStatus.CreatedTime = recordTime;
                        }
                        else
                        {
                            var measurementPointDataByStatus = new MeasurementPointMonitoringByStatusEntity()
                            {
                                Id = await _snowflakeIdService.GenerateId(cancellationToken),
                                MeasurementPointId = measurementPoint.Id,
                                TagNameId = tagName.Id,
                                TagNameDescription = tagName.Description,
                                Value = convertedValue,
                                Quanlity = parsedQuality,
                                GroupId = groupId,
                                CreatedTime = recordTime
                            };

                            await _measurementPointByStatusReadWriteRepository.AddAsync(measurementPointDataByStatus, cancellationToken);
                            if (!string.IsNullOrEmpty(tagName.Description))
                            {
                                latestStatusMap[(measurementPoint.Id, tagName.Description)] = measurementPointDataByStatus;
                            }
                        }

                        var parsedV = TryGetBooleanFromJsonElement(iotDataValue.V);

                        if (!parsedV && parsedV != tagName.WarningValue)
                        {
                            var measurementPointData = new MeasurementPointsEventEntity()
                            {
                                Id = await _snowflakeIdService.GenerateId(cancellationToken),
                                MeasurementPointId = measurementPoint.Id,
                                TagNameId = tagName.Id,
                                EventName = tagName.Description == "MAT_KET_NOI" ? "Trạng thái kết nối" : tagName.CustomName,
                                Value = convertedValue,
                                Quanlity = parsedQuality,
                                Status = !parsedV ? "NORMAL" : "ALARM",
                                CreatedTime = recordTime
                            };

                            await _measurementPointEventReadWriteRepository.AddAsync(measurementPointData, cancellationToken);
                            
                            tagName.WarningValue = false;
                            await _tagNameReadWriteRepository.UpdateAsync(tagName, cancellationToken);

                            // Remove existing alert if it exists
                            var existingAlerts = alerts.Where(a => a.TagNameId == tagName.Id).ToList();
                            foreach (var alert in existingAlerts)
                            {
                                await _measurementPointAlertReadWriteRepository.RemoveAsync(alert, cancellationToken);
                                alerts.Remove(alert);
                                hasRemovedAlert = true;
                            }
                            
                            var dto = new MeasurementPointEventDto()
                            {
                                MeasurementPointsId = measurementPoint.Id,
                                MeasurementPointCode = measurementPoint.Code,
                                CustomerName = measurementPoint.Customer?.FullName ?? string.Empty,
                                EventName = tagName.Description == "MAT_KET_NOI" ? "Trạng thái kết nối" : tagName.CustomName,
                                Status = !parsedV ? "NORMAL" : "ALARM",
                                Quanlity = parsedQuality,
                                Value = convertedValue,
                                CreatedTime = recordTime,
                            };
                            viewListEventDto.Add(dto);
                        }
                        else if (!parsedV && parsedV == tagName.WarningValue)
                        {
                            // Clean up any stale alert records if tag is already NORMAL
                            var staleAlerts = alerts.Where(a => a.TagNameId == tagName.Id).ToList();
                            foreach (var staleAlert in staleAlerts)
                            {
                                await _measurementPointAlertReadWriteRepository.RemoveAsync(staleAlert, cancellationToken);
                                alerts.Remove(staleAlert);
                                hasRemovedAlert = true;
                            }
                        }

                        if (parsedV && parsedV != tagName.WarningValue)
                        {
                            var measurementPointData = new MeasurementPointsEventEntity()
                            {
                                Id = await _snowflakeIdService.GenerateId(cancellationToken),
                                MeasurementPointId = measurementPoint.Id,
                                TagNameId = tagName.Id,
                                EventName = tagName.Description == "MAT_KET_NOI" ? "Trạng thái kết nối" : tagName.CustomName,
                                Value = convertedValue,
                                Quanlity = parsedQuality,
                                Status = !parsedV ? "NORMAL" : "ALARM",
                                CreatedTime = recordTime
                            };

                            await _measurementPointEventReadWriteRepository.AddAsync(measurementPointData, cancellationToken);

                            // Check if alert already exists for this measurement point and tag name
                            var existingAlert = alerts.FirstOrDefault(a => a.TagNameId == tagName.Id);
                            
                            // Only create new alert if no existing alert for this tag
                            if (existingAlert == null)
                            {
                                var measurementPointAlert = new MeasurementPointsAlertEntity()
                                {
                                    Id = await _snowflakeIdService.GenerateId(cancellationToken),
                                    MeasurementPointId = measurementPoint.Id,
                                    TagNameId = tagName.Id,
                                    EventName = tagName.Description == "MAT_KET_NOI" ? "Trạng thái kết nối" : tagName.CustomName,
                                    Value = convertedValue,
                                    Quanlity = parsedQuality,
                                    Status = "ALARM",
                                    CreatedTime = recordTime
                                };
                                await _measurementPointAlertReadWriteRepository.AddAsync(measurementPointAlert, cancellationToken);
                                alerts.Add(measurementPointAlert);

                                var listAlert = new MeasurementPointAlertDto()
                                {
                                    MeasurementPointsId = measurementPoint.Id,
                                    MeasurementPointCode = measurementPoint.Code,
                                    CustomerName = measurementPoint.Customer?.FullName ?? string.Empty,
                                    EventName = tagName.Description == "MAT_KET_NOI" ? "Trạng thái kết nối" : tagName.CustomName,
                                    Status = "ALARM",
                                    Value = convertedValue,
                                    Quanlity = parsedQuality,
                                    CreatedTime = recordTime,
                                };
                                viewListMeasurementPointAlertDto.Add(listAlert);
                            }

                            tagName.WarningValue = true;
                            await _tagNameReadWriteRepository.UpdateAsync(tagName, cancellationToken);

                            var dto = new MeasurementPointEventDto()
                            {
                                MeasurementPointsId = measurementPoint.Id,
                                MeasurementPointCode = measurementPoint.Code,
                                CustomerName = measurementPoint.Customer?.FullName ?? string.Empty,
                                EventName = tagName.Description == "MAT_KET_NOI" ? "Trạng thái kết nối" : tagName.CustomName,
                                Status = !parsedV ? "NORMAL" : "ALARM",
                                Value = convertedValue,
                                Quanlity = parsedQuality,
                                CreatedTime = recordTime,
                            };
                            viewListEventDto.Add(dto);
                        }
                        else if (parsedV && parsedV == tagName.WarningValue)
                        {
                            // Value is true and alert already exists - do nothing
                            _loggerService.LogInformation($"Alert already exists for MeasurementPoint: {measurementPoint.Code}, TagName: {tagName.TagName}, Value: {convertedValue}");
                        }
                    }
                }
            }

            await _measurementPointEventReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _measurementPointAlertReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _tagNameReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _measurementPointByStatusReadWriteRepository.SaveChangesAsync(cancellationToken);

            if (viewListEventDto.Count > 0)
                await _notificationEventHub.Clients.All.SendAsync("ReceiveEvent", "New Event", cancellationToken: cancellationToken);
            if (viewListMeasurementPointAlertDto.Count > 0)
                await _notificationEventHub.Clients.All.SendAsync("ReceiveAlert", "New Alert", cancellationToken: cancellationToken);
            else if (hasRemovedAlert)
                await _notificationEventHub.Clients.All.SendAsync("ReceiveAlert", "Clear Alert", cancellationToken: cancellationToken);
            var countMeasurementPoint = await _measurementPointAlertReadOnlyRepository.CountMeasurementPointAlert(cancellationToken);
            await _notificationEventHub.Clients.All.SendAsync("CountAlert", countMeasurementPoint, cancellationToken: cancellationToken);

            return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }

    private static bool ParseQuality(string? q)
    {
        if (string.IsNullOrWhiteSpace(q)) return true;
        if (bool.TryParse(q, out var bVal)) return bVal;
        var trimmed = q.Trim();
        if (trimmed == "1" || trimmed == "192" || trimmed.Equals("Good", StringComparison.OrdinalIgnoreCase)) return true;
        if (trimmed == "0" || trimmed.Equals("Bad", StringComparison.OrdinalIgnoreCase)) return false;
        return true;
    }

    public static string ConvertToString(object value)
    {
        if (value == null)
        {
            return "null";
        }

        return value switch
        {
            double d => d.ToString(),
            float f => f.ToString(),
            long l => l.ToString(),
            bool b => b.ToString(),
            string s => s,
            _ => value.ToString()
        } ?? string.Empty;
    }

    private bool TryGetBooleanFromJsonElement(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.True)
        {
            return true;
        }
        else if (value.ValueKind == JsonValueKind.False)
        {
            return false;
        }
        else if (value.ValueKind == JsonValueKind.Number)
        {
            if (value.TryGetDouble(out var d))
            {
                return Math.Abs(d) > 1e-6;
            }
        }
        else if (value.ValueKind == JsonValueKind.String)
        {
            var str = value.GetString()?.Trim();
            if (string.IsNullOrEmpty(str)) return false;
            if (bool.TryParse(str, out bool result)) return result;
            if (str == "1" || str.Equals("ALARM", StringComparison.OrdinalIgnoreCase)) return true;
            if (str == "0" || str.Equals("NORMAL", StringComparison.OrdinalIgnoreCase)) return false;
        }
        return false;
    }
}
