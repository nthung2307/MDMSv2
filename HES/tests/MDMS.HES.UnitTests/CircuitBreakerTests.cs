using MDMS.HES.Core.Interfaces;
using MDMS.HES.Infrastructure.Uplink;
using Xunit;

namespace MDMS.HES.UnitTests;

public class CircuitBreakerTests
{
    [Fact]
    public void InitialState_IsClosed_CanPublishTrue()
    {
        var cb = new CircuitBreaker();

        Assert.Equal(CircuitState.Closed, cb.State);
        Assert.True(cb.CanPublish());
        Assert.False(cb.CanProbe());
    }

    [Fact]
    public void RecordFailure_TripsImmediatelyToOpen()
    {
        var cb = new CircuitBreaker { FailureThreshold = 1 };

        cb.RecordFailure("Connection refused");

        Assert.Equal(CircuitState.Open, cb.State);
        Assert.False(cb.CanPublish());
    }

    [Fact]
    public void StateOpen_ProbeSucceeds_TransitionsToHalfOpenThenClosed()
    {
        var cb = new CircuitBreaker
        {
            FailureThreshold = 1,
            SuccessThreshold = 2,
            ProbeInterval = TimeSpan.Zero // Allow immediate probe for unit test
        };

        // 1. Trip
        cb.RecordFailure("Network down");
        Assert.Equal(CircuitState.Open, cb.State);
        Assert.True(cb.CanProbe());

        // 2. First probe succeeds -> HalfOpen
        cb.RecordSuccess();
        Assert.Equal(CircuitState.HalfOpen, cb.State);
        Assert.False(cb.CanPublish());

        // 3. Second probe succeeds -> Closed
        cb.RecordSuccess();
        Assert.Equal(CircuitState.Closed, cb.State);
        Assert.True(cb.CanPublish());
    }

    [Fact]
    public void StateHalfOpen_Fails_ReTripsToOpen()
    {
        var cb = new CircuitBreaker
        {
            FailureThreshold = 1,
            SuccessThreshold = 2,
            ProbeInterval = TimeSpan.Zero
        };

        cb.RecordFailure("Outage");
        cb.RecordSuccess(); // -> HalfOpen
        Assert.Equal(CircuitState.HalfOpen, cb.State);

        cb.RecordFailure("Flapping network");
        Assert.Equal(CircuitState.Open, cb.State);
        Assert.False(cb.CanPublish());
    }
}

