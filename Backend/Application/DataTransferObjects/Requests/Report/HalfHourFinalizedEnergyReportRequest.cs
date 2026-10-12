using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.Report;

public class HalfHourFinalizedEnergyReportRequest : CursorPaginationRequest
{
    public long CustomerId { get; set; }
    public long MeasurementPointId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
}