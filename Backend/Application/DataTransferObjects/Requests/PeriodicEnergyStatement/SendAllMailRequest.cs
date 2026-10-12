namespace Application.DataTransferObjects.Requests.PeriodicEnergyStatement;

public class SendAllMailRequest
{
    public long GcsId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
}