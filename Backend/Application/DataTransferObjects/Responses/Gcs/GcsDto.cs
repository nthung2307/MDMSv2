using Domain.Entities;

namespace Application.DataTransferObjects.Responses.Gcs;

public class GcsDto
{
    public long Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int PeriodNumber { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; }
    public List<MeasurementPointsEntity> MeasurementPoints { get; set; } = new List<MeasurementPointsEntity>();
    public List<int> DayPeriod { get; set; } = new List<int>();
    public DateTimeOffset? CreatedTime { get; set; }
    public bool IsNewChange { get; set; }
}