using Domain.Entities;

namespace Application.DataTransferObjects.Responses.PeriodicEnergyStatement;

public class PeriodicEnergyStatementDto
{
    public long Id { get; set; }
    public long GcsId { get; set; }
    public long MeasurementPointId { get; set; }
    public MeasurementPointsEntity MeasurementPoint { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public int PeriodNumber { get; set; }
    public decimal Bt { get; set; }
    public decimal Cd { get; set; }
    public decimal Td { get; set; }
    public decimal Sg { get; set; }
    public decimal Vc { get; set; }
    public decimal Pmax { get; set; }
    public decimal PreviousBt { get; set; }
    public decimal PreviousCd { get; set; }
    public decimal PreviousTd { get; set; }
    public decimal PreviousSg { get; set; }
    public decimal PreviousVc { get; set; }
    public decimal PreviousPmax { get; set; }
    public decimal ConsumtionBt { get; set; }
    public decimal ConsumtionCd { get; set; }
    public decimal ConsumtionTd { get; set; }
    public decimal ConsumtionSg { get; set; }
    public decimal ConsumtionVc { get; set; }
    public decimal? CosPhi { get; set; }
    public int? PreviousDay { get; set; }
    public int? PreviousMonth { get; set; }
    public int? PreviousYear { get; set; }
    public int? PreviousPeriodNumber { get; set; }
    public int? Day { get; set; }
    public string? Status { get; set; }
    public decimal Multiplier { get; set; }
    public string SerialNumber { get; set; }
    public DateTimeOffset? SentTime { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset? PmaxTime { get; set; }
    public DateTimeOffset? PreviousPmaxTime { get; set; }
    public DateTimeOffset? BillingTimestamp { get; set; }
}
