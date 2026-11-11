using Application.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Database.DbContext;

namespace Infrastructure.Services;

public sealed class TagRoutingCacheService : ITagRoutingCacheService
{
    private const string CacheKey = "mdms:phase1:tag-routing-snapshot:v1";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private sealed record CachedRoutingState(
        IReadOnlyDictionary<string, TagRoutingEntry> TagMap,
        IReadOnlyDictionary<long, PointRoutingInfo> PointMap);

    private readonly IMemoryCache _memoryCache;
    private readonly IDbContextFactory<AppReadOnlyDbContext> _dbContextFactory;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private long _cacheVersion;

    public TagRoutingCacheService(
        IMemoryCache memoryCache,
        IDbContextFactory<AppReadOnlyDbContext> dbContextFactory)
    {
        _memoryCache = memoryCache;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyDictionary<string, TagRoutingEntry>> GetTagMapAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrBuildStateAsync(cancellationToken);
        return state.TagMap;
    }

    public async Task<IReadOnlyDictionary<long, PointRoutingInfo>> GetPointMapAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrBuildStateAsync(cancellationToken);
        return state.PointMap;
    }

    public void Invalidate()
    {
        Interlocked.Increment(ref _cacheVersion);
        _memoryCache.Remove(CacheKey);
    }

    private async Task<CachedRoutingState> GetOrBuildStateAsync(CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue<CachedRoutingState>(CacheKey, out var cached) && cached != null)
        {
            return cached;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_memoryCache.TryGetValue<CachedRoutingState>(CacheKey, out cached) && cached != null)
            {
                return cached;
            }

            var capturedVersion = Volatile.Read(ref _cacheVersion);

            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var points = await context.Set<MeasurementPointsEntity>()
                .AsNoTracking()
                .Include(p => p.Customer)
                .Where(p => !p.Deleted)
                .OrderByDescending(p => p.Status == EntityStatus.Active)
                .ThenBy(p => p.Id)
                .Select(p => new
                {
                    p.Id,
                    p.Code,
                    p.Description,
                    p.Status,
                    CustomerCode = p.Customer != null ? p.Customer.CustomerCode : null,
                    CustomerName = p.Customer != null ? p.Customer.FullName : null
                })
                .ToListAsync(cancellationToken);

            var pointMap = new Dictionary<long, PointRoutingInfo>(points.Count);
            foreach (var p in points)
            {
                pointMap[p.Id] = new PointRoutingInfo
                {
                    MeasurementPointId = p.Id,
                    Code = p.Code ?? string.Empty,
                    Description = p.Description,
                    CustomerCode = p.CustomerCode,
                    CustomerName = p.CustomerName,
                    Status = p.Status,
                    TagsByDescription = new Dictionary<string, TagRoutingEntry>(StringComparer.OrdinalIgnoreCase)
                };
            }

            var tags = await context.Set<TagNameEntity>()
                .AsNoTracking()
                .Where(t => !t.Deleted && !string.IsNullOrWhiteSpace(t.TagName))
                .OrderBy(t => t.Id)
                .Select(t => new
                {
                    t.Id,
                    t.MeasurementPointId,
                    t.TagName,
                    t.Description,
                    t.Unit,
                    t.CustomName,
                    t.Type
                })
                .ToListAsync(cancellationToken);

            var tagMap = new Dictionary<string, TagRoutingEntry>(tags.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var t in tags)
            {
                if (!pointMap.TryGetValue(t.MeasurementPointId, out var pointInfo))
                {
                    continue;
                }

                var entry = new TagRoutingEntry
                {
                    TagNameId = t.Id,
                    MeasurementPointId = pointInfo.MeasurementPointId,
                    MeasurementPointCode = pointInfo.Code,
                    MeasurementPointDescription = pointInfo.Description,
                    CustomerCode = pointInfo.CustomerCode,
                    CustomerName = pointInfo.CustomerName,
                    PointStatus = pointInfo.Status,
                    TagName = t.TagName!.Trim(),
                    Description = t.Description ?? string.Empty,
                    Unit = t.Unit,
                    CustomName = t.CustomName,
                    Type = t.Type ?? string.Empty
                };

                // Prefer ACTIVE measurement point if duplicate TagName string exists in legacy config
                if (!tagMap.TryGetValue(entry.TagName, out var existing) ||
                    existing.PointStatus != EntityStatus.Active && entry.PointStatus == EntityStatus.Active)
                {
                    tagMap[entry.TagName] = entry;
                }

                if (!string.IsNullOrWhiteSpace(entry.Description))
                {
                    pointInfo.TagsByDescription[entry.Description] = entry;
                }
            }

            var built = new CachedRoutingState(tagMap, pointMap);
            if (Volatile.Read(ref _cacheVersion) == capturedVersion)
            {
                _memoryCache.Set(CacheKey, built, CacheDuration);
            }

            return built;
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}

