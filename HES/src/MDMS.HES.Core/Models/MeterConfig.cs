namespace MDMS.HES.Core.Models;

public enum TagCategory
{
    Measurement,
    Energy,
    StatusAlarm,
    DateTimePart,
    ObisDateTimeString,
    Other
}

public class MeterConfig
{
    public string MeterId { get; set; } = string.Empty;
    public string Serial { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public int ScanRateMs { get; set; } = 60000;
}

