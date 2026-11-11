using System.Data.Common;
using System.Text;
using System.Threading.Channels;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.Services;
using Domain.Entities;
using Domain.Helpers;
using Infrastructure.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Nobisoft.Core.Database.DbContext;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Services;

public sealed class BufferedTelemetryIngestionService : BackgroundService, IBufferedTelemetryIngestionService
{
    private const int ChannelCapacity = 20_000;
    private const int MaxBatchSize = 500;
    private const int SnapshotSqlChunkSize = 25;
    private const int TimeSeriesSqlChunkSize = 50;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan SignalRDebounceInterval = TimeSpan.FromSeconds(2);

    private readonly Channel<TelemetryIngestionItem> _channel;
    private readonly IDbContextFactory<AppReadWriteDbContext> _rwContextFactory;
    private readonly IDbContextFactory<AppReadOnlyDbContext> _roContextFactory;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly ITagRoutingCacheService _tagRoutingCacheService;
    private readonly IHubContext<ListNotificationHub> _listHubContext;
    private readonly IHubContext<DetailNotificationHub> _detailHubContext;
    private readonly IHubContext<DetailNotificationDateTimeHub> _detailDateTimeHubContext;
    private readonly ILoggerService _loggerService;
    private readonly SemaphoreSlim _flushGate = new(1, 1);

    private readonly HashSet<long> _dirtyBroadcastPointIds = new();
    private readonly Dictionary<long, (string Code, MeasurementPointMonitoringByDateTimeDto Dto)> _dirtyDateTimeBroadcasts = new();
    private DateTimeOffset _lastSignalRBroadcastAt = DateTimeOffset.MinValue;

