using Domain.Entities;
using Newtonsoft.Json;

namespace Application.DataTransferObjects.Responses.MeasurementPointMonitoring;

public class MeasurementPointMonitoringByDateTimeDto
{
    public long MeasurementPointsId { get; set; }
    public string? MeasurementPointCode { get; set; }
    public string? MeasurementPointDescription { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CurrentDate { get; set; }
    public bool? CurrentDateQuanlity { get; set; }
    public string? CurrentMonth { get; set; }
    public bool? CurrentMonthQuanlity { get; set; }
    public string? CurrentYear { get; set; }
    public bool? CurrentYearQuanlity { get; set; }
    public string? CurrentSecond { get; set; }
    public bool? CurrentSecondQuanlity { get; set; }
    public string? CurrentMinute { get; set; }
    public bool? CurrentMinuteQuanlity { get; set; }
    public string? CurrentTime { get; set; }
    public bool? CurrentTimeQuanlity { get; set; }
    public string? LatestClosingTime { get; set; }
    public bool? LatestClosingTimeQuanlity { get; set; }
    public string? LatestClosingMinute { get; set; }
    public bool? LatestClosingMinuteQuanlity { get; set; }
    public string? LatestClosingSecond { get; set; }
    public bool? LatestClosingSecondQuanlity { get; set; }
    public string? LatestClosingDate { get; set; }
    public bool? LatestClosingDateQuanlity { get; set; }
    public string? LatestClosingMonth { get; set; }
    public bool? LatestClosingMonthQuanlity { get; set; }
    public string? LatestClosingYear { get; set; }
    public bool? LatestClosingYearQuanlity { get; set; }
    public long GroupId { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}