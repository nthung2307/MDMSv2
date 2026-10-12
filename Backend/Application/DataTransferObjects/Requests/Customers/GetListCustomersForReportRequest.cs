using Application.Common.ValueObjects.CursorBased;
using Application.Common.ValueObjects.OffsetBased;

namespace Application.DataTransferObjects.Requests.Customers;

public class GetListCustomersForReportRequest : CursorPaginationRequest
{
    public long? CustomerId { get; set; }
}