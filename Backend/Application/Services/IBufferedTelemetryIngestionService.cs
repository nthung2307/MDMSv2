using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;

namespace Application.Services;

public sealed class TelemetryTagSample
{
    public string Description { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public decimal? NumericValue { get; init; }
    public short? SmallIntValue { get; init; }
    public bool? BoolValue { get; init; }
    public bool Quality { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public sealed class TelemetryIngestionItem
{
    public long MeasurementPointId { get; init; }
    public string MeasurementPointCode { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
    public bool WriteTimeSeries { get; init; }
    public byte SourcePacketType { get; init; } = 1;
    public List<TelemetryTagSample> Samples { get; init; } = new();
    public MeasurementPointMonitoringByDateTimeDto? DateTimeNotification { get; init; }
}

/// <summary>
/// High-throughput micro-batching ingestion pipeline backed by BoundedChannel and bulk MySQL writes.
/// </summary>
public interface IBufferedTelemetryIngestionService
{
    ValueTask EnqueueAsync(TelemetryIngestionItem item, CancellationToken cancellationToken = default);
    Task FlushPendingAsync(CancellationToken cancellationToken = default);
}

