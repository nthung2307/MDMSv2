namespace Application.Common.ValueObjects.OffsetBased;

public class OffsetPaginationResponse<TDataType>
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public long Total { get; set; }
    public ICollection<TDataType>? Data { get; set; }
}