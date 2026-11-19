using MDMS.HES.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MDMS.HES.Infrastructure.Uplink;

public class CircuitBreaker : ICircuitBreaker
{
    private readonly ILogger<CircuitBreaker>? _logger;
    private readonly object _syncRoot = new();

    private CircuitState _state = CircuitState.Closed;
    private int _consecutiveFailures = 0;
    private int _consecutiveSuccesses = 0;
    private DateTime _lastStateChangedAt = DateTime.UtcNow;
    private DateTime _lastProbeAttemptAt = DateTime.MinValue;

    public int FailureThreshold { get; set; } = 1; // Trip immediately on first network error to protect the socket stack
    public int SuccessThreshold { get; set; } = 2; // Require 2 consecutive successful probes to close
    public TimeSpan ProbeInterval { get; set; } = TimeSpan.FromSeconds(5);

    public CircuitState State
    {
        get
        {
            lock (_syncRoot)
            {
                return _state;
            }
        }
    }

    public DateTime LastStateChangedAt
    {
        get
        {
            lock (_syncRoot)
            {
                return _lastStateChangedAt;
            }
        }
    }

    public int ConsecutiveFailures
    {
        get
        {
            lock (_syncRoot)
            {
                return _consecutiveFailures;
            }
        }
    }

    public int ConsecutiveSuccesses
    {
        get
        {
            lock (_syncRoot)
            {
                return _consecutiveSuccesses;
            }
        }
    }

    public CircuitBreaker(ILogger<CircuitBreaker>? logger = null)
    {
        _logger = logger;
    }

    public bool CanPublish()
    {
        lock (_syncRoot)
        {
            return _state == CircuitState.Closed;
        }
    }

    public bool CanProbe()
    {
        lock (_syncRoot)
        {
            if (_state == CircuitState.Closed)
                return false;

            var now = DateTime.UtcNow;
            if (now - _lastProbeAttemptAt >= ProbeInterval)
            {
                _lastProbeAttemptAt = now;
                return true;
            }

            return false;
        }
    }

    public void RecordSuccess()
    {
        lock (_syncRoot)
        {
            _consecutiveFailures = 0;
            _consecutiveSuccesses++;

            if (_state == CircuitState.Open)
            {
                _state = CircuitState.HalfOpen;
                _lastStateChangedAt = DateTime.UtcNow;
                _logger?.LogInformation("[CircuitBreaker] State -> HALF-OPEN (First probe succeeded, verifying stability)");
            }
            else if (_state == CircuitState.HalfOpen && _consecutiveSuccesses >= SuccessThreshold)
            {
                _state = CircuitState.Closed;
                _lastStateChangedAt = DateTime.UtcNow;
                _consecutiveSuccesses = 0;
                _logger?.LogInformation("[CircuitBreaker] State -> CLOSED (Network connection fully restored, resuming all meter publishers)");
            }
        }
    }

    public void RecordFailure(string? reason = null)
    {
        lock (_syncRoot)
        {
            _consecutiveSuccesses = 0;
            _consecutiveFailures++;

            if (_state == CircuitState.Closed && _consecutiveFailures >= FailureThreshold)
            {
                Trip(reason);
            }
            else if (_state == CircuitState.HalfOpen)
            {
                Trip(reason ?? "Probe failed during Half-Open evaluation");
            }
        }
    }

    public void Trip(string? reason = null)
    {
        lock (_syncRoot)
        {
            _state = CircuitState.Open;
            _lastStateChangedAt = DateTime.UtcNow;
            _consecutiveSuccesses = 0;
            _lastProbeAttemptAt = DateTime.UtcNow; // Start waiting probe interval
            _logger?.LogWarning("[CircuitBreaker] State -> OPEN (Outage detected: {Reason}). Pausing all normal meter publishes. Single probe every {Interval}s.",
                reason ?? "Network unreachable", ProbeInterval.TotalSeconds);
        }
    }

    public void Reset()
    {
        lock (_syncRoot)
        {
            _state = CircuitState.Closed;
            _consecutiveFailures = 0;
            _consecutiveSuccesses = 0;
            _lastStateChangedAt = DateTime.UtcNow;
            _logger?.LogInformation("[CircuitBreaker] Manually reset to CLOSED.");
        }
    }
}
