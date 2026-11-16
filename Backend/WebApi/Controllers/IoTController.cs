using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Commands.IoT;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.IoT;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Application.Services;
using Domain.Entities;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.Constants;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

[ApiController]
[Area(Common.Url.Areas.IoT)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.IoT)]
public class IoTController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;
    private readonly IGcsCutOffCycleReadOnlyRepository _gcsCutOffCycleReadOnlyRepository;
    private readonly ISystemConfigReadOnlyRepository _systemConfigReadOnlyRepository;
    private readonly ISystemConfigReadWriteRepository _systemConfigReadWriteRepository;
    private readonly IMeasurementPointsReadWriteRepository _measurementPointsReadWriteRepository;
    private readonly ITagRoutingCacheService _tagRoutingCacheService;
    private readonly IMeterTelemetryIngestionService _meterTelemetryIngestionService;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<ListNotificationHub> _listHubContext;

    public IoTController(
        IMediator mediator,
        IHttpContextAccessorService httpContextAccessorService,
        ILoggerService loggerService,
        IGcsCutOffCycleReadOnlyRepository gcsCutOffCycleReadOnlyRepository,
        ISystemConfigReadOnlyRepository systemConfigReadOnlyRepository,
        ISystemConfigReadWriteRepository systemConfigReadWriteRepository,
        IMeasurementPointsReadWriteRepository measurementPointsReadWriteRepository,
        ITagRoutingCacheService tagRoutingCacheService,
        IMeterTelemetryIngestionService meterTelemetryIngestionService,
        ISnowflakeIdService snowflakeIdService,
        IConfiguration configuration,
        IHubContext<ListNotificationHub> listHubContext)
    {
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _gcsCutOffCycleReadOnlyRepository = gcsCutOffCycleReadOnlyRepository;
        _systemConfigReadOnlyRepository = systemConfigReadOnlyRepository;
        _systemConfigReadWriteRepository = systemConfigReadWriteRepository;
        _measurementPointsReadWriteRepository = measurementPointsReadWriteRepository;
        _tagRoutingCacheService = tagRoutingCacheService;
        _meterTelemetryIngestionService = meterTelemetryIngestionService;
        _snowflakeIdService = snowflakeIdService;
        _configuration = configuration;
        _listHubContext = listHubContext;
    }

    private string? GetIoTCryptoSecret()
        => _configuration["Security:IoTKeyCryptoSecret"] ?? _configuration["Email:SecretCryptoKey"];

    private string? GetHeaderMeterSerial()
    {
        if (HttpContext.Request.Headers.TryGetValue("Serial", out var s0) && !string.IsNullOrWhiteSpace(s0))
            return s0.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("X-Meter-Serial", out var s1) && !string.IsNullOrWhiteSpace(s1))
            return s1.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("Meter-Serial", out var s2) && !string.IsNullOrWhiteSpace(s2))
            return s2.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("MeterSerial", out var s3) && !string.IsNullOrWhiteSpace(s3))
            return s3.ToString().Trim();
        return null;
    }

    private string? GetHeaderApiKey()
    {
        if (HttpContext.Request.Headers.TryGetValue("Private-key", out var pk) && !string.IsNullOrWhiteSpace(pk))
            return pk.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("Private-Key", out var pk2) && !string.IsNullOrWhiteSpace(pk2))
            return pk2.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("PrivateKey", out var pk3) && !string.IsNullOrWhiteSpace(pk3))
            return pk3.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("Global-key", out var gk) && !string.IsNullOrWhiteSpace(gk))
            return gk.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("Global-Key", out var gk2) && !string.IsNullOrWhiteSpace(gk2))
            return gk2.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("GlobalKey", out var gk3) && !string.IsNullOrWhiteSpace(gk3))
            return gk3.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("X-IoT-Api-Key", out var k1) && !string.IsNullOrWhiteSpace(k1))
            return k1.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("X-API-Key", out var k1b) && !string.IsNullOrWhiteSpace(k1b))
            return k1b.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("ApiKey", out var k1c) && !string.IsNullOrWhiteSpace(k1c))
            return k1c.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("Header Value", out var k2) && !string.IsNullOrWhiteSpace(k2))
            return k2.ToString().Trim();
        if (HttpContext.Request.Headers.TryGetValue("Header-Value", out var k3) && !string.IsNullOrWhiteSpace(k3))
            return k3.ToString().Trim();
        return null;
    }

    private async Task RecordPushAndBroadcastAsync(string ip, bool authorized, int tagCount, CancellationToken cancellationToken)
    {
        IoTSecurityManager.RecordPush(ip, authorized, tagCount);
        if (!IoTSecurityManager.TryClaimTelemetryBroadcastSlot(TimeSpan.FromSeconds(2)))
        {
            return;
        }

        try
        {
            var lastPush = IoTSecurityManager.LastPushTime;
            await _listHubContext.Clients.All.SendAsync("ReceiveIoTTelemetry", new
            {
                lastPushTime = lastPush,
                lastPushIp = IoTSecurityManager.LastPushIp,
                lastPushStatus = IoTSecurityManager.LastPushStatus,
                lastPushTagCount = IoTSecurityManager.LastPushTagCount,
                totalPushCount = IoTSecurityManager.TotalPushCount,
                isActiveRecently = lastPush.HasValue && (DateTimeOffset.Now - lastPush.Value).TotalMinutes < 5
            }, cancellationToken);
        }
        catch
        {
            // Keep REST client ingestion fast even if SignalR broadcast fails
        }
    }

    private async Task EnsureIoTSecurityConfigLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (IoTSecurityManager.IsInitialized)
            return;

        await IoTSecurityManager.ConfigRefreshLock.WaitAsync(cancellationToken);
        try
        {
            if (IoTSecurityManager.IsInitialized)
                return;

            try
            {
                var configs = await _systemConfigReadOnlyRepository.GetListSystemConfigAsync(cancellationToken);
                var apiKey = configs.FirstOrDefault(x => x.Key == Constants.SystemConfig.IoT_ApiKey)?.Value;
                var allowedIps = configs.FirstOrDefault(x => x.Key == Constants.SystemConfig.IoT_AllowedIps)?.Value;
                var requireKeyStr = configs.FirstOrDefault(x => x.Key == Constants.SystemConfig.IoT_RequireApiKey)?.Value;
                var allowLegacyStr = configs.FirstOrDefault(x => x.Key == Constants.SystemConfig.IoT_AllowLegacyTagRouting)?.Value;

                bool requireKey = bool.TryParse(requireKeyStr, out var rk) && rk;
                bool allowLegacy = string.IsNullOrWhiteSpace(allowLegacyStr) || !bool.TryParse(allowLegacyStr, out var al) || al;

                if (string.IsNullOrWhiteSpace(apiKey))
                    apiKey = _configuration["IoT:ApiKey"] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(allowedIps))
                    allowedIps = _configuration["IoT:AllowedIps"] ?? string.Empty;

                IoTSecurityManager.UpdateConfig(apiKey, allowedIps, requireKey, allowLegacy);
            }
            catch
            {
                var apiKey = _configuration["IoT:ApiKey"] ?? string.Empty;
                var allowedIps = _configuration["IoT:AllowedIps"] ?? string.Empty;
                IoTSecurityManager.UpdateConfig(apiKey, allowedIps, false, true);
            }
        }
        finally
        {
            IoTSecurityManager.ConfigRefreshLock.Release();
        }
    }

    private async Task<(bool Authorized, string? ErrorMessage)> AuthorizeIoTRequestAsync(
        string? meterSerial,
        int tagCount,
        CancellationToken cancellationToken)
    {
        await EnsureIoTSecurityConfigLoadedAsync(cancellationToken);
        var (globalApiKey, allowedIps, requireApiKey, allowLegacyTagRouting) = IoTSecurityManager.GetCachedConfig();

        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null && remoteIp.IsIPv4MappedToIPv6)
            remoteIp = remoteIp.MapToIPv4();
        var ipStr = remoteIp?.ToString() ?? "Unknown";

        var providedKey = GetHeaderApiKey();

        // Layer 0: If AllowedIps whitelist is explicitly configured, enforce it for all unauthenticated IoT pushes
        if (HttpContext.User?.Identity?.IsAuthenticated != true &&
            !string.IsNullOrWhiteSpace(allowedIps) &&
            !IsTrustedNetworkIp(remoteIp, ipStr, allowedIps, allowPrivateLanWhenUnconfigured: false, globalApiKey))
        {
            await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
            return (false, $"IP nguồn '{ipStr}' không nằm trong danh sách IP được phép (AllowedIps).");
        }

        // Flow A: Simultaneous X-Meter-Serial + X-IoT-Api-Key 2-tier validation
        if (!string.IsNullOrWhiteSpace(meterSerial))
        {
            var point = await _tagRoutingCacheService.GetPointBySerialAsync(meterSerial, cancellationToken);
            if (point == null || !string.Equals(point.Status, EntityStatus.Active, StringComparison.OrdinalIgnoreCase))
            {
                IoTSecurityManager.RecordUnmatchedSerial(meterSerial, ipStr);
                await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
                return (false, $"Không tìm thấy điểm đo đang hoạt động với X-Meter-Serial '{meterSerial}'.");
            }

            if (HttpContext.User?.Identity?.IsAuthenticated == true)
            {
                IoTSecurityManager.RemoveUnmatchedSerial(meterSerial);
                await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
                return (true, null);
            }

            // Tier 1: Point has dedicated ApiKeyHash -> strictly enforce dedicated key
            if (!string.IsNullOrWhiteSpace(point.ApiKeyHash))
            {
                if (IoTSecurityManager.VerifySecretHash(providedKey, point.ApiKeyHash))
                {
                    var (sessionOk, sessionErr) = IoTSecurityManager.ValidateAndTrackSerialSession(meterSerial, ipStr, tagCount);
                    if (!sessionOk)
                    {
                        _loggerService.LogWarning($"[IoTSecurity] {sessionErr}");
                        await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
                        return (false, sessionErr);
                    }

                    IoTSecurityManager.RemoveUnmatchedSerial(meterSerial);
                    await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
                    return (true, null);
                }

                await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
                return (false, $"X-IoT-Api-Key không khớp với khóa riêng của công tơ '{meterSerial}'.");
            }

            // Tier 2: Point has no dedicated key -> fall back to Global IOT_API_KEY (or trusted IP if unkeyed & !requireApiKey)
            if (!string.IsNullOrWhiteSpace(globalApiKey))
            {
                if (IoTSecurityManager.FixedTimeEquals(providedKey, globalApiKey))
                {
                    var (sessionOk, sessionErr) = IoTSecurityManager.ValidateAndTrackSerialSession(meterSerial, ipStr, tagCount);
                    if (!sessionOk)
                    {
                        _loggerService.LogWarning($"[IoTSecurity] {sessionErr}");
                        await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
                        return (false, sessionErr);
                    }

                    IoTSecurityManager.RemoveUnmatchedSerial(meterSerial);
                    await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
                    return (true, null);
                }

                await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
                return (false, "X-IoT-Api-Key không khớp Global API Key.");
            }

            if (!requireApiKey && IsTrustedNetworkIp(remoteIp, ipStr, allowedIps, allowPrivateLanWhenUnconfigured: true, globalApiKey))
            {
                var (sessionOk, sessionErr) = IoTSecurityManager.ValidateAndTrackSerialSession(meterSerial, ipStr, tagCount);
                if (!sessionOk)
                {
                    _loggerService.LogWarning($"[IoTSecurity] {sessionErr}");
                    await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
                    return (false, sessionErr);
                }

                IoTSecurityManager.RemoveUnmatchedSerial(meterSerial);
                await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
                return (true, null);
            }

            await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
            return (false, "Thiếu X-IoT-Api-Key hợp lệ.");
        }

        // Flow B: Legacy Kepware request without X-Meter-Serial
        if (HttpContext.User?.Identity?.IsAuthenticated == true)
            return (true, null);

        if (!allowLegacyTagRouting)
        {
            await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
            return (false, "Chế độ định tuyến TagName cũ đã tắt. Vui lòng gửi kèm header X-Meter-Serial.");
        }

        bool hasValidGlobalKey = !string.IsNullOrWhiteSpace(globalApiKey) &&
                                 IoTSecurityManager.FixedTimeEquals(providedKey, globalApiKey);

        if (requireApiKey)
        {
            await RecordPushAndBroadcastAsync(ipStr, hasValidGlobalKey, tagCount, cancellationToken);
            return hasValidGlobalKey
                ? (true, null)
                : (false, "Truy cập IoT Gateway không hợp lệ (X-IoT-Api-Key không khớp).");
        }

        if (hasValidGlobalKey || IsTrustedNetworkIp(remoteIp, ipStr, allowedIps, allowPrivateLanWhenUnconfigured: true, globalApiKey))
        {
            await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
            return (true, null);
        }

        await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
        return (false, "Truy cập IoT Gateway không hợp lệ (IP hoặc X-IoT-Api-Key không được phép).");
    }

    private bool IsTrustedNetworkIp(
        IPAddress? remoteIp,
        string ipStr,
        string allowedIps,
        bool allowPrivateLanWhenUnconfigured,
        string globalApiKey)
    {
        if (remoteIp == null) return false;
        if (IPAddress.IsLoopback(remoteIp)) return true;

        if (!string.IsNullOrWhiteSpace(allowedIps))
        {
            var list = allowedIps.Split(',', ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (list.Any(ip => string.Equals(ip, ipStr, StringComparison.OrdinalIgnoreCase)))
                return true;
        }

        var iotServerUrl = _configuration["BaseUrlGetDataFromServerIoT"];
        if (!string.IsNullOrWhiteSpace(iotServerUrl) &&
            Uri.TryCreate(iotServerUrl, UriKind.Absolute, out var uri) &&
            string.Equals(uri.Host, ipStr, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (allowPrivateLanWhenUnconfigured)
        {
            var bytes = remoteIp.GetAddressBytes();
            if (bytes.Length == 4)
            {
                bool isPrivateLan = bytes[0] == 10 ||
                                    (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                                    (bytes[0] == 192 && bytes[1] == 168);
                if (isPrivateLan && string.IsNullOrWhiteSpace(globalApiKey) && string.IsNullOrWhiteSpace(allowedIps))
                    return true;
            }
        }

        return false;
    }

    [HttpGet]
    [Route(Common.Url.Default.IoT.GetConfig)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<IoTConfigResponse>> GetIoTConfigAsync(CancellationToken cancellationToken)
    {
        try
        {
            await EnsureIoTSecurityConfigLoadedAsync(cancellationToken);
            var (apiKey, allowedIps, requireApiKey, allowLegacyTagRouting) = IoTSecurityManager.GetCachedConfig();

            var lastPush = IoTSecurityManager.LastPushTime;
            bool isActiveRecently = lastPush.HasValue && (DateTimeOffset.Now - lastPush.Value).TotalMinutes < 5;

            return RequestResult<IoTConfigResponse>.Succeed(new IoTConfigResponse
            {
                ApiKey = apiKey,
                AllowedIps = allowedIps,
                RequireApiKey = requireApiKey,
                AllowLegacyTagRouting = allowLegacyTagRouting,
                LastPushTime = lastPush,
                LastPushIp = IoTSecurityManager.LastPushIp,
                LastPushStatus = IoTSecurityManager.LastPushStatus,
                LastPushTagCount = IoTSecurityManager.LastPushTagCount,
                TotalPushCount = IoTSecurityManager.TotalPushCount,
                IsActiveRecently = isActiveRecently
            });
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, nameof(GetIoTConfigAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.UpdateConfig)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<IoTConfigResponse>> UpdateIoTConfigAsync([FromBody] UpdateIoTConfigRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var apiKey = request.ApiKey?.Trim() ?? string.Empty;
            var allowedIps = request.AllowedIps?.Trim() ?? string.Empty;
            var requireApiKey = request.RequireApiKey;
            var allowLegacyTagRouting = request.AllowLegacyTagRouting;

            var pairs = new Dictionary<string, string>
            {
                [Constants.SystemConfig.IoT_ApiKey] = apiKey,
                [Constants.SystemConfig.IoT_AllowedIps] = allowedIps,
                [Constants.SystemConfig.IoT_RequireApiKey] = requireApiKey ? "true" : "false",
                [Constants.SystemConfig.IoT_AllowLegacyTagRouting] = allowLegacyTagRouting ? "true" : "false"
            };

            foreach (var kvp in pairs)
            {
                var existing = await _systemConfigReadWriteRepository.GetConfigByKeyAsync(kvp.Key, cancellationToken);
                if (existing != null)
                {
                    existing.Value = kvp.Value;
                }
                else
                {
                    var id = await _snowflakeIdService.GenerateId(cancellationToken);
                    await _systemConfigReadWriteRepository.AddAsync(new SystemConfigEntity
                    {
                        Id = id,
                        Key = kvp.Key,
                        Value = kvp.Value
                    }, cancellationToken);
                }
            }

            await _systemConfigReadWriteRepository.SaveChangesAsync(cancellationToken);

            IoTSecurityManager.UpdateConfig(apiKey, allowedIps, requireApiKey, allowLegacyTagRouting);

            var lastPush = IoTSecurityManager.LastPushTime;
            return RequestResult<IoTConfigResponse>.Succeed(new IoTConfigResponse
            {
                ApiKey = apiKey,
                AllowedIps = allowedIps,
                RequireApiKey = requireApiKey,
                AllowLegacyTagRouting = allowLegacyTagRouting,
                LastPushTime = lastPush,
                LastPushIp = IoTSecurityManager.LastPushIp,
                LastPushStatus = IoTSecurityManager.LastPushStatus,
                LastPushTagCount = IoTSecurityManager.LastPushTagCount,
                TotalPushCount = IoTSecurityManager.TotalPushCount,
                IsActiveRecently = lastPush.HasValue && (DateTimeOffset.Now - lastPush.Value).TotalMinutes < 5
            });
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, nameof(UpdateIoTConfigAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.GenerateKey)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public Task<RequestResult<string>> GenerateIoTKeyAsync()
    {
        var newKey = IoTSecurityManager.GenerateSecureKey();
        return Task.FromResult(RequestResult<string>.Succeed(newKey));
    }

    [HttpGet]
    [Route(Common.Url.Default.IoT.GetMeterProfiles)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<List<MeterProfileDto>>> GetMeterProfilesAsync(CancellationToken cancellationToken)
    {
        var profiles = await _tagRoutingCacheService.GetMeterProfilesAsync(cancellationToken);
        var result = profiles.Select(p => new MeterProfileDto
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
            TagMappings = p.TagMappings.Select(m => new MeterProfileTagMappingDto
            {
                ObisCode = m.ObisCode,
                ParameterCode = m.ParameterCode,
                Unit = m.Unit,
                CustomName = m.CustomName,
                LegacyAliases = m.LegacyAliases,
                DataCategory = m.DataCategory,
                DisplayOrder = m.DisplayOrder
            }).ToList()
        }).ToList();

        return RequestResult<List<MeterProfileDto>>.Succeed(result);
    }

    [HttpGet]
    [Route(Common.Url.Default.IoT.GetUnmatchedSerials)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public Task<RequestResult<List<UnmatchedSerialDto>>> GetUnmatchedSerialsAsync()
    {
        var list = IoTSecurityManager.GetRecentUnmatchedSerials()
            .Select(x => new UnmatchedSerialDto
            {
                MeterSerial = x.MeterSerial,
                RemoteIp = x.RemoteIp,
                FirstSeenAt = x.FirstSeenAt,
                LastSeenAt = x.LastSeenAt,
                HitCount = x.HitCount
            })
            .ToList();

        return Task.FromResult(RequestResult<List<UnmatchedSerialDto>>.Succeed(list));
    }

    [HttpGet]
    [Route(Common.Url.Default.IoT.GetPointApiKey)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<PointApiKeyDto>> GetPointApiKeyAsync(
        long id,
        [FromQuery] bool reveal = false,
        CancellationToken cancellationToken = default)
    {
        var point = await _measurementPointsReadWriteRepository.GetMeasurementPointByIdAsync(id, cancellationToken);
        if (point == null || point.Deleted)
        {
            return RequestResult<PointApiKeyDto>.Fail(new[]
            {
                new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy điểm đo." }
            });
        }

        bool hasDedicated = !string.IsNullOrWhiteSpace(point.ApiKeyHash);
        string? decryptedKey = null;
        if (reveal && hasDedicated && !string.IsNullOrWhiteSpace(point.ApiKeyEncrypted))
        {
            decryptedKey = IoTSecurityManager.DecryptPointSecret(point.ApiKeyEncrypted, GetIoTCryptoSecret());
        }

        return RequestResult<PointApiKeyDto>.Succeed(new PointApiKeyDto
        {
            MeasurementPointId = point.Id,
            PointCode = point.Code,
            MeterSerial = point.MeterSerial,
            HasDedicatedKey = hasDedicated,
            KeyPrefix = point.KeyPrefix,
            ApiKey = decryptedKey,
            AuthTier = hasDedicated ? "TIER_1_DEDICATED" : "TIER_2_GLOBAL"
        });
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.GeneratePointApiKey)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<PointApiKeyDto>> GeneratePointApiKeyAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var point = await _measurementPointsReadWriteRepository.GetMeasurementPointByIdAsync(id, cancellationToken);
        if (point == null || point.Deleted)
        {
            return RequestResult<PointApiKeyDto>.Fail(new[]
            {
                new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy điểm đo." }
            });
        }

        var plainKey = IoTSecurityManager.GenerateSecureKey("MDMS_PT");
        point.ApiKeyHash = IoTSecurityManager.HashSecret(plainKey);
        point.ApiKeyEncrypted = IoTSecurityManager.EncryptPointSecret(plainKey, GetIoTCryptoSecret());
        point.KeyPrefix = IoTSecurityManager.ExtractKeyPrefix(plainKey);

        await _measurementPointsReadWriteRepository.UpdateAsync(point, cancellationToken);
        await _measurementPointsReadWriteRepository.SaveChangesAsync(cancellationToken);
        IoTSecurityManager.ResetSerialSession(point.MeterSerial);
        _tagRoutingCacheService.Invalidate();

        return RequestResult<PointApiKeyDto>.Succeed(new PointApiKeyDto
        {
            MeasurementPointId = point.Id,
            PointCode = point.Code,
            MeterSerial = point.MeterSerial,
            HasDedicatedKey = true,
            KeyPrefix = point.KeyPrefix,
            ApiKey = plainKey,
            AuthTier = "TIER_1_DEDICATED"
        });
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.RevokePointApiKey)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<PointApiKeyDto>> RevokePointApiKeyAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var point = await _measurementPointsReadWriteRepository.GetMeasurementPointByIdAsync(id, cancellationToken);
        if (point == null || point.Deleted)
        {
            return RequestResult<PointApiKeyDto>.Fail(new[]
            {
                new ErrorItem { FieldName = nameof(id), Error = "Không tìm thấy điểm đo." }
            });
        }

        point.ApiKeyHash = null;
        point.ApiKeyEncrypted = null;
        point.KeyPrefix = null;

        await _measurementPointsReadWriteRepository.UpdateAsync(point, cancellationToken);
        await _measurementPointsReadWriteRepository.SaveChangesAsync(cancellationToken);
        IoTSecurityManager.ResetSerialSession(point.MeterSerial);
        _tagRoutingCacheService.Invalidate();

        return RequestResult<PointApiKeyDto>.Succeed(new PointApiKeyDto
        {
            MeasurementPointId = point.Id,
            PointCode = point.Code,
            MeterSerial = point.MeterSerial,
            HasDedicatedKey = false,
            KeyPrefix = null,
            ApiKey = null,
            AuthTier = "TIER_2_GLOBAL"
        });
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.UnifiedTelemetry)]
    [Route("api/v1/" + Common.Url.Default.IoT.UnifiedTelemetry)]
    [Route("api/" + Common.Url.Default.IoT.UnifiedTelemetry)]
    public async Task<RequestResult<MeasurementPointMonitoringResponse>> ReceiveUnifiedTelemetryAsync(
        [FromBody] IoTDataRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var meterSerial = GetHeaderMeterSerial();
            var (authorized, authError) = await AuthorizeIoTRequestAsync(
                meterSerial,
                request?.Values?.Count ?? 0,
                cancellationToken);

            if (!authorized)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Authorization", Error = authError ?? "Truy cập IoT Gateway không hợp lệ." }
                });
            }

            return await _meterTelemetryIngestionService.IngestAsync(
                request,
                meterSerial,
                TelemetrySourceKind.Unified,
                cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ReceiveUnifiedTelemetryAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.MeasurementPointMonitoring)]
    [Route("api/v1/" + Common.Url.Default.IoT.MeasurementPointMonitoring)]
    [Route("api/" + Common.Url.Default.IoT.MeasurementPointMonitoring)]
    public async Task<RequestResult<MeasurementPointMonitoringResponse>> ReceiveDataAsync([FromBody] IoTDataRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            var meterSerial = GetHeaderMeterSerial();
            var (authorized, authError) = await AuthorizeIoTRequestAsync(meterSerial, request?.Values?.Count ?? 0, cancellationToken);
            if (!authorized)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Authorization", Error = authError ?? "Truy cập IoT Gateway không hợp lệ (IP hoặc X-IoT-Api-Key không được phép)." }
                });
            }

            return await _mediator.Send(new MeasurementPointMonitoringCommand
            {
                Request = request ?? new IoTDataRequest(),
                MeterSerial = meterSerial,
                Tracker = _httpContextAccessorService.Tracker()
            }, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ReceiveDataAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.MeasurementPointMonitoringByDateTime)]
    [Route("api/v1/" + Common.Url.Default.IoT.MeasurementPointMonitoringByDateTime)]
    [Route("api/" + Common.Url.Default.IoT.MeasurementPointMonitoringByDateTime)]
    public async Task<RequestResult<MeasurementPointMonitoringResponse>> ReceiveDataByDateTimeAsync([FromBody] IoTDataRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            var meterSerial = GetHeaderMeterSerial();
            var (authorized, authError) = await AuthorizeIoTRequestAsync(meterSerial, request?.Values?.Count ?? 0, cancellationToken);
            if (!authorized)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Authorization", Error = authError ?? "Truy cập IoT Gateway không hợp lệ (IP hoặc X-IoT-Api-Key không được phép)." }
                });
            }

            return await _mediator.Send(new MeasurementPointMonitoringByDateTimeCommand
            {
                Request = request ?? new IoTDataRequest(),
                MeterSerial = meterSerial,
                Tracker = _httpContextAccessorService.Tracker()
            }, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ReceiveDataByDateTimeAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.MeasurementPointAlert)]
    [Route("api/v1/" + Common.Url.Default.IoT.MeasurementPointAlert)]
    [Route("api/" + Common.Url.Default.IoT.MeasurementPointAlert)]
    public async Task<RequestResult<MeasurementPointMonitoringResponse>> ReceiveDataAlertAsync([FromBody] IoTDataRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            var meterSerial = GetHeaderMeterSerial();
            var (authorized, authError) = await AuthorizeIoTRequestAsync(meterSerial, request?.Values?.Count ?? 0, cancellationToken);
            if (!authorized)
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Authorization", Error = authError ?? "Truy cập IoT Gateway không hợp lệ (IP hoặc X-IoT-Api-Key không được phép)." }
                });
            }

            return await _mediator.Send(new MeasurementPointEventCommand
            {
                Request = request ?? new IoTDataRequest(),
                MeterSerial = meterSerial,
                Tracker = _httpContextAccessorService.Tracker()
            }, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ReceiveDataAlertAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.GetDataGcs)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<CursorPaginationResponse<GetDataLatchResponse>>> GetDataLatchAsync(GetDataLatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await _gcsCutOffCycleReadOnlyRepository.GetDataLatchResponseAsync(request, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetDataLatchAsync));
            return RequestResult<CursorPaginationResponse<GetDataLatchResponse>>.Fail(new[]
            {
                new ErrorItem()
                {
                    FieldName = "System",
                    Error = $"Có lỗi xử lý dữ liệu chốt sổ: {e.Message}"
                }
            });
        }
    }
}
