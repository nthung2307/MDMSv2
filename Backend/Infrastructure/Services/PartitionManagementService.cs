using System.Text.RegularExpressions;
using Application.Services;
using Domain.Helpers;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Services;

/// <summary>
/// Maintains monthly RANGE partitions on MeasurementPointTimeSeries:
/// creates partitions ahead (+3 months by default) and drops partitions older than 12 months.
/// </summary>
public sealed class PartitionManagementService : IPartitionManagementService
{
    private const string TableName = "MeasurementPointTimeSeries";
    private const string MaxPartitionName = "p_max";
    private static readonly Regex PartitionNameRegex = new(@"^p_(?<ym>\d{6})$", RegexOptions.Compiled);

    private readonly IDbContextFactory<AppReadWriteDbContext> _dbContextFactory;
    private readonly ILoggerService _loggerService;

    public PartitionManagementService(
        IDbContextFactory<AppReadWriteDbContext> dbContextFactory,
        ILoggerService loggerService)
    {
        _dbContextFactory = dbContextFactory;
        _loggerService = loggerService;
    }

    public async Task EnsureAndCleanupPartitionsAsync(
        int monthsAhead = 3,
        int retentionMonths = 12,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var conn = context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
        }

        var existingPartitions = new List<(string Name, string Description)>();
        await using (var queryCmd = conn.CreateCommand())
        {
            queryCmd.CommandText = @"
                SELECT PARTITION_NAME, PARTITION_DESCRIPTION
                FROM INFORMATION_SCHEMA.PARTITIONS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND LOWER(TABLE_NAME) = LOWER(@tableName)
                  AND PARTITION_NAME IS NOT NULL
                ORDER BY PARTITION_ORDINAL_POSITION;";
            var p = queryCmd.CreateParameter();
            p.ParameterName = "@tableName";
            p.Value = TableName;
            queryCmd.Parameters.Add(p);

            await using var reader = await queryCmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var pName = reader.GetString(0);
                var pDesc = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                existingPartitions.Add((pName, pDesc));
            }
        }

        if (existingPartitions.Count == 0)
        {
            // Table is not partitioned yet (e.g. dev/clean DB before migration script ran)
            return;
        }

        var existingNames = existingPartitions.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var hasMaxPartition = existingPartitions.Any(x =>
            string.Equals(x.Name, MaxPartitionName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.Description, "MAXVALUE", StringComparison.OrdinalIgnoreCase));

        var maxExistingBound = 0;
        foreach (var (_, desc) in existingPartitions)
        {
            if (int.TryParse(desc, out var bound) && bound > maxExistingBound)
            {
                maxExistingBound = bound;
            }
        }

        var vnNow = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        var currentMonthStart = new DateTimeOffset(vnNow.Year, vnNow.Month, 1, 0, 0, 0, vnNow.Offset);
        var partitionsToAdd = new List<(string Name, int LessThanYm)>();

        for (var offset = 0; offset <= monthsAhead; offset++)
        {
            var targetMonth = currentMonthStart.AddMonths(offset);
            var targetYm = targetMonth.Year * 100 + targetMonth.Month;
            var nextMonth = targetMonth.AddMonths(1);
            var nextYm = nextMonth.Year * 100 + nextMonth.Month;
            var partName = $"p_{targetYm}";

            if (!existingNames.Contains(partName) && nextYm > maxExistingBound)
            {
                partitionsToAdd.Add((partName, nextYm));
                maxExistingBound = nextYm;
            }
        }

        if (partitionsToAdd.Count > 0)
        {
            var clauses = partitionsToAdd.Select(p => $"PARTITION `{p.Name}` VALUES LESS THAN ({p.LessThanYm})");
            string alterAddSql;
            if (hasMaxPartition)
            {
                alterAddSql = $@"
                    ALTER TABLE `{TableName}`
                    REORGANIZE PARTITION `{MaxPartitionName}` INTO (
                        {string.Join(",\n                        ", clauses)},
                        PARTITION `{MaxPartitionName}` VALUES LESS THAN MAXVALUE
                    );";
            }
            else
            {
                alterAddSql = $@"
                    ALTER TABLE `{TableName}`
                    ADD PARTITION (
                        {string.Join(",\n                        ", clauses)}
                    );";
            }

            await using var addCmd = conn.CreateCommand();
            addCmd.CommandText = alterAddSql;
            await addCmd.ExecuteNonQueryAsync(cancellationToken);
            _loggerService.LogInformation($"[PartitionMaintenance] Added {partitionsToAdd.Count} monthly partition(s) to {TableName}.");
        }

        var cutoffYm = MeteringValueHelper.GetRetentionCutoffPartitionYm(DateTimeOffset.UtcNow, retentionMonths);
        var partitionsToDrop = new List<string>();
        foreach (var (name, _) in existingPartitions)
        {
            var match = PartitionNameRegex.Match(name);
            if (!match.Success) continue;

            if (int.TryParse(match.Groups["ym"].Value, out var partYm) && partYm < cutoffYm)
            {
                partitionsToDrop.Add(name);
            }
        }

        // Keep at least one data/max partition intact
        if (partitionsToDrop.Count > 0 && partitionsToDrop.Count < existingPartitions.Count)
        {
            var dropList = string.Join(", ", partitionsToDrop.Select(n => $"`{n}`"));
            await using var dropCmd = conn.CreateCommand();
            dropCmd.CommandText = $"ALTER TABLE `{TableName}` DROP PARTITION {dropList};";
            await dropCmd.ExecuteNonQueryAsync(cancellationToken);
            _loggerService.LogInformation($"[PartitionMaintenance] Dropped expired partition(s) (< {cutoffYm}): {dropList}.");
        }
    }
}
