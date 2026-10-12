using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.IoT;

public class GetDataLatchRequest : CursorPaginationRequest
{
    public long GcsId { get; set; }
    public int PeriodNumber { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public bool IsRefresh { get; set; }
    public bool? Quanlity { get; set; }
    public string? Type { get; set; }
}