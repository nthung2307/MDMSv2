namespace Application.Common.ValueObjects.Sorting;

public class SearchModel
{
    public string SearchFieldName { get; set; } = string.Empty;
    public string? SearchValue { get; set; } = string.Empty;
}