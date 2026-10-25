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

    public MeasurementPointEventCommandHandler(ILoggerService loggerService, IMeasurementPointEventReadWriteRepository measurementPointEventReadWriteRepository, ISnowflakeIdService snowflakeIdService, IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository, IMeasurementPointAlertReadWriteRepository measurementPointAlertReadWriteRepository, ITagNameReadWriteRepository tagNameReadWriteRepository, IHubContext<NotificationEventHub> notificationEventHub, IMeasurementPointAlertReadOnlyRepository measurementPointAlertReadOnlyRepository, IMeasurementPointByStatusReadWriteRepository measurementPointByStatusReadWriteRepository)
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
            var viewListEventDto = new List<MeasurementPointEventDto>();
            var viewListMeasurementPointAlertDto = new List<MeasurementPointAlertDto>();
            var measurementPoints = await _measurementPointsReadOnlyRepository.GetListMeasurementPointAsync(cancellationToken);

            var measurementPointIds = measurementPoints.Select(mp => mp.Id).ToList();

            var tagNameEntities = await _tagNameReadWriteRepository
                .GetTagNamesForMeasurementPointsAsync(measurementPointIds, cancellationToken);

            var measurementPointsAlertEntities = await _measurementPointAlertReadWriteRepository
                .GetAlertsForMeasurementPointsAsync(measurementPointIds, cancellationToken);

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
                    var iotDataValue = request.Request.Values.OrderByDescending(x => x.T).FirstOrDefault(v => v.Id == tagName.TagName);

                    if (iotDataValue != null)
                    {
                        var convertedValue = ConvertToString(iotDataValue.V);
                        if (string.IsNullOrEmpty(convertedValue) || convertedValue == "null")
                        {
                            continue;
                        }

                        var measurementPointDataByStatus = new MeasurementPointMonitoringByStatusEntity()
                        {
                            MeasurementPointId = measurementPoint.Id,
                            TagNameId = tagName.Id,
                            TagNameDescription = tagName.Description,
                            Value = convertedValue,
                            Quanlity = bool.Parse(iotDataValue.Q),
                            GroupId = groupId,
                            CreatedTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T)
                        };

                        await _measurementPointByStatusReadWriteRepository.AddAsync(measurementPointDataByStatus, cancellationToken);
                    }

                    if (iotDataValue != null)
                    {
                        // Filter out records with empty string values
                        var convertedValue = ConvertToString(iotDataValue.V);
                        if (string.IsNullOrEmpty(convertedValue) || convertedValue == "null")
                        {
                            _loggerService.LogInformation($"Skipping event record with empty value for TagName: {tagName.TagName}, MeasurementPoint: {measurementPoint.Code}");
                            continue;
                        }

                        var parsedV = TryGetBooleanFromJsonElement(iotDataValue.V);
                        var parsedQ = bool.TryParse(iotDataValue.Q, out bool qValue) && qValue;

                        if (!parsedV && parsedV != tagName.WarningValue)
                        {
                            var measurementPointData = new MeasurementPointsEventEntity()
                            {
                                MeasurementPointId = measurementPoint.Id,
                                TagNameId = tagName.Id,
                                EventName = tagName.Description == "MAT_KET_NOI" ? "Trạng thái kết nối" : tagName.CustomName,
                                Value = convertedValue,
                                Quanlity = bool.TryParse(iotDataValue.Q, out var q1) ? q1 : (iotDataValue.Q == "1" || iotDataValue.Q == "192"),
                                Status = !parsedV ? "NORMAL" : "ALARM",
                                CreatedTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T)
                            };

                            await _measurementPointEventReadWriteRepository.AddAsync(measurementPointData, cancellationToken);
                            
                            if (tagName == null)
                            {
                                _loggerService.LogError("TagName is null when trying to update.");
                                throw new InvalidOperationException("TagName entity is null");
                            }
                            
                            tagName.WarningValue = false;
                            await _tagNameReadWriteRepository.UpdateAsync(tagName, cancellationToken);

                            // Remove existing alert if it exists
                            var alert = alerts.FirstOrDefault(a => a.TagNameId == tagName.Id);
                            if (alert != null)
                            {
                                await _measurementPointAlertReadWriteRepository.RemoveAsync(alert, cancellationToken);
                            }
                            
                            var dto = new MeasurementPointEventDto()
                            {
                                MeasurementPointsId = measurementPoint.Id,
                                MeasurementPointCode = measurementPoint.Code,
                                CustomerName = measurementPoint.Customer?.FullName ?? string.Empty,
                                EventName = tagName.Description == "MAT_KET_NOI" ? "Trạng thái kết nối" : tagName.CustomName,
                                Status = !parsedV ? "NORMAL" : "ALARM",
                                Quanlity = bool.TryParse(iotDataValue.Q, out var q2) ? q2 : (iotDataValue.Q == "1" || iotDataValue.Q == "192"),
                                Value = convertedValue,
                                CreatedTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T),
                            };
                            viewListEventDto.Add(dto);
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
                                Quanlity = bool.TryParse(iotDataValue.Q, out var q3) ? q3 : (iotDataValue.Q == "1" || iotDataValue.Q == "192"),
                                Status = !parsedV ? "NORMAL" : "ALARM",
                                CreatedTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T)
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
                                    Quanlity = bool.TryParse(iotDataValue.Q, out var q4) ? q4 : (iotDataValue.Q == "1" || iotDataValue.Q == "192"),
                                    Status = "ALARM",
                                    CreatedTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T)
                                };
                                await _measurementPointAlertReadWriteRepository.AddAsync(measurementPointAlert, cancellationToken);

                                var listAlert = new MeasurementPointAlertDto()
                                {
                                    MeasurementPointsId = measurementPoint.Id,
                                    MeasurementPointCode = measurementPoint.Code,
                                    CustomerName = measurementPoint.Customer?.FullName ?? string.Empty,
                                    EventName = tagName.Description == "MAT_KET_NOI" ? "Trạng thái kết nối" : tagName.CustomName,
                                    Status = "ALARM",
                                    Value = convertedValue,
                                    Quanlity = bool.TryParse(iotDataValue.Q, out var q5) ? q5 : (iotDataValue.Q == "1" || iotDataValue.Q == "192"),
                                    CreatedTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T),
                                };
                                viewListMeasurementPointAlertDto.Add(listAlert);
                            }
                            // If alert already exists, do nothing (no duplicate creation)

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
                                Quanlity = bool.TryParse(iotDataValue.Q, out var q6) ? q6 : (iotDataValue.Q == "1" || iotDataValue.Q == "192"),
                                CreatedTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T),
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

    public static string ConvertToString(object value)
    {
        if (value == null)
        {
            return "null";  // or return string.Empty; based on your preference
        }

        return value switch
        {
            double d => d.ToString(),  // Convert double to string
            float f => f.ToString(),    // Convert float to string
            long l => l.ToString(),      // Convert long to string
            bool b => b.ToString(),      // Convert bool to string
            string s => s,               // Already a string
            _ => value.ToString()        // For other types, call ToString()
        };
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
        else if (value.ValueKind == JsonValueKind.String)
        {
            if (bool.TryParse(value.GetString(), out bool result))
            {
                return result;
            }
        }
        return false; // Default if value is not convertible to bool
    }

}
