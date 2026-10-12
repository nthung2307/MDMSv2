using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.MeasurementPoints;

public class GetListMeasurementPointsRequest : CursorPaginationRequest
{
    public long? CustomerId { get; set; }
}