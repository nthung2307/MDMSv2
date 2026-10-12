namespace Application.DataTransferObjects.Responses.GcsCutOffCycle;

public class GcsCutOffCycleDto
{
    public long Id { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
    public long GcsId { get; set; }
}