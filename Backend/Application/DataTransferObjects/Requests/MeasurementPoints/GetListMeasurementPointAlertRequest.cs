using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.MeasurementPoints;

public class GetListMeasurementPointAlertRequest : CursorPaginationRequest
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? MeasurementPointCode { get; set; }
}