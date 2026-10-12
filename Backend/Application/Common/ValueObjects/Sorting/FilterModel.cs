namespace Application.Common.ValueObjects.Sorting;

public class FilterModel
{
    public string FilterFieldName { get; set; } = string.Empty;
    public string? FilterValue { get; set; } = string.Empty;
}