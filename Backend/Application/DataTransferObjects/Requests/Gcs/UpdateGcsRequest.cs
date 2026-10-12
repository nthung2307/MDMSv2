namespace Application.DataTransferObjects.Requests.Gcs;

public class UpdateGcsRequest
{
    public long Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int PeriodNumber { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
    public List<int> DayPeriod { get; set; } = new List<int>();
}