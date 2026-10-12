namespace Application.DataTransferObjects.Responses.Report;

public class VoltageChartResponse
{
    public DateTimeOffset? Date { get; set; }
    public List<VoltageDataPoint> CurrentPeriodData { get; set; } = [];
}

public class VoltageDataPoint
{
    public string? Label { get; set; }
    public double? PhaA { get; set; }
    public string? PhaAUnit { get; set; }
    public double? PhaB { get; set; }
    public string? PhaBUnit { get; set; }
    public double? PhaC { get; set; }
    public string? PhaCUnit { get; set; }
}
