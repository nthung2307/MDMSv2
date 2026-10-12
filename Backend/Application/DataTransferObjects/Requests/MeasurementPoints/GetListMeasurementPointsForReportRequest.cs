using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.MeasurementPoints;

public class GetListMeasurementPointsForReportRequest : CursorPaginationRequest
{
    public long? CustomerId { get; set; }
}