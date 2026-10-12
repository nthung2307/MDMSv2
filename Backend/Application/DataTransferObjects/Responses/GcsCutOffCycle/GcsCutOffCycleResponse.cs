using Domain.Entities;

namespace Application.DataTransferObjects.Responses.GcsCutOffCycle;

public class GcsCutOffCycleResponse
{
    public long id { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int Day { get; set; }
    public int PeriodNumber { get; set; }
    public int PreviousMonth { get; set; }
    public int PreviousYear { get; set; }
    public int PreviousDay { get; set; }
    public int PreviousPeriodNumber { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public GcsEntity Gcs { get; set; }
}