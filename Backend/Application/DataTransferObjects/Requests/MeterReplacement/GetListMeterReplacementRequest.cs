using System;
using Application.Common.ValueObjects.OffsetBased;

namespace Application.DataTransferObjects.Requests.MeterReplacement;

public class GetListMeterReplacementRequest : OffsetPaginationRequest
{
    public long? MeasurementPointId { get; set; }
    public string? PointCode { get; set; }
    public string? CustomerName { get; set; }
    public string? Status { get; set; } // DRAFT, APPROVED, CANCELLED
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }
    public string? Keyword { get; set; }
}

