namespace MDMS.HES.Core.Interfaces;

public enum CircuitState
{
    Closed,     // Normal operations: all meters allowed to publish
    Open,       // Outage detected: all normal publishers paused, only single probe active
    HalfOpen    // Probe succeeded: attempting recovery
}

public interface ICircuitBreaker
{
    CircuitState State { get; }
    DateTime LastStateChangedAt { get; }
    int ConsecutiveFailures { get; }
    int ConsecutiveSuccesses { get; }

    bool CanPublish();
    bool CanProbe();
    void RecordSuccess();
    void RecordFailure(string? reason = null);
    void Trip(string? reason = null);
    void Reset();
}

