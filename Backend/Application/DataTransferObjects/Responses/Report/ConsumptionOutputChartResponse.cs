namespace Application.DataTransferObjects.Responses.Report;
public class ConsumptionOutputChartResponse
{
    public DateTimeOffset? Date { get; set; }
    public string? Unit { get; set; }
    public List<ConsumptionOutputDataPoint> CurrentPeriodData { get; set; } = [];
    public List<PreviousConsumptionOutputDataPoint> PreviousPeriodData { get; set; } = [];
}

public class ConsumptionOutputDataPoint
{
    public string? Label { get; set; }
    public double? Bt { get; set; }
    public double? Td { get; set; }
    public double? Cd { get; set; }
    public double? Sg { get; set; }
    public double? Vc { get; set; }
}

public class PreviousConsumptionOutputDataPoint
{
    public string? Label { get; set; }
    public double? PreviousBt { get; set; }
    public double? PreviousTd { get; set; }
    public double? PreviousCd { get; set; }
    public double? PreviousSg { get; set; }
    public double? PreviousVc { get; set; }
}

