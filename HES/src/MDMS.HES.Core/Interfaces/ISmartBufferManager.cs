using MDMS.HES.Core.Models;

namespace MDMS.HES.Core.Interfaces;

public interface ISmartBufferManager
{
    void RecordTag(string meterId, TagValue tag);
    TelemetryPayload? DrainPendingPayload(string meterId, string serial, int maxEvents = 150);
    int GetPendingAlarmCount(string meterId);
    int GetCachedMeterCount();
    void Clear();
}
