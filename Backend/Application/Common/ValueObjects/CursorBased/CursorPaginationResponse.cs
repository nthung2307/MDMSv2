namespace Application.Common.ValueObjects.CursorBased;

public class CursorPaginationResponse<TDataType>
{
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
    public int TotalCount { get; set; }
    public int TotalMeasurementPoints { get; set; }
    public int TotalSuspiciousCount { get; set; }
    public ICollection<TDataType>? Data { get; set; }
}