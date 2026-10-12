using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.MeasurementPointMonitoring;

public class GetListMeasurementPointMonitoringRequest : CursorPaginationRequest
{
    public string? CustomerName { get; set; }
    public string? MeasurementPointCode { get; set; }
    public bool? Status { get; set; }
}