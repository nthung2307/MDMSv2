using Application.Helpers;

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
    public string? ObisCode { get; init; }
    public ObisCompositeDateTimeTarget CompositeDateTime { get; init; } = ObisCompositeDateTimeTarget.None;
    public bool IsAutoMapped => TagNameId <= 0;
}

public sealed class MeterProfileCachedTag
{
    public string ObisCode { get; init; } = string.Empty;
    public string ParameterCode { get; init; } = string.Empty;
    public string? Unit { get; init; }
    public string? CustomName { get; init; }
    public string? LegacyAliases { get; init; }
    public string DataCategory { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
}

public sealed class MeterProfileCachedInfo
{
    public long Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string VoltageLevel { get; init; } = "LV";
    public string DefaultVoltageUnit { get; init; } = "V";
    public string DefaultPowerUnit { get; init; } = "kW";
    public bool IsDefault { get; init; }
    public bool IsSystem { get; init; }
    public IReadOnlyList<MeterProfileCachedTag> TagMappings { get; init; } = Array.Empty<MeterProfileCachedTag>();
}

public sealed class PointRoutingInfo
{
    public long MeasurementPointId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? CustomerCode { get; init; }
    public string? CustomerName { get; init; }
    public string? Status { get; init; }
    public string? MeterSerial { get; init; }
    public string? ApiKeyHash { get; init; }
    public string? KeyPrefix { get; init; }
    public long? MeterProfileId { get; init; }
    public string? MeterProfileCode { get; init; }
    public string? MeterProfileVoltageUnit { get; init; }
    public string MappingMode { get; init; } = "HYBRID";

    // Effective mapping per MDMS ParameterCode (Baseline MeterProfile + manual TagName overrides)
    public Dictionary<string, TagRoutingEntry> TagsByDescription { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    // Tier 1: Manual TagName overrides indexed by raw TagName string on this point
    public Dictionary<string, TagRoutingEntry> ManualTagsByRawId { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    // Tier 2a: Canonical OBIS codes mapped from assigned/default MeterProfile
    public Dictionary<string, TagRoutingEntry> ProfileObisMap { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    // Tier 2b: Normalized Kepware legacy suffixes mapped from assigned/default MeterProfile
    public Dictionary<string, TagRoutingEntry> ProfileAliasMap { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Provides O(1) cached resolution of IoT tag names, MeterSerial routing, MeterProfiles, and OBIS mappings.
/// </summary>
public interface ITagRoutingCacheService
{
    Task<IReadOnlyDictionary<string, TagRoutingEntry>> GetTagMapAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<long, PointRoutingInfo>> GetPointMapAsync(CancellationToken cancellationToken = default);
    Task<PointRoutingInfo?> GetPointBySerialAsync(string meterSerial, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MeterProfileCachedInfo>> GetMeterProfilesAsync(CancellationToken cancellationToken = default);
    TagRoutingEntry? ResolveTagForPoint(PointRoutingInfo point, string rawTagId);
    void Invalidate();
}
