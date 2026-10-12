namespace Application.DataTransferObjects.Responses.Report;
public class LoadChartResponse
{
    public DateTimeOffset? Date { get; set; }
    public DateTimeOffset? PreviousPeriodDate { get; set; }
    public List<DataPoint> CurrentPeriodData { get; set; } = [];
    public List<DataPoint> PreviousPeriodData { get; set; } = [];
}
public class DataPoint
{
    public string? Label { get; set; }
    public double? Value { get; set; }
    public string? Unit { get; set; }
}
