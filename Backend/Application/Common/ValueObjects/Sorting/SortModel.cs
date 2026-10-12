namespace Application.Common.ValueObjects.Sorting;

public class SortModel
{
    public string ColName { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;

    public string PairAsSqlExpression => $"{ColName} {SortDirection}";
}