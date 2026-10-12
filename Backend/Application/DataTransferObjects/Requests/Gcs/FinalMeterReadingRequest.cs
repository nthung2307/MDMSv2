namespace Application.DataTransferObjects.Requests.Gcs;

public class FinalMeterReadingRequest
{
    public long GcsId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
}