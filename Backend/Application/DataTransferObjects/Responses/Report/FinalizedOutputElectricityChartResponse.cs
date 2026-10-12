using Domain.Entities;

namespace Application.DataTransferObjects.Responses.Report;

public class FinalizedOutputElectricityChartResponse
{
    public DateTimeOffset? Date { get; set; }
    public string? Unit { get; set; }
    public List<FinalizedOutputElectricityDataPoint> CurrentPeriodData { get; set; } = [];
    public List<PreviousFinalizedOutputElectricityDataPoint> PreviousPeriodData { get; set; } = [];
}

public class FinalizedOutputElectricityDataPoint
{
    public int? Period { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public int? PeriodNumber { get; set; }
    public int? Day { get; set; }
    public int? MonthPrevious { get; set; }
    public int? YearPrevious { get; set; }
    public int? DayPrevious { get; set; }
    public int? PeriodNumberPrevious { get; set; }
    public double? Bt { get; set; }
    public double? Td { get; set; }
    public double? Cd { get; set; }
    public double? Sg { get; set; }
    public double? Vc { get; set; }
}

public class PreviousFinalizedOutputElectricityDataPoint
{
    public int? Period { get; set; }
    public int? Month { get; set; }
    public double? PreviousSg { get; set; }
    public double? PreviousVc { get; set; }
}
