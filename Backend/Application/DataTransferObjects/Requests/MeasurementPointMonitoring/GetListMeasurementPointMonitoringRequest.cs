using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.MeasurementPointMonitoring;

public class GetListMeasurementPointMonitoringRequest : CursorPaginationRequest
{
    public string? CustomerName { get; set; }
    public string? MeasurementPointCode { get; set; }
    public bool? Status { get; set; }

    /// <summary>
    /// Phase 3 Filter mode: "all" (default), "good", "modbus_lost", "rest_stale"
    /// </summary>
    public string? FilterMode { get; set; }

    /// <summary>
    /// Inactivity timeout threshold for REST data reception in minutes (defaults to 5 minutes)
    /// </summary>
    public int? RestTimeoutMinutes { get; set; }
}