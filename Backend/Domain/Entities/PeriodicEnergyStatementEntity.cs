using System.ComponentModel.DataAnnotations.Schema;
using Nobisoft.Core.Domain.Entities;

namespace Domain.Entities;

public class PeriodicEnergyStatementEntity : IEntity<long>
{
    public long Id { get; set; }
    public long GcsId { get; set; }
    public GcsEntity Gcs { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
    public double Bt { get; set; }
    public double Cd { get; set; }
    public double Td { get; set; }
    public double Sg { get; set; }
    public double Vc { get; set; }
    public double Pmax { get; set; }
    public double PreviousBt { get; set; }
    public double PreviousCd { get; set; }
    public double PreviousTd { get; set; }
    public double PreviousSg { get; set; }
    public double PreviousVc { get; set; }
    public double PreviousPmax { get; set; }
    public double ConsumtionBt { get; set; }
    public double ConsumtionCd { get; set; }
    public double ConsumtionTd { get; set; }
    public double ConsumtionSg { get; set; }
    public double ConsumtionVc { get; set; }
    public int? PreviousDay { get; set; }
    public int? PreviousMonth { get; set; }
    public int? PreviousYear { get; set; }
    public int? PreviousPeriodNumber { get; set; }
    public int? Day { get; set; }
    public string? Status { get; set; }
    public double Multiplier { get; set; }
    public string SerialNumber { get; set; }
    public DateTimeOffset? SentTime { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}