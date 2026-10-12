using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.GcsMonthlyDetail;

public class GetListGcsMonthlyDetailRequest : CursorPaginationRequest
{
    public long GcsCutOffCycleId { get; set; }
    public bool? IsChange { get; set; }
}