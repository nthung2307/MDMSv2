using System.Text.Json.Serialization;

namespace MDMS.HES.Core.Models;

public class TelemetryPayload
{
    [JsonPropertyName("meter")]
    public string? Meter { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("values")]
    public List<TelemetryValueItem> Values { get; set; } = new();
}

public class TelemetryValueItem
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("v")]
    public object? V { get; set; }

    [JsonPropertyName("q")]
    public string? Q { get; set; }

    [JsonPropertyName("t")]
    public long T { get; set; }

    public TelemetryValueItem() { }

    public TelemetryValueItem(string id, object? v, string? q, long t)
    {
        Id = id;
        V = v;
        Q = q;
        T = t;
    }
}
