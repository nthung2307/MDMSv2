namespace Application.Services;

/// <summary>
/// Manages monthly RANGE partitions on MeasurementPointTimeSeries (pre-creation and retention pruning).
/// </summary>
public interface IPartitionManagementService
{
    Task EnsureAndCleanupPartitionsAsync(int monthsAhead = 3, int retentionMonths = 12, CancellationToken cancellationToken = default);
}

