using Application.Common.ValueObjects.CursorBased;

namespace Application.DataTransferObjects.Requests.PeriodicEnergyStatement;

public class GetListPeriodicEnergyStatementRequest : CursorPaginationRequest
{
    public long GcsId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
}