using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.Report;

public class DailyFinalizedEnergyReportRequest : CursorPaginationRequest
{
    public long CustomerId { get; set; }
    public long MeasurementPointId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}