namespace Nobisoft.Core.Domain.ValueObjects;

/// <summary>
/// Event history
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class EventHistoryItem<TKey>
{
    public string EventType { get; set; } = string.Empty;
    public long EventRevision { get; set; }
    public TKey CreatedBy { get; set; }
}