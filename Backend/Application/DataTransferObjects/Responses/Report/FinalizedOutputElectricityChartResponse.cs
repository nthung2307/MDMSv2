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
    public decimal? Bt { get; set; }
    public decimal? Td { get; set; }
    public decimal? Cd { get; set; }
    public decimal? Sg { get; set; }
    public decimal? Vc { get; set; }
}

public class PreviousFinalizedOutputElectricityDataPoint
{
    public int? Period { get; set; }
    public int? Month { get; set; }
    public decimal? PreviousSg { get; set; }
    public decimal? PreviousVc { get; set; }
}
