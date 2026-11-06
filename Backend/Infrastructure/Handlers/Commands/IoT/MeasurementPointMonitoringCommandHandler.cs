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
    private static readonly HashSet<string> FrozenMonitoringTags = new(StringComparer.OrdinalIgnoreCase)
    {
        ListDataMeasurementPoint.ChiSoChotDienNangAPGioBT,
        ListDataMeasurementPoint.ChiSoChotDienNangAPGioCD,
        ListDataMeasurementPoint.ChiSoChotDienNangAPGioTD,
        ListDataMeasurementPoint.ChiSoChotDienNangAPTong,
        ListDataMeasurementPoint.ChiSoChotDienNangAQTong,
        ListDataMeasurementPoint.ChiSoChotDienNangAPGioBTChieuNhan,
        ListDataMeasurementPoint.ChiSoChotDienNangAPGioCDChieuNhan,
        ListDataMeasurementPoint.ChiSoChotDienNangAPGioTDChieuNhan,
        ListDataMeasurementPoint.ChiSoChotDienNangAPTongChieuNhan,
        ListDataMeasurementPoint.ChiSoChotDienNangAQTongChieuNhan,
        ListDataMeasurementPoint.PmaxKyTruoc,
        ListDataMeasurementPoint.PmaxKyHienTai
    };

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

            var measurementPoints = await _measurementPointsReadOnlyRepository.GetListMeasurementPointAsync(cancellationToken);
            var measurementPointIds = measurementPoints.Select(x => x.Id).ToList();
            var groupId = await _snowflakeIdService.GenerateId(cancellationToken);

            // Smart Upsert: Lấy bản ghi mới nhất của nhóm thông số chốt đóng băng (Frozen Tags)
            var latestFrozenMap = await _measurementPointMonitoringReadWriteRepository.GetLatestFrozenMonitoringRecordsAsync(
                measurementPointIds,
                FrozenMonitoringTags,
                cancellationToken);

            foreach (var measurementPoint in measurementPoints)
            {
                if (measurementPoint.TagNames == null || measurementPoint.TagNames.Count == 0)
                {
                    continue;
                }

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
                var touchedFrozenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
                        var parsedQuality = ParseQuality(iotDataValue.Q);
                        var recordTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T);
                        var isFrozenTag = !string.IsNullOrEmpty(tagName.Description) && FrozenMonitoringTags.Contains(tagName.Description);

                        if (isFrozenTag)
                        {
                            touchedFrozenKeys.Add(tagName.Description!);
                            if (latestFrozenMap.TryGetValue((measurementPoint.Id, tagName.Description!), out var existingFrozen)
                                && existingFrozen != null
                                && AreValuesEqual(existingFrozen.Value, convertedValue))
                            {
                                // Smart Upsert: Giá trị đóng băng không thay đổi -> Chỉ cập nhật GroupId & Quality tại chỗ, KHÔNG tạo dòng mới
                                existingFrozen.GroupId = groupId;
                                existingFrozen.Quanlity = parsedQuality;
                                existingFrozen.TagNameId = tagName.Id;
                                continue;
                            }
                        }

                        var measurementDataId = await _snowflakeIdService.GenerateId(cancellationToken);
                        var measurementPointData = new MeasurementPointMonitoringEntity()
                        {
                            Id = measurementDataId,
                            MeasurementPointsId = measurementPoint.Id,
                            TagNameId = tagName.Id,
                            TagNameDescription = tagName.Description,
                            Value = convertedValue,
                            Quanlity = parsedQuality,
                            GroupId = groupId,
                            CreatedTime = recordTime
                        };

                        listEntity.Add(measurementPointData);

                        if (isFrozenTag)
                        {
                            latestFrozenMap[(measurementPoint.Id, tagName.Description!)] = measurementPointData;
                        }
                    }
                }

                if (count > 0)
                {
                    // Giữ đồng bộ GroupId cho các Frozen Tags khác của cùng điểm đo (nếu gói tin gửi từng phần)
                    foreach (var frozenTagDesc in FrozenMonitoringTags)
                    {
                        if (!touchedFrozenKeys.Contains(frozenTagDesc)
                            && latestFrozenMap.TryGetValue((measurementPoint.Id, frozenTagDesc), out var untouchedFrozen)
                            && untouchedFrozen != null)
                        {
                            untouchedFrozen.GroupId = groupId;
                        }
                    }

                    viewDetailNotifications.Add(dto);
                }
            }

            if (listEntity.Count > 0)
            {
                await _measurementPointMonitoringReadWriteRepository.AddRangeAsync(listEntity, cancellationToken);
            }
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

            _loggerService.LogInformation($"Successfully processed measurement point monitoring for group: {groupId} (Inserted: {listEntity.Count})");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, "Error in ProcessMeasurementPointMonitoring");
            throw;
        }
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
