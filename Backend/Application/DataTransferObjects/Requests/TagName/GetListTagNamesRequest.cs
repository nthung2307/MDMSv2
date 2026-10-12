using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.TagName;

public class GetListTagNamesRequest : CursorPaginationRequest
{
    public bool? HaveMeasurementPoint { get; set; }
}