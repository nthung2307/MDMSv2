namespace Application.Common.ValueObjects.CursorBased;

public class CursorPaginationResponse<TDataType>
{
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
    public ICollection<TDataType>? Data { get; set; }
}