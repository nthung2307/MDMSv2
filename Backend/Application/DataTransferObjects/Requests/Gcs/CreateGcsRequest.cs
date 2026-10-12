namespace Application.DataTransferObjects.Requests.Gcs;

public class CreateGcsRequest
{
    public string Code { get; set; }
    public string Name { get; set; }
    public int PeriodNumber { get; set; }
    public string? Description { get; set; }
    public List<int> DayPeriod { get; set; } = new List<int>();
    public string Status { get; set; }
}