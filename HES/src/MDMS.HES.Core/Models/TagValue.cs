namespace MDMS.HES.Core.Models;

public enum TagQuality
{
    Good = 0,
    Bad = 1,
    Uncertain = 2
}

public class TagValue
{
    public string Id { get; set; } = string.Empty;
    public object? Value { get; set; }
    public TagQuality Quality { get; set; } = TagQuality.Good;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public long TimestampEpochMs
    {
        get
        {
            var utc = Timestamp.Kind switch
            {
                DateTimeKind.Utc => Timestamp,
                DateTimeKind.Local => Timestamp.ToUniversalTime(),
                _ => DateTime.SpecifyKind(Timestamp, DateTimeKind.Utc)
            };
            return new DateTimeOffset(utc, TimeSpan.Zero).ToUnixTimeMilliseconds();
        }
    }

    public string QualityString => Quality == TagQuality.Good ? "true" : "false";

    public TagValue() { }

    public TagValue(string id, object? value, TagQuality quality = TagQuality.Good, DateTime? timestamp = null)
    {
        Id = id;
        Value = value;
        Quality = quality;
        Timestamp = timestamp ?? DateTime.UtcNow;
    }

    public override string ToString() => $"{Id} = {Value} [{Quality}] @ {Timestamp:yyyy-MM-dd HH:mm:ss.fff}";
}
