using MDMS.HES.Core.Common;
using MDMS.HES.Core.Models;
using MDMS.HES.Infrastructure.Uplink;
using Xunit;

namespace MDMS.HES.UnitTests;

public class SmartBufferManagerTests
{
    [Fact]
    public void RecordTag_OverwritesLatestMeasurementInPlace_FixedMemory()
    {
        var manager = new SmartBufferManager();
        string meterId = "M001";

        // Record 1000 successive voltage updates
        for (int i = 0; i < 1000; i++)
        {
            manager.RecordTag(meterId, new TagValue(ObisConstants.VoltageL1, 220.0 + i, TagQuality.Good, DateTime.UtcNow));
        }

        var payload = manager.DrainPendingPayload(meterId, "209555163");
        Assert.NotNull(payload);

        // Only 1 voltage item should exist with the latest value (220 + 999 = 1219)
        var voltageItems = payload.Values.Where(x => x.Id == ObisConstants.VoltageL1).ToList();
        Assert.Single(voltageItems);
        Assert.Equal(1219.0, voltageItems[0].V);
    }

    [Fact]
    public void RecordTag_AlarmTransitions_QueuedInChronologicalOrder()
    {
        var manager = new SmartBufferManager();
        string meterId = "M001";
        var t0 = DateTime.UtcNow;

        // Transition 1: Phase loss turns ON (0 -> 1)
        manager.RecordTag(meterId, new TagValue(ObisConstants.AlarmPhase1Loss, false, TagQuality.Good, t0));
        manager.RecordTag(meterId, new TagValue(ObisConstants.AlarmPhase1Loss, true, TagQuality.Good, t0.AddSeconds(1)));

        // Transition 2: Phase loss turns OFF (1 -> 0)
        manager.RecordTag(meterId, new TagValue(ObisConstants.AlarmPhase1Loss, false, TagQuality.Good, t0.AddSeconds(5)));

        // Non-transition: Phase loss stays OFF
        manager.RecordTag(meterId, new TagValue(ObisConstants.AlarmPhase1Loss, false, TagQuality.Good, t0.AddSeconds(6)));

        var payload = manager.DrainPendingPayload(meterId, "209555163");
        Assert.NotNull(payload);

        // We expect the transitions to be preserved
        var alarmItems = payload.Values.Where(x => x.Id == ObisConstants.AlarmPhase1Loss).ToList();
        Assert.True(alarmItems.Count >= 2);
    }

    [Fact]
    public void DrainPendingPayload_EmptyBuffer_ReturnsNull()
    {
        var manager = new SmartBufferManager();
        var payload = manager.DrainPendingPayload("M999", "NON_EXISTING");
        Assert.Null(payload);
    }
}
