namespace Application.DataTransferObjects.Responses.IoT;

public sealed class MeterProfileTagMappingDto
{
    public string ObisCode { get; set; } = string.Empty;
    public string ParameterCode { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? CustomName { get; set; }
    public string? LegacyAliases { get; set; }
    public string DataCategory { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public sealed class MeterProfileDto
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string VoltageLevel { get; set; } = "LV";
    public string DefaultVoltageUnit { get; set; } = "V";
    public string DefaultPowerUnit { get; set; } = "kW";
    public bool IsDefault { get; set; }
    public bool IsSystem { get; set; }
    public IReadOnlyList<MeterProfileTagMappingDto> TagMappings { get; set; } = Array.Empty<MeterProfileTagMappingDto>();
}

public sealed class PointApiKeyDto
{
    public long MeasurementPointId { get; set; }
    public string PointCode { get; set; } = string.Empty;
    public string? MeterSerial { get; set; }
    public bool HasDedicatedKey { get; set; }
    public string? KeyPrefix { get; set; }
    public string? ApiKey { get; set; }
    public string AuthTier { get; set; } = "TIER_2_GLOBAL";
}

public sealed class UnmatchedSerialDto
{
    public string MeterSerial { get; set; } = string.Empty;
    public string RemoteIp { get; set; } = string.Empty;
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastSeenAt { get; set; }
    public long HitCount { get; set; }
}

