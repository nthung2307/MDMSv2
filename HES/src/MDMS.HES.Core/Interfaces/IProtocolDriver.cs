using MDMS.HES.Core.Models;

namespace MDMS.HES.Core.Interfaces;

public interface IProtocolDriver
{
    string DriverName { get; }
    bool IsConnected { get; }

    Task InitializeAsync(IEnumerable<MeterConfig> meters, CancellationToken cancellationToken);
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);

    event Action<string, TagValue>? OnTagChanged;

    IReadOnlyDictionary<string, TagValue> GetLatestTags(string meterId);
}
