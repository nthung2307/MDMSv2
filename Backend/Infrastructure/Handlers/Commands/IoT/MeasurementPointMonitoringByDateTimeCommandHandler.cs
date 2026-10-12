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
    private readonly ILoggerService _loggerService;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IHubContext<DetailNotificationDateTimeHub> _detailHubContext;
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;
    private readonly IMeasurementPointMonitoringReadWriteByDateTimeRepository _measurementPointMonitoringReadWriteByDateTimeRepository;

    public MeasurementPointMonitoringByDateTimeCommandHandler(ILoggerService loggerService, ISnowflakeIdService snowflakeIdService, IDateTimeService dateTimeService, IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository, IMeasurementPointMonitoringReadWriteByDateTimeRepository measurementPointMonitoringReadWriteByDateTimeRepository, IHubContext<DetailNotificationDateTimeHub> detailHubContext)
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
            var viewDetailNotifications = new List<MeasurementPointMonitoringByDateTimeDto>();
            var cultureInfo = CultureInfo.InvariantCulture;

            var measurementPoints = await _measurementPointsReadOnlyRepository.GetListMeasurementPointAsync(cancellationToken);
            var groupId = await _snowflakeIdService.GenerateId(cancellationToken);

            var measurementPointEntity = new List<MeasurementPointMonitoringByDateTimeEntity>();
            foreach (var measurementPoint in measurementPoints)
            {
                var entityId = await _snowflakeIdService.GenerateId(cancellationToken);
                var dto = new MeasurementPointMonitoringByDateTimeDto()
                {
                    MeasurementPointsId = measurementPoint.Id,
                    MeasurementPointCode = measurementPoint.Code,
                    MeasurementPointDescription = measurementPoint.Description,
                    CustomerCode = measurementPoint.Customer.CustomerCode,
                    CustomerName = measurementPoint.Customer.FullName,
                    GroupId = groupId,
                };

                var count = 0;

                var createdTime = _dateTimeService.Now();
                if (measurementPoint.TagNames.Count > 0)
                {
                    foreach (var tagName in measurementPoint.TagNames)
                    {
                        var iotDataValues = request.Request.Values.Where(v => v.Id == tagName.TagName);

                        foreach (var iotDataValue in iotDataValues)
                        {
                            var convertedValue = ConvertToString(iotDataValue.V);
                            if (string.IsNullOrEmpty(convertedValue) || convertedValue == "null")
                            {
                                continue;
                            }

                            count += 1;
                            var measurementDataId = await _snowflakeIdService.GenerateId(cancellationToken);
                            var measurementPointData = new MeasurementPointMonitoringByDateTimeEntity()
                            {
                                Id = measurementDataId,
                                MeasurementPointId = measurementPoint.Id,
                                TagNameId = tagName.Id,
                                TagNameDescription = tagName.Description,
                                Value = convertedValue,
                                Quanlity = bool.Parse(iotDataValue.Q),
                                GroupId = groupId,
                                CreatedTime = DateTimeOffset.FromUnixTimeMilliseconds(iotDataValue.T)
                            };

                            measurementPointEntity.Add(measurementPointData);
                            var value = convertedValue;
                            switch (tagName.Description)
                            {
                                case ListDataMeasurementPoint.NgayHienTai:
                                    dto.CurrentDate = double.Parse(value, cultureInfo).ToString();
                                    dto.CurrentDateQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.ThangHienTai:
                                    dto.CurrentMonth = double.Parse(value, cultureInfo).ToString();
                                    dto.CurrentMonthQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.NamHienTai:
                                    dto.CurrentYear = double.Parse(value, cultureInfo).ToString();
                                    dto.CurrentYearQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.GioHienTai:
                                    dto.CurrentTime = double.Parse(value, cultureInfo).ToString();
                                    dto.CurrentTimeQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.PhutHienTai:
                                    dto.CurrentMinute = double.Parse(value, cultureInfo).ToString();
                                    dto.CurrentMinuteQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.GiayHienTai:
                                    dto.CurrentSecond = double.Parse(value, cultureInfo).ToString();
                                    dto.CurrentSecondQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.NgayChotGanNhat:
                                    dto.LatestClosingDate = double.Parse(value, cultureInfo).ToString();
                                    dto.LatestClosingDateQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.ThangChotGanNhat:
                                    dto.LatestClosingMonth = double.Parse(value, cultureInfo).ToString();
                                    dto.LatestClosingMonthQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.NamChotGanNhat:
                                    dto.LatestClosingYear = double.Parse(value, cultureInfo).ToString();
                                    dto.LatestClosingYearQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.GioChotGanNhat:
                                    dto.LatestClosingTime = double.Parse(value, cultureInfo).ToString();
                                    dto.LatestClosingTimeQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.PhutChotGanNhat:
                                    dto.LatestClosingMinute = double.Parse(value, cultureInfo).ToString();
                                    dto.LatestClosingMinuteQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                case ListDataMeasurementPoint.GiayChotGanNhat:
                                    dto.LatestClosingSecond = double.Parse(value, cultureInfo).ToString();
                                    dto.LatestClosingSecondQuanlity = bool.Parse(iotDataValue.Q);
                                    break;
                                    }
                        }
                    }
                }
                else
                {
                    var measurementPointData = new MeasurementPointMonitoringByDateTimeEntity()
                    {
                        MeasurementPointId = measurementPoint.Id,
                        GroupId = groupId,
                        CreatedTime = createdTime
                    };

                    await _measurementPointMonitoringReadWriteByDateTimeRepository.AddAsync(measurementPointData, cancellationToken);
                }

                dto.CreatedTime = createdTime;
                viewDetailNotifications.Add(dto);
            }

            await _measurementPointMonitoringReadWriteByDateTimeRepository.AddRangeAsync(measurementPointEntity, cancellationToken);
            await _measurementPointMonitoringReadWriteByDateTimeRepository.SaveChangesAsync(cancellationToken);
            foreach (var item in viewDetailNotifications)
            {
                var measurementPointCode = item.MeasurementPointCode;
                await _detailHubContext.Clients.Group(measurementPointCode).SendAsync("ReceiveDetailDateTimeUpdate", item, cancellationToken: cancellationToken);
            }

            return RequestResult<MeasurementPointMonitoringResponse>.Succeed(new MeasurementPointMonitoringResponse());
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
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

}
