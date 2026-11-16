using Application.Common;
using Application.Helpers;
using Application.Services;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Database.DbContext;

namespace Infrastructure.Services;

public sealed class TagRoutingCacheService : ITagRoutingCacheService
{
    private const string CacheKey = "mdms:phase2:tag-routing-snapshot:v2";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private sealed record CachedRoutingState(
        IReadOnlyDictionary<string, TagRoutingEntry> TagMap,
        IReadOnlyDictionary<long, PointRoutingInfo> PointMap,
        IReadOnlyDictionary<string, PointRoutingInfo> SerialMap,
        IReadOnlyList<MeterProfileCachedInfo> MeterProfiles);

    private readonly IMemoryCache _memoryCache;
    private readonly IDbContextFactory<AppReadOnlyDbContext> _dbContextFactory;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private volatile CachedRoutingState? _lastKnownState;
    private long _cacheVersion;
    private long _loadedVersion = -1;
    private int _backgroundRefreshRunning;

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

    public async Task<PointRoutingInfo?> GetPointBySerialAsync(string meterSerial, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(meterSerial)) return null;
        var trimmed = meterSerial.Trim();
        var state = await GetOrBuildStateAsync(cancellationToken);
        if (state.SerialMap.TryGetValue(trimmed, out var point))
        {
            // If cache was invalidated (e.g., key rotated on this point), ensure we have the latest version
            if (Volatile.Read(ref _loadedVersion) == Volatile.Read(ref _cacheVersion))
            {
                return point;
            }

            var freshState = await ForceRebuildStateAsync(cancellationToken);
            return freshState.SerialMap.TryGetValue(trimmed, out var freshPoint) ? freshPoint : point;
        }

        if (Volatile.Read(ref _loadedVersion) != Volatile.Read(ref _cacheVersion))
        {
            var freshState = await ForceRebuildStateAsync(cancellationToken);
            return freshState.SerialMap.TryGetValue(trimmed, out var freshPoint) ? freshPoint : null;
        }