    public BufferedTelemetryIngestionService(
        IDbContextFactory<AppReadWriteDbContext> rwContextFactory,
        IDbContextFactory<AppReadOnlyDbContext> roContextFactory,
        ISnowflakeIdService snowflakeIdService,
        ITagRoutingCacheService tagRoutingCacheService,
        IHubContext<ListNotificationHub> listHubContext,
        IHubContext<DetailNotificationHub> detailHubContext,
        IHubContext<DetailNotificationDateTimeHub> detailDateTimeHubContext,
        ILoggerService loggerService)
    {
        _rwContextFactory = rwContextFactory;
        _roContextFactory = roContextFactory;
        _snowflakeIdService = snowflakeIdService;
        _tagRoutingCacheService = tagRoutingCacheService;
        _listHubContext = listHubContext;
        _detailHubContext = detailHubContext;
        _detailDateTimeHubContext = detailDateTimeHubContext;
        _loggerService = loggerService;

        _channel = Channel.CreateBounded<TelemetryIngestionItem>(new BoundedChannelOptions(ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ValueTask EnqueueAsync(TelemetryIngestionItem item, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(item, cancellationToken);
    }

    public async Task FlushPendingAsync(CancellationToken cancellationToken = default)
    {
        var drained = new List<TelemetryIngestionItem>();
        while (_channel.Reader.TryRead(out var item))
        {
            drained.Add(item);
        }

        await _flushGate.WaitAsync(cancellationToken);
        try
        {
            if (drained.Count > 0)
            {
                await PersistBatchAsync(drained, cancellationToken);
            }

            await TryBroadcastDebouncedAsync(force: true, cancellationToken);
        }
        finally
        {
            _flushGate.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var buffer = new List<TelemetryIngestionItem>(MaxBatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                buffer.Clear();

                using var waitCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                waitCts.CancelAfter(FlushInterval);

                try
                {
                    if (await _channel.Reader.WaitToReadAsync(waitCts.Token))
                    {
                        while (buffer.Count < MaxBatchSize && _channel.Reader.TryRead(out var item))
                        {
                            buffer.Add(item);
                        }

                        if (buffer.Count < MaxBatchSize)
                        {
                            var deadline = DateTimeOffset.UtcNow + FlushInterval;
                            while (buffer.Count < MaxBatchSize && DateTimeOffset.UtcNow < deadline)
                            {
                                using var microCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                                var remaining = deadline - DateTimeOffset.UtcNow;
                                if (remaining <= TimeSpan.Zero) break;
                                microCts.CancelAfter(remaining);

                                if (!await _channel.Reader.WaitToReadAsync(microCts.Token)) break;
                                while (buffer.Count < MaxBatchSize && _channel.Reader.TryRead(out var nextItem))
                                {
                                    buffer.Add(nextItem);
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                {
                    // Normal timer expiry for periodic SignalR debounce check / partial flush
                    while (buffer.Count < MaxBatchSize && _channel.Reader.TryRead(out var extra))
                    {
                        buffer.Add(extra);
                    }
                }

                await _flushGate.WaitAsync(stoppingToken);
                try
                {
                    if (buffer.Count > 0)
                    {
                        await PersistBatchAsync(buffer, stoppingToken);
                    }

                    await TryBroadcastDebouncedAsync(force: false, stoppingToken);
                }
                finally
                {
                    _flushGate.Release();
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _loggerService.LogError(ex, nameof(BufferedTelemetryIngestionService));
                await Task.Delay(500, stoppingToken).ConfigureAwait(false);
            }
        }

        try
        {
            await FlushPendingAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, nameof(BufferedTelemetryIngestionService));
        }
    }

    private async Task PersistBatchAsync(List<TelemetryIngestionItem> batch, CancellationToken cancellationToken)
    {
        var snapshotsByPoint = new Dictionary<long, MeasurementPointCurrentSnapshotEntity>();
        var touchedSpecsSet = new HashSet<SnapshotFieldSpec>();
        var timeSeriesBuckets = new Dictionary<(long PointId, DateTimeOffset BucketUtc), MeasurementPointTimeSeriesEntity>();
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var item in batch)
        {
            if (item.MeasurementPointId <= 0 || item.Samples.Count == 0)
            {
                continue;
            }

            if (!snapshotsByPoint.TryGetValue(item.MeasurementPointId, out var snap))
            {
                snap = new MeasurementPointCurrentSnapshotEntity
                {
                    Id = item.MeasurementPointId,
                    MeasurementPointId = item.MeasurementPointId,
                    UpdatedAt = utcNow
                };
                snapshotsByPoint[item.MeasurementPointId] = snap;
            }

            if (item.SourcePacketType == 1)
            {
                snap.LastTelemetryAt = MaxNullableTime(snap.LastTelemetryAt, item.Timestamp);
            }
            else if (item.SourcePacketType == 2)
            {
                snap.LastStatusAt = MaxNullableTime(snap.LastStatusAt, item.Timestamp);
            }
            else if (item.SourcePacketType == 3)
            {
                snap.LastDateTimeAt = MaxNullableTime(snap.LastDateTimeAt, item.Timestamp);
            }

            foreach (var sample in item.Samples)
            {
                if (!TelemetryMetadataRegistry.ByDescription.TryGetValue(sample.Description, out var spec))
                {
                    continue;
                }

                spec.ApplyToSnapshot(snap, sample);
                touchedSpecsSet.Add(spec);

                if (item.WriteTimeSeries && spec.ApplyToTimeSeries != null)
                {
                    var sampleUtc = sample.Timestamp.ToUniversalTime();
                    var bucketUtc = new DateTimeOffset(
                        sampleUtc.Year,
                        sampleUtc.Month,
                        sampleUtc.Day,
                        sampleUtc.Hour,
                        sampleUtc.Minute,
                        sampleUtc.Second,
                        TimeSpan.Zero);

                    var bucketKey = (item.MeasurementPointId, bucketUtc);
                    if (!timeSeriesBuckets.TryGetValue(bucketKey, out var tsRow))
                    {
                        tsRow = new MeasurementPointTimeSeriesEntity
                        {
                            Id = await _snowflakeIdService.GenerateId(cancellationToken),
                            MeasurementPointId = item.MeasurementPointId,
                            PartitionYm = MeteringValueHelper.ComputePartitionYm(bucketUtc),
                            RecordedAt = bucketUtc,
                            IngestedAt = utcNow,
                            QualityFlag = true,
                            SourcePacketType = item.SourcePacketType
                        };
                        timeSeriesBuckets[bucketKey] = tsRow;
                    }

                    spec.ApplyToTimeSeries(tsRow, sample);
                    if (!sample.Quality)
                    {
                        tsRow.QualityFlag = false;
                    }
                }
            }

            if (item.DateTimeNotification != null && !string.IsNullOrWhiteSpace(item.MeasurementPointCode))
            {
                _dirtyDateTimeBroadcasts[item.MeasurementPointId] = (item.MeasurementPointCode, item.DateTimeNotification);
            }
            else
            {
                _dirtyBroadcastPointIds.Add(item.MeasurementPointId);
            }
        }

        if (snapshotsByPoint.Count == 0 && timeSeriesBuckets.Count == 0)
        {
            return;
        }

        await using var rwContext = await _rwContextFactory.CreateDbContextAsync(cancellationToken);
        var conn = rwContext.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
        {
            await conn.OpenAsync(cancellationToken);
        }

        if (snapshotsByPoint.Count > 0 && touchedSpecsSet.Count > 0)
        {
            var touchedSpecs = touchedSpecsSet.ToList();
            var snapshotList = snapshotsByPoint.Values.ToList();
            for (var i = 0; i < snapshotList.Count; i += SnapshotSqlChunkSize)
            {
                var chunk = snapshotList.Skip(i).Take(SnapshotSqlChunkSize).ToList();
                await ExecuteSnapshotUpsertChunkAsync(conn, chunk, touchedSpecs, cancellationToken);
            }
        }

        if (timeSeriesBuckets.Count > 0)
        {
            var tsList = timeSeriesBuckets.Values.ToList();
            for (var i = 0; i < tsList.Count; i += TimeSeriesSqlChunkSize)
            {
                var chunk = tsList.Skip(i).Take(TimeSeriesSqlChunkSize).ToList();
                await ExecuteTimeSeriesInsertChunkAsync(conn, chunk, cancellationToken);
            }
        }
    }

    private static async Task ExecuteSnapshotUpsertChunkAsync(
        DbConnection conn,
        List<MeasurementPointCurrentSnapshotEntity> chunk,
        List<SnapshotFieldSpec> touchedSpecs,
        CancellationToken cancellationToken)
    {
        var cols = new List<string>(6 + touchedSpecs.Count * 4)
        {
            "Id", "MeasurementPointId", "LastTelemetryAt", "LastStatusAt", "LastDateTimeAt", "UpdatedAt"
        };

        foreach (var spec in touchedSpecs)
        {
            cols.Add(spec.ValueColumn);
            if (spec.UnitColumn != null) cols.Add(spec.UnitColumn);
            cols.Add(spec.QualityColumn);
            if (spec.UpdatedAtColumn != null) cols.Add(spec.UpdatedAtColumn);
        }

        await using var cmd = conn.CreateCommand();
        var sb = new StringBuilder();
        sb.Append("INSERT INTO `MeasurementPointCurrentSnapshots` (");
        sb.Append(string.Join(", ", cols.Select(c => $"`{c}`")));
        sb.Append(") VALUES ");

        var paramIdx = 0;
        for (var r = 0; r < chunk.Count; r++)
        {
            if (r > 0) sb.Append(", ");
            var s = chunk[r];
            sb.Append('(');

            var rowParams = new List<string>(cols.Count);
            AddParam(cmd, ref paramIdx, rowParams, s.Id);
            AddParam(cmd, ref paramIdx, rowParams, s.MeasurementPointId);
            AddParam(cmd, ref paramIdx, rowParams, s.LastTelemetryAt?.UtcDateTime);
            AddParam(cmd, ref paramIdx, rowParams, s.LastStatusAt?.UtcDateTime);
            AddParam(cmd, ref paramIdx, rowParams, s.LastDateTimeAt?.UtcDateTime);
            AddParam(cmd, ref paramIdx, rowParams, s.UpdatedAt.UtcDateTime);

            foreach (var spec in touchedSpecs)
            {
                AddParam(cmd, ref paramIdx, rowParams, spec.GetValue(s));
                if (spec.UnitColumn != null)
                {
                    AddParam(cmd, ref paramIdx, rowParams, spec.GetUnit?.Invoke(s));
                }
                AddParam(cmd, ref paramIdx, rowParams, spec.GetQuality(s));
                if (spec.UpdatedAtColumn != null)
                {
                    var dtVal = spec.GetUpdatedAt?.Invoke(s);
                    AddParam(cmd, ref paramIdx, rowParams, dtVal?.UtcDateTime);
                }
            }

            sb.Append(string.Join(", ", rowParams));
            sb.Append(')');
        }

        sb.Append(" ON DUPLICATE KEY UPDATE ");
        var updates = new List<string>
        {
            "`LastTelemetryAt` = CASE WHEN VALUES(`LastTelemetryAt`) IS NULL THEN `LastTelemetryAt` WHEN `LastTelemetryAt` IS NULL THEN VALUES(`LastTelemetryAt`) WHEN VALUES(`LastTelemetryAt`) > `LastTelemetryAt` THEN VALUES(`LastTelemetryAt`) ELSE `LastTelemetryAt` END",
            "`LastStatusAt` = CASE WHEN VALUES(`LastStatusAt`) IS NULL THEN `LastStatusAt` WHEN `LastStatusAt` IS NULL THEN VALUES(`LastStatusAt`) WHEN VALUES(`LastStatusAt`) > `LastStatusAt` THEN VALUES(`LastStatusAt`) ELSE `LastStatusAt` END",
            "`LastDateTimeAt` = CASE WHEN VALUES(`LastDateTimeAt`) IS NULL THEN `LastDateTimeAt` WHEN `LastDateTimeAt` IS NULL THEN VALUES(`LastDateTimeAt`) WHEN VALUES(`LastDateTimeAt`) > `LastDateTimeAt` THEN VALUES(`LastDateTimeAt`) ELSE `LastDateTimeAt` END",
            "`UpdatedAt` = GREATEST(`UpdatedAt`, VALUES(`UpdatedAt`))"
        };

        foreach (var spec in touchedSpecs)
        {
            if (spec.UpdatedAtColumn != null)
            {
                var u = spec.UpdatedAtColumn;
                var cond = $"(VALUES(`{u}`) IS NOT NULL AND (`{u}` IS NULL OR VALUES(`{u}`) >= `{u}`))";
                updates.Add($"`{spec.ValueColumn}` = IF({cond}, VALUES(`{spec.ValueColumn}`), `{spec.ValueColumn}`)");
                if (spec.UnitColumn != null)
                {
                    updates.Add($"`{spec.UnitColumn}` = IF({cond} AND VALUES(`{spec.UnitColumn}`) IS NOT NULL, VALUES(`{spec.UnitColumn}`), `{spec.UnitColumn}`)");
                }
                updates.Add($"`{spec.QualityColumn}` = IF({cond}, VALUES(`{spec.QualityColumn}`), `{spec.QualityColumn}`)");
                updates.Add($"`{u}` = IF({cond}, VALUES(`{u}`), `{u}`)");
            }
            else
            {
                var q = spec.QualityColumn;
                updates.Add($"`{spec.ValueColumn}` = IF(VALUES(`{q}`) IS NOT NULL, VALUES(`{spec.ValueColumn}`), `{spec.ValueColumn}`)");
                updates.Add($"`{q}` = IF(VALUES(`{q}`) IS NOT NULL, VALUES(`{q}`), `{q}`)");
            }
        }

        sb.Append(string.Join(", ", updates));
        cmd.CommandText = sb.ToString();
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private static readonly string[] TimeSeriesColumns =
    [
        "Id", "MeasurementPointId", "PartitionYm", "RecordedAt", "IngestedAt", "QualityFlag", "SourcePacketType",
        "DongDienPhaA", "DongDienPhaB", "DongDienPhaC",
        "DienApPhaA", "DienApPhaB", "DienApPhaC",
        "CongSuatTacDungPhaA", "CongSuatTacDungPhaB", "CongSuatTacDungPhaC", "CongSuatTacDungTong3Pha",
        "CongSuatPhanKhangPhaA", "CongSuatPhanKhangPhaB", "CongSuatPhanKhangPhaC", "CongSuatPhanKhangTong3Pha",
        "HeSoCongSuatPFPhaA", "HeSoCongSuatPFPhaB", "HeSoCongSuatPFPhaC", "HeSoCongSuatPFTrungBinh3Pha", "TanSoHeThong",
        "ChiSoTichLuyDienNangTacDungAPBieuGioBT", "ChiSoTichLuyDienNangTacDungAPBieuGioCD",
        "ChiSoTichLuyDienNangTacDungAPBieuGioTD", "ChiSoTichLuyDienNangTacDungAPTong", "ChiSoTichLuyDienNangPhanKhangAQTong",
        "ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan", "ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan",
        "ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan", "ChiSoTichLuyDienNangTacDungAPTongChieuNhan", "ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan",
        "ChiSoChotDienNangAPGioBT", "ChiSoChotDienNangAPGioCD", "ChiSoChotDienNangAPGioTD", "ChiSoChotDienNangAPTong", "ChiSoChotDienNangAQTong",
        "ChiSoChotDienNangAPGioBTChieuNhan", "ChiSoChotDienNangAPGioCDChieuNhan", "ChiSoChotDienNangAPGioTDChieuNhan", "ChiSoChotDienNangAPTongChieuNhan", "ChiSoChotDienNangAQTongChieuNhan",
        "PmaxKyHienTai", "PmaxKyTruoc"
    ];

    private static async Task ExecuteTimeSeriesInsertChunkAsync(
        DbConnection conn,
        List<MeasurementPointTimeSeriesEntity> chunk,
        CancellationToken cancellationToken)
    {
        await using var cmd = conn.CreateCommand();
        var sb = new StringBuilder();
        sb.Append("INSERT INTO `MeasurementPointTimeSeries` (");
        sb.Append(string.Join(", ", TimeSeriesColumns.Select(c => $"`{c}`")));
        sb.Append(") VALUES ");

        var paramIdx = 0;
        for (var i = 0; i < chunk.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            var t = chunk[i];
            sb.Append('(');
            var rowParams = new List<string>(TimeSeriesColumns.Length);

            AddParam(cmd, ref paramIdx, rowParams, t.Id);
            AddParam(cmd, ref paramIdx, rowParams, t.MeasurementPointId);
            AddParam(cmd, ref paramIdx, rowParams, t.PartitionYm);
            AddParam(cmd, ref paramIdx, rowParams, t.RecordedAt.UtcDateTime);
            AddParam(cmd, ref paramIdx, rowParams, t.IngestedAt.UtcDateTime);
            AddParam(cmd, ref paramIdx, rowParams, t.QualityFlag);
            AddParam(cmd, ref paramIdx, rowParams, t.SourcePacketType);

            AddParam(cmd, ref paramIdx, rowParams, t.DongDienPhaA);
            AddParam(cmd, ref paramIdx, rowParams, t.DongDienPhaB);
            AddParam(cmd, ref paramIdx, rowParams, t.DongDienPhaC);
            AddParam(cmd, ref paramIdx, rowParams, t.DienApPhaA);
            AddParam(cmd, ref paramIdx, rowParams, t.DienApPhaB);
            AddParam(cmd, ref paramIdx, rowParams, t.DienApPhaC);
            AddParam(cmd, ref paramIdx, rowParams, t.CongSuatTacDungPhaA);
            AddParam(cmd, ref paramIdx, rowParams, t.CongSuatTacDungPhaB);
            AddParam(cmd, ref paramIdx, rowParams, t.CongSuatTacDungPhaC);
            AddParam(cmd, ref paramIdx, rowParams, t.CongSuatTacDungTong3Pha);
            AddParam(cmd, ref paramIdx, rowParams, t.CongSuatPhanKhangPhaA);
            AddParam(cmd, ref paramIdx, rowParams, t.CongSuatPhanKhangPhaB);
            AddParam(cmd, ref paramIdx, rowParams, t.CongSuatPhanKhangPhaC);
            AddParam(cmd, ref paramIdx, rowParams, t.CongSuatPhanKhangTong3Pha);
            AddParam(cmd, ref paramIdx, rowParams, t.HeSoCongSuatPFPhaA);
            AddParam(cmd, ref paramIdx, rowParams, t.HeSoCongSuatPFPhaB);
            AddParam(cmd, ref paramIdx, rowParams, t.HeSoCongSuatPFPhaC);
            AddParam(cmd, ref paramIdx, rowParams, t.HeSoCongSuatPFTrungBinh3Pha);
            AddParam(cmd, ref paramIdx, rowParams, t.TanSoHeThong);

            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoTichLuyDienNangTacDungAPBieuGioBT);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoTichLuyDienNangTacDungAPBieuGioCD);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoTichLuyDienNangTacDungAPBieuGioTD);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoTichLuyDienNangTacDungAPTong);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoTichLuyDienNangPhanKhangAQTong);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoTichLuyDienNangTacDungAPBieuGioBTChieuNhan);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoTichLuyDienNangTacDungAPBieuGioCDChieuNhan);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoTichLuyDienNangTacDungAPBieuGioTDChieuNhan);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoTichLuyDienNangTacDungAPTongChieuNhan);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoTichLuyDienNangPhanKhangAQTongChieuNhan);

            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoChotDienNangAPGioBT);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoChotDienNangAPGioCD);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoChotDienNangAPGioTD);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoChotDienNangAPTong);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoChotDienNangAQTong);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoChotDienNangAPGioBTChieuNhan);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoChotDienNangAPGioCDChieuNhan);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoChotDienNangAPGioTDChieuNhan);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoChotDienNangAPTongChieuNhan);
            AddParam(cmd, ref paramIdx, rowParams, t.ChiSoChotDienNangAQTongChieuNhan);

            AddParam(cmd, ref paramIdx, rowParams, t.PmaxKyHienTai);
            AddParam(cmd, ref paramIdx, rowParams, t.PmaxKyTruoc);

            sb.Append(string.Join(", ", rowParams));
            sb.Append(')');
        }

        cmd.CommandText = sb.ToString();
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task TryBroadcastDebouncedAsync(bool force, CancellationToken cancellationToken)
    {
        if (_dirtyBroadcastPointIds.Count == 0 && _dirtyDateTimeBroadcasts.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (!force && now - _lastSignalRBroadcastAt < SignalRDebounceInterval)
        {
            return;
        }

        _lastSignalRBroadcastAt = now;
        var pointIds = _dirtyBroadcastPointIds.ToList();
        var dtItems = _dirtyDateTimeBroadcasts.Values.ToList();
        _dirtyBroadcastPointIds.Clear();
        _dirtyDateTimeBroadcasts.Clear();

        if (pointIds.Count > 0)
        {
            try
            {
                await using var roContext = await _roContextFactory.CreateDbContextAsync(cancellationToken);
                var pointMap = await _tagRoutingCacheService.GetPointMapAsync(cancellationToken);

                var snapshots = await roContext.Set<MeasurementPointCurrentSnapshotEntity>()
                    .AsNoTracking()
                    .Include(s => s.MeasurementPoint)
                        .ThenInclude(p => p!.Customer)
                    .Where(s => pointIds.Contains(s.MeasurementPointId))
                    .ToListAsync(cancellationToken);

                if (snapshots.Count > 0)
                {
                    var listDtos = new List<ListMeasurementPointMonitoringDto>(snapshots.Count);
                    foreach (var snap in snapshots)
                    {
                        if (snap.MeasurementPoint == null) continue;
                        pointMap.TryGetValue(snap.MeasurementPointId, out var routingInfo);

                        listDtos.Add(SnapshotDtoMapper.ToListDto(snap.MeasurementPoint, snap, routingInfo));

                        if (!string.IsNullOrWhiteSpace(snap.MeasurementPoint.Code))
                        {
                            var detailDto = SnapshotDtoMapper.ToDetailDto(snap.MeasurementPoint, snap, routingInfo);
                            await _detailHubContext.Clients.Group(snap.MeasurementPoint.Code)
                                .SendAsync("ReceiveDetailUpdate", detailDto, cancellationToken);
                        }
                    }

                    if (listDtos.Count > 0)
                    {
                        var deltaResponse = new CursorPaginationResponse<ListMeasurementPointMonitoringDto>
                        {
                            Data = listDtos,
                            TotalCount = pointMap.Count,
                            HasNext = false,
                            HasPrevious = false
                        };
                        await _listHubContext.Clients.All.SendAsync("ReceiveUpdate", deltaResponse, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggerService.LogError(ex, nameof(TryBroadcastDebouncedAsync));
            }
        }

        foreach (var (code, dtDto) in dtItems)
        {
            try
            {
                await _detailDateTimeHubContext.Clients.Group(code)
                    .SendAsync("ReceiveDetailDateTimeUpdate", dtDto, cancellationToken);
            }
            catch (Exception ex)
            {
                _loggerService.LogError(ex, nameof(TryBroadcastDebouncedAsync));
            }
        }
    }

    private static void AddParam(DbCommand cmd, ref int index, List<string> placeholders, object? value)
    {
        var name = $"@p{index++}";
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(param);
        placeholders.Add(name);
    }

    private static DateTimeOffset? MaxNullableTime(DateTimeOffset? current, DateTimeOffset incoming)
    {
        return !current.HasValue || incoming > current.Value ? incoming : current;
    }
}
