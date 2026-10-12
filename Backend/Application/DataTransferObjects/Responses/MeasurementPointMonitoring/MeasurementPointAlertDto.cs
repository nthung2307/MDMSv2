using Domain.Entities;
using Nobisoft.Core.Domain.Entities;

namespace Application.DataTransferObjects.Responses.MeasurementPointMonitoring;

public class MeasurementPointAlertDto
{
    public long Id { get; set; }
    public long MeasurementPointsId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public string? MeasurementPointCode { get; set; }
    public string? CustomerName { get; set; }
    public string EventName { get; set; }
    public string? Status { get; set; }
    public string? Value { get; set; }
    public bool? Quanlity { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}