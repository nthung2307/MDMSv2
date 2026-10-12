namespace Application.DataTransferObjects.Responses.Report;
public class PowerChartResponse
{
    public DateTimeOffset? Date { get; set; }
    public List<DataPointPowerChart> CurrentPeriodData { get; set; } = [];
}

public class DataPointPowerChart
{
    public string? LabelMax { get; set; }
    public double? ValueMax { get; set; }
    public string? LabelMin { get; set; }
    public double? ValueMin { get; set; }
    public string? Unit { get; set; }
}
