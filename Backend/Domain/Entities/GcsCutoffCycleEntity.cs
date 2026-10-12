using Nobisoft.Core.Domain.Entities;
using VF.Core.Domain.Attributes;

namespace Domain.Entities;

public class GcsCutoffCycleEntity : IEntity<long>
{
    public long Id { get; set; }
    public long GcsId { get; set; }
    public GcsEntity Gcs { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
    public int? PreviousDay { get; set; }
    public int? PreviousMonth { get; set; }
    public int? PreviousYear { get; set; }
    public int? PreviousPeriodNumber { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? CreatedTime { get; set; }
    public long? CreatedBy { get; set; }
    public string? CreatedName { get; set; }
    public List<GcsMonthlyDetailEntity> GcsMonthlyDetail { get; set; } = new List<GcsMonthlyDetailEntity>();
    public bool IsSendingMail { get; set; }
}