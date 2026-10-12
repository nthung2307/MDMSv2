namespace Application.DataTransferObjects.Responses.Report;

public class ListFinalizedOutputElectricityResponse
{
    public long MeasurementPointId { get; set; }
    public string CustomerCode { get; set; }
    public string CustomerName { get; set; }
    public string MeasurementPointCode { get; set; }
    public string MeasurementPointDescription { get; set; }
    public int? Day { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public int? PeriodNumber { get; set; }
    public double? Bt { get; set; }
    public double? Td { get; set; }
    public double? Cd { get; set; }
    public double? Sg { get; set; }
    public double? Vc { get; set; }
}