        return null;
    }

    public async Task<IReadOnlyList<MeterProfileCachedInfo>> GetMeterProfilesAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrBuildStateAsync(cancellationToken);
        return state.MeterProfiles;
    }

    public TagRoutingEntry? ResolveTagForPoint(PointRoutingInfo point, string rawTagId)
    {
        if (point == null || string.IsNullOrWhiteSpace(rawTagId))
            return null;

        var trimmedId = rawTagId.Trim();
        var mode = Constants.MappingMode.NormalizeOrDefault(point.MappingMode);

        // Layer 1: Manual TagName override for this point (HYBRID or MANUAL mode)
        if (mode != Constants.MappingMode.Auto &&
            point.ManualTagsByRawId.TryGetValue(trimmedId, out var manualEntry) &&
            !string.IsNullOrWhiteSpace(manualEntry.Description))
        {
            return manualEntry;
        }

        if (mode == Constants.MappingMode.Manual)
            return null;

        // Layer 2a: Canonical OBIS Auto-Mapping (1_8_0, 1-0:1.8.0*255, M010.31_7_0 -> 31_7_0, etc.)
        var normalizedObis = ObisTelemetryCatalog.NormalizeObisCode(trimmedId);
        if (!string.IsNullOrEmpty(normalizedObis) &&
            point.ProfileObisMap.TryGetValue(normalizedObis, out var obisEntry))
        {
            return obisEntry;
        }

        // Layer 2b: Verified Kepware suffix alias or legacy 24-tag DateTime mapping
        var segments = trimmedId.Split('.', StringSplitOptions.RemoveEmptyEntries);
        for (int start = 0; start < segments.Length; start++)
        {
            var suffix = ObisTelemetryCatalog.NormalizeAliasKey(string.Join('.', segments, start, segments.Length - start));
            if (point.ProfileAliasMap.TryGetValue(suffix, out var aliasEntry))
            {
                return aliasEntry;
            }

            if (ObisTelemetryCatalog.LegacyDateTimeSuffixToParameter.TryGetValue(suffix, out var legacyDtParam))
            {
                return new TagRoutingEntry
                {
                    TagNameId = 0,
                    MeasurementPointId = point.MeasurementPointId,
                    MeasurementPointCode = point.Code,
                    MeasurementPointDescription = point.Description,
                    CustomerCode = point.CustomerCode,
                    CustomerName = point.CustomerName,
                    PointStatus = point.Status,
                    TagName = trimmedId,
                    Description = legacyDtParam,
                    Type = "TIME"
                };
            }
        }

        return null;
    }

    public void Invalidate()
    {
        Interlocked.Increment(ref _cacheVersion);
        _memoryCache.Remove(CacheKey);
        TriggerBackgroundRefresh();
    }

    private void TriggerBackgroundRefresh()
    {
        if (_dbContextFactory == null) return;
        if (Interlocked.CompareExchange(ref _backgroundRefreshRunning, 1, 0) != 0)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await ForceRebuildStateAsync(CancellationToken.None);
            }
            catch
            {
                // Keep serving _lastKnownState if transient DB error occurs
            }
            finally
            {
                Interlocked.Exchange(ref _backgroundRefreshRunning, 0);
            }
        });
    }

    private async Task<CachedRoutingState> GetOrBuildStateAsync(CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue<CachedRoutingState>(CacheKey, out var cached) && cached != null)
        {
            return cached;
        }

        // Stale-While-Revalidate: Never block 100+ Kepware REST Client threads when 15-minute TTL expires
        var fallback = _lastKnownState;
        if (fallback != null && Volatile.Read(ref _loadedVersion) == Volatile.Read(ref _cacheVersion))
        {
            TriggerBackgroundRefresh();
            return fallback;
        }

        return await ForceRebuildStateAsync(cancellationToken);
    }

    private async Task<CachedRoutingState> ForceRebuildStateAsync(CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_memoryCache.TryGetValue<CachedRoutingState>(CacheKey, out var cached) &&
                cached != null &&
                Volatile.Read(ref _loadedVersion) == Volatile.Read(ref _cacheVersion))
            {
                return cached;
            }

            var capturedVersion = Volatile.Read(ref _cacheVersion);

            await using var context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            // 1. Load MeterProfiles + Mappings (fallback gracefully to in-memory ObisTelemetryCatalog if table isn't seeded yet)
            var dbProfiles = await context.Set<MeterProfileEntity>()
                .AsNoTracking()
                .Include(p => p.TagMappings.Where(m => !m.Deleted))
                .Where(p => !p.Deleted)
                .OrderBy(p => p.Id)
                .ToListAsync(cancellationToken);

            var profilesList = dbProfiles.Count > 0
                ? dbProfiles.Select(p => new MeterProfileCachedInfo
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    VoltageLevel = p.VoltageLevel,
                    DefaultVoltageUnit = p.DefaultVoltageUnit,
                    DefaultPowerUnit = p.DefaultPowerUnit,
                    IsDefault = p.IsDefault,
                    IsSystem = p.IsSystem,
                    TagMappings = p.TagMappings
                        .OrderBy(m => m.DisplayOrder)
                        .Select(m => new MeterProfileCachedTag
                        {
                            ObisCode = m.ObisCode,
                            ParameterCode = m.ParameterCode,
                            Unit = m.Unit,
                            CustomName = m.CustomName,
                            LegacyAliases = m.LegacyAliases,
                            DataCategory = m.DataCategory,
                            DisplayOrder = m.DisplayOrder
                        })
                        .ToList()
                }).ToList()
                : BuildDefaultFallbackProfiles();

            var profileById = profilesList.ToDictionary(p => p.Id);
            var defaultProfile = profilesList.FirstOrDefault(p => p.IsDefault)
                ?? profilesList.FirstOrDefault(p => p.Code == Constants.MeterProfileCode.LowVoltageVKw)
                ?? profilesList.First();

            // 2. Load MeasurementPoints
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
                    p.MeterSerial,
                    p.ApiKeyHash,
                    p.KeyPrefix,
                    p.MeterProfileId,
                    p.MappingMode,
                    CustomerCode = p.Customer != null ? p.Customer.CustomerCode : null,
                    CustomerName = p.Customer != null ? p.Customer.FullName : null
                })
                .ToListAsync(cancellationToken);

            var pointMap = new Dictionary<long, PointRoutingInfo>(points.Count);
            var serialMap = new Dictionary<string, PointRoutingInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var p in points)
            {
                MeterProfileCachedInfo? assignedProfile = null;
                if (p.MeterProfileId.HasValue && profileById.TryGetValue(p.MeterProfileId.Value, out var matchedProf))
                {
                    assignedProfile = matchedProf;
                }
                else
                {
                    assignedProfile = defaultProfile;
                }

                var normalizedMode = Constants.MappingMode.NormalizeOrDefault(p.MappingMode);
                var info = new PointRoutingInfo
                {
                    MeasurementPointId = p.Id,
                    Code = p.Code ?? string.Empty,
                    Description = p.Description,
                    CustomerCode = p.CustomerCode,
                    CustomerName = p.CustomerName,
                    Status = p.Status,
                    MeterSerial = p.MeterSerial?.Trim(),
                    ApiKeyHash = p.ApiKeyHash,
                    KeyPrefix = p.KeyPrefix,
                    MeterProfileId = p.MeterProfileId ?? assignedProfile?.Id,
                    MeterProfileCode = assignedProfile?.Code,
                    MeterProfileVoltageUnit = assignedProfile?.DefaultVoltageUnit,
                    MappingMode = normalizedMode,
                    TagsByDescription = new Dictionary<string, TagRoutingEntry>(StringComparer.OrdinalIgnoreCase),
                    ManualTagsByRawId = new Dictionary<string, TagRoutingEntry>(StringComparer.OrdinalIgnoreCase),
                    ProfileObisMap = new Dictionary<string, TagRoutingEntry>(StringComparer.OrdinalIgnoreCase),
                    ProfileAliasMap = new Dictionary<string, TagRoutingEntry>(StringComparer.OrdinalIgnoreCase)
                };

                // Seed baseline profile mappings (Tier 2) unless point explicitly disabled Auto-Mapping
                if (normalizedMode != Constants.MappingMode.Manual && assignedProfile != null)
                {
                    PopulateProfileMappings(info, assignedProfile);
                }

                pointMap[p.Id] = info;

                if (!string.IsNullOrWhiteSpace(info.MeterSerial))
                {
                    var serialKey = info.MeterSerial;
                    if (!serialMap.TryGetValue(serialKey, out var existingSerialPoint) ||
                        existingSerialPoint.Status != EntityStatus.Active && info.Status == EntityStatus.Active)
                    {
                        serialMap[serialKey] = info;
                    }
                }
            }

            // 3. Load manual TagName overrides
            var tags = await context.Set<TagNameEntity>()
                .AsNoTracking()
                .Where(t => !t.Deleted)
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

                var hasRawTag = !string.IsNullOrWhiteSpace(t.TagName);
                var trimmedTag = hasRawTag ? t.TagName!.Trim() : string.Empty;
                var desc = t.Description ?? string.Empty;

                if (hasRawTag)
                {
                    var entry = new TagRoutingEntry
                    {
                        TagNameId = t.Id,
                        MeasurementPointId = pointInfo.MeasurementPointId,
                        MeasurementPointCode = pointInfo.Code,
                        MeasurementPointDescription = pointInfo.Description,
                        CustomerCode = pointInfo.CustomerCode,
                        CustomerName = pointInfo.CustomerName,
                        PointStatus = pointInfo.Status,
                        TagName = trimmedTag,
                        Description = desc,
                        Unit = t.Unit,
                        CustomName = t.CustomName,
                        Type = t.Type ?? string.Empty
                    };

                    // Global legacy TagName map (used when request arrives without X-Meter-Serial)
                    if (!tagMap.TryGetValue(entry.TagName, out var existing) ||
                        existing.PointStatus != EntityStatus.Active && entry.PointStatus == EntityStatus.Active)
                    {
                        tagMap[entry.TagName] = entry;
                    }

                    // Per-point Tier 1 manual override
                    pointInfo.ManualTagsByRawId[entry.TagName] = entry;

                    if (!string.IsNullOrWhiteSpace(entry.Description))
                    {
                        pointInfo.TagsByDescription[entry.Description] = entry;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(desc) && !string.IsNullOrWhiteSpace(t.CustomName))
                {
                    // Allow preserving user CustomName customization for Auto-Mapped alert channels without manual TagName
                    if (pointInfo.TagsByDescription.TryGetValue(desc, out var baseline))
                    {
                        var customized = new TagRoutingEntry
                        {
                            TagNameId = baseline.TagNameId,
                            MeasurementPointId = baseline.MeasurementPointId,
                            MeasurementPointCode = baseline.MeasurementPointCode,
                            MeasurementPointDescription = baseline.MeasurementPointDescription,
                            CustomerCode = baseline.CustomerCode,
                            CustomerName = baseline.CustomerName,
                            PointStatus = baseline.PointStatus,
                            TagName = baseline.TagName,
                            Description = baseline.Description,
                            Unit = !string.IsNullOrWhiteSpace(t.Unit) ? t.Unit : baseline.Unit,
                            CustomName = t.CustomName,
                            Type = baseline.Type,
                            ObisCode = baseline.ObisCode,
                            CompositeDateTime = baseline.CompositeDateTime
                        };
                        pointInfo.TagsByDescription[desc] = customized;
                        if (!string.IsNullOrWhiteSpace(customized.ObisCode))
                        {
                            pointInfo.ProfileObisMap[customized.ObisCode] = customized;
                        }
                    }
                }
            }

            var built = new CachedRoutingState(tagMap, pointMap, serialMap, profilesList);
            _lastKnownState = built;
            if (Volatile.Read(ref _cacheVersion) == capturedVersion)
            {
                Interlocked.Exchange(ref _loadedVersion, capturedVersion);
                _memoryCache.Set(CacheKey, built, CacheDuration);
            }

            return built;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static void PopulateProfileMappings(PointRoutingInfo pointInfo, MeterProfileCachedInfo profile)
    {
        foreach (var mapping in profile.TagMappings)
        {
            ObisTelemetryCatalog.ByObisCode.TryGetValue(mapping.ObisCode, out var catalogEntry);
            var entry = new TagRoutingEntry
            {
                TagNameId = 0,
                MeasurementPointId = pointInfo.MeasurementPointId,
                MeasurementPointCode = pointInfo.Code,
                MeasurementPointDescription = pointInfo.Description,
                CustomerCode = pointInfo.CustomerCode,
                CustomerName = pointInfo.CustomerName,
                PointStatus = pointInfo.Status,
                TagName = mapping.ObisCode,
                Description = mapping.ParameterCode,
                Unit = mapping.Unit,
                CustomName = mapping.CustomName,
                Type = mapping.DataCategory,
                ObisCode = mapping.ObisCode,
                CompositeDateTime = catalogEntry?.CompositeDateTime ?? ObisCompositeDateTimeTarget.None
            };

            if (!string.IsNullOrWhiteSpace(mapping.ObisCode))
            {
                pointInfo.ProfileObisMap[mapping.ObisCode] = entry;
            }

            if (!string.IsNullOrWhiteSpace(mapping.ParameterCode) &&
                !pointInfo.TagsByDescription.ContainsKey(mapping.ParameterCode))
            {
                pointInfo.TagsByDescription[mapping.ParameterCode] = entry;
            }

            var aliasSource = !string.IsNullOrWhiteSpace(mapping.LegacyAliases)
                ? mapping.LegacyAliases
                : catalogEntry != null ? string.Join(',', catalogEntry.LegacyAliases) : string.Empty;

            if (!string.IsNullOrWhiteSpace(aliasSource))
            {
                var aliases = aliasSource.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var alias in aliases)
                {
                    var key = ObisTelemetryCatalog.NormalizeAliasKey(alias);
                    if (!string.IsNullOrEmpty(key))
                    {
                        pointInfo.ProfileAliasMap[key] = entry;
                        if (ObisTelemetryCatalog.LooksLikeObisUnderscore(key) && !pointInfo.ProfileObisMap.ContainsKey(key))
                        {
                            pointInfo.ProfileObisMap[key] = entry;
                        }
                    }
                }
            }
        }
    }

    private static List<MeterProfileCachedInfo> BuildDefaultFallbackProfiles()
    {
        return new List<MeterProfileCachedInfo>
        {
            BuildFallbackProfile(1, Constants.MeterProfileCode.MediumVoltageKvKw, "Công tơ Trung thế chuẩn (kV / kW)", "MV", "kV", false),
            BuildFallbackProfile(2, Constants.MeterProfileCode.LowVoltageVKw, "Công tơ Hạ thế chuẩn (V / kW)", "LV", "V", true)
        };
    }

    private static MeterProfileCachedInfo BuildFallbackProfile(
        long id, string code, string name, string voltageLevel, string voltageUnit, bool isDefault)
    {
        var isMv = voltageLevel == "MV";
        return new MeterProfileCachedInfo
        {
            Id = id,
            Code = code,
            Name = name,
            VoltageLevel = voltageLevel,
            DefaultVoltageUnit = voltageUnit,
            DefaultPowerUnit = "kW",
            IsDefault = isDefault,
            IsSystem = true,
            TagMappings = ObisTelemetryCatalog.AllEntries
                .Select(e => new MeterProfileCachedTag
                {
                    ObisCode = e.ObisCode,
                    ParameterCode = e.ParameterCode,
                    Unit = isMv ? e.MvUnit : e.LvUnit,
                    CustomName = e.DefaultCustomName,
                    LegacyAliases = string.Join(',', e.LegacyAliases),
                    DataCategory = e.DataCategory,
                    DisplayOrder = e.DisplayOrder
                })
                .ToList()
        };
    }
}
