using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.GcsCutOffCycle;

public class GetListGcsCutOffCycleRequest : CursorPaginationRequest
{
    public long GcsId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}