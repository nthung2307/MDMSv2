namespace Application.Services;

public sealed class TagRoutingEntry
{
    public long TagNameId { get; init; }
    public long MeasurementPointId { get; init; }
    public string MeasurementPointCode { get; init; } = string.Empty;
    public string? MeasurementPointDescription { get; init; }
    public string? CustomerCode { get; init; }
    public string? CustomerName { get; init; }
    public string? PointStatus { get; init; }
    public string TagName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public string? CustomName { get; init; }
    public string Type { get; init; } = string.Empty;
}

public sealed class PointRoutingInfo
{
    public long MeasurementPointId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CustomerCode { get; init; }
    public string? CustomerName { get; init; }
    public string? Status { get; init; }
    public Dictionary<string, TagRoutingEntry> TagsByDescription { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Provides O(1) cached resolution of IoT tag names and measurement point metadata.
/// </summary>
public interface ITagRoutingCacheService
{
    Task<IReadOnlyDictionary<string, TagRoutingEntry>> GetTagMapAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<long, PointRoutingInfo>> GetPointMapAsync(CancellationToken cancellationToken = default);
    void Invalidate();
}

