using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.MeasurementPoints;

public class GetListMeasurementPointEventRequest : CursorPaginationRequest
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? MeasurementPointCode { get; set; }
}