using System.ComponentModel;
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
using Hangfire;
using Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Infrastructure.Interfaces;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.IoT;

public class MeasurementPointMonitoringCommandHandler : IMeasurementPointMonitoringCommandHandlers
{
    private readonly ILoggerService _loggerService;
    private readonly IMeasurementPointMonitoringReadWriteRepository _measurementPointMonitoringReadWriteRepository;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IHubContext<ListNotificationHub> _listHubContext;
    private readonly IHubContext<DetailNotificationHub> _detailHubContext;
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;
    private readonly IMeasurementPointMonitoringReadOnlyRepository _measurementPointMonitoringReadOnlyRepository;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public MeasurementPointMonitoringCommandHandler(
        ILoggerService loggerService, 
        IMeasurementPointMonitoringReadWriteRepository measurementPointMonitoringReadWriteRepository, 
        ISnowflakeIdService snowflakeIdService, 
        IDateTimeService dateTimeService, 
        IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository, 
        IHubContext<ListNotificationHub> listHubContext, 
        IHubContext<DetailNotificationHub> detailHubContext, 
        IMeasurementPointMonitoringReadOnlyRepository measurementPointMonitoringReadOnlyRepository,
        IBackgroundJobClient backgroundJobClient)
    {
        _loggerService = loggerService;
        _measurementPointMonitoringReadWriteRepository = measurementPointMonitoringReadWriteRepository;
        _snowflakeIdService = snowflakeIdService;
        _dateTimeService = dateTimeService;
        _measurementPointsReadOnlyRepository = measurementPointsReadOnlyRepository;
        _listHubContext = listHubContext;
        _detailHubContext = detailHubContext;
        _measurementPointMonitoringReadOnlyRepository = measurementPointMonitoringReadOnlyRepository;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<RequestResult<MeasurementPointMonitoringResponse>> Handle(MeasurementPointMonitoringCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Serialize the request to pass to the background job
            var serializedRequest = JsonSerializer.Serialize(request);
            
            // Enqueue the entire process as a background job
            var jobId = _backgroundJobClient.Enqueue(() => ProcessMeasurementPointMonitoring(serializedRequest));
            
            _loggerService.LogInformation($"Enqueued measurement point monitoring job with ID: {jobId}");
            
            return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }

    // This method will be called by Hangfire
    [AutomaticRetry(Attempts = 3)]
    [DisplayName("Process Measurement Point Monitoring")]
    public async Task ProcessMeasurementPointMonitoring(string serializedRequest)
    {
        try
        {
            // Deserialize the request
            var request = JsonSerializer.Deserialize<MeasurementPointMonitoringCommand>(serializedRequest);
            if (request?.Request?.Values == null)
            {
                _loggerService.LogError("Failed to deserialize request or request values is null in background job");
                return;
            }
            
            var cancellationToken = CancellationToken.None;
            var viewDetailNotifications = new List<MeasurementPointMonitoringFromIoTDto>();
            var listEntity = new List<MeasurementPointMonitoringEntity>();
            var cultureInfo = CultureInfo.InvariantCulture;

            var measurementPoints = await _measurementPointsReadOnlyRepository.GetListMeasurementPointAsync(cancellationToken);
            var groupId = await _snowflakeIdService.GenerateId(cancellationToken);

            foreach (var measurementPoint in measurementPoints)
            {
                var entityId = await _snowflakeIdService.GenerateId(cancellationToken);
                var dto = new MeasurementPointMonitoringFromIoTDto
                {
                    Id = entityId,
                    MeasurementPointsId = measurementPoint.Id,
                    MeasurementPointCode = measurementPoint.Code,
                    MeasurementPointDescription = measurementPoint.Description,
                    CustomerCode = measurementPoint.Customer?.CustomerCode ?? string.Empty,
                    CustomerName = measurementPoint.Customer?.FullName ?? string.Empty,
                    GroupId = groupId,
                };

                var count = 0;

                var createdTime = _dateTimeService.Now();
                if (measurementPoint.TagNames != null && measurementPoint.TagNames.Count > 0)
                {
                    foreach (var tagName in measurementPoint.TagNames)
                    {
                        var iotDataValues = request.Request.Values.Where(v => v != null && v.Id == tagName.TagName);

                        foreach (var iotDataValue in iotDataValues)
                        {
                            var convertedValue = ConvertToString(iotDataValue.V);
                            if (string.IsNullOrEmpty(convertedValue) || convertedValue == "null")
                            {
                                continue;
                            }

                            count += 1;
                            var measurementDataId = await _snowflakeIdService.GenerateId(cancellationToken);
                            var measurementPointData = new MeasurementPointMonitoringEntity()
                            {
                                Id = measurementDataId,
                                MeasurementPointsId = measurementPoint.Id,
                                TagNameId = tagName.Id,
                                TagNameDescription = tagName.Description,
                                Value = convertedValue,
                                Quanlity = ParseQuality(iotDataValue.Q),
                                GroupId = groupId,
                                CreatedTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T)
                            };

                            listEntity.Add(measurementPointData);
                        }
                    }

                    if (count == 0)
                    {
                        var measurementPointData = new MeasurementPointMonitoringEntity()
                        {
                            MeasurementPointsId = measurementPoint.Id,
                            GroupId = groupId,
                            CreatedTime = createdTime
                        };

                        await _measurementPointMonitoringReadWriteRepository.AddAsync(measurementPointData, cancellationToken);
                    }
                }
                else
                {
                    var measurementPointData = new MeasurementPointMonitoringEntity()
                    {
                        MeasurementPointsId = measurementPoint.Id,
                        GroupId = groupId,
                        CreatedTime = createdTime
                    };

                    await _measurementPointMonitoringReadWriteRepository.AddAsync(measurementPointData, cancellationToken);
                }

                viewDetailNotifications.Add(dto);
            }

            await _measurementPointMonitoringReadWriteRepository.AddRangeAsync(listEntity, cancellationToken);
            await _measurementPointMonitoringReadWriteRepository.SaveChangesAsync(cancellationToken);
            
            // Send notifications synchronously within this job
            var responseViewList = await _measurementPointMonitoringReadOnlyRepository.GetListMeasurementPointsMonitoringAsync(
                new GetListMeasurementPointMonitoringRequest()
                {
                    PageSize = 1000
                }, cancellationToken);

            if (_listHubContext?.Clients != null)
            {
                await _listHubContext.Clients.All.SendAsync("ReceiveUpdate", responseViewList, cancellationToken: cancellationToken);
            }
            
            foreach (var item in viewDetailNotifications)
            {
                var measurementPointCode = item.MeasurementPointCode;   
                if (!string.IsNullOrEmpty(measurementPointCode) && _detailHubContext?.Clients != null)
                {
                    var measurementPointDetail = await _measurementPointMonitoringReadOnlyRepository.GetMeasurementPointDetailByIoTAsync(
                        measurementPointCode, cancellationToken);

                    await _detailHubContext.Clients.Group(measurementPointCode).SendAsync(
                        "ReceiveDetailUpdate", measurementPointDetail, cancellationToken: cancellationToken);
                }
            }

            _loggerService.LogInformation($"Successfully processed measurement point monitoring for group: {groupId}");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, "Error in ProcessMeasurementPointMonitoring");
            throw;
        }
    }

    public static string ConvertToString(object value)
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
                    return Math.Round(d, 3).ToString("F3", CultureInfo.InvariantCulture);
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
                double d => Math.Round(d, 3).ToString("F3", CultureInfo.InvariantCulture),
                float f => Math.Round(f, 3).ToString("F3", CultureInfo.InvariantCulture),
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
            return Math.Round(parsedDouble, 3).ToString("F3", CultureInfo.InvariantCulture);
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
