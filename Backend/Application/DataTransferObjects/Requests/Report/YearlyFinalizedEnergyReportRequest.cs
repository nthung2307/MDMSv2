using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.Report;

public class YearlyFinalizedEnergyReportRequest : CursorPaginationRequest
{
    public long CustomerId { get; set; }
    public long MeasurementPointId { get; set; }
    public int Year { get; set; }
}
