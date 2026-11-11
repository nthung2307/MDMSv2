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
using Domain.Entities;
using Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
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
    private readonly IMeasurementPointMonitoringReadOnlyRepository _pointMonitoringReadOnlyRepository;
    private readonly ISystemConfigReadOnlyRepository _systemConfigReadOnlyRepository;
    private readonly ISystemConfigReadWriteRepository _systemConfigReadWriteRepository;
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<ListNotificationHub> _listHubContext;

    public IoTController(
        IMediator mediator,
        IHttpContextAccessorService httpContextAccessorService,
        ILoggerService loggerService,
        IGcsCutOffCycleReadOnlyRepository gcsCutOffCycleReadOnlyRepository,
        IMeasurementPointMonitoringReadOnlyRepository pointMonitoringReadOnlyRepository,
        ISystemConfigReadOnlyRepository systemConfigReadOnlyRepository,
        ISystemConfigReadWriteRepository systemConfigReadWriteRepository,
        ISnowflakeIdService snowflakeIdService,
        IConfiguration configuration,
        IHubContext<ListNotificationHub> listHubContext)
    {
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _gcsCutOffCycleReadOnlyRepository = gcsCutOffCycleReadOnlyRepository;
        _pointMonitoringReadOnlyRepository = pointMonitoringReadOnlyRepository;
        _systemConfigReadOnlyRepository = systemConfigReadOnlyRepository;
        _systemConfigReadWriteRepository = systemConfigReadWriteRepository;
        _snowflakeIdService = snowflakeIdService;
        _configuration = configuration;
        _listHubContext = listHubContext;
    }

    private async Task RecordPushAndBroadcastAsync(string ip, bool authorized, int tagCount, CancellationToken cancellationToken)
    {
        IoTSecurityManager.RecordPush(ip, authorized, tagCount);
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
            // Không làm gián đoạn phản hồi cho REST Client nếu SignalR broadcast lỗi
        }
    }

    private async Task EnsureIoTSecurityConfigLoadedAsync(CancellationToken cancellationToken = default)
    {
        if (IoTSecurityManager.IsInitialized)
            return;

        try
        {
            var configs = await _systemConfigReadOnlyRepository.GetListSystemConfigAsync(cancellationToken);
            var apiKey = configs.FirstOrDefault(x => x.Key == Constants.SystemConfig.IoT_ApiKey)?.Value;
            var allowedIps = configs.FirstOrDefault(x => x.Key == Constants.SystemConfig.IoT_AllowedIps)?.Value;
            var requireKeyStr = configs.FirstOrDefault(x => x.Key == Constants.SystemConfig.IoT_RequireApiKey)?.Value;
            bool requireKey = bool.TryParse(requireKeyStr, out var rk) && rk;

            // Fallback sang appsettings.json nếu DB chưa cấu hình
            if (string.IsNullOrWhiteSpace(apiKey))
                apiKey = _configuration["IoT:ApiKey"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(allowedIps))
                allowedIps = _configuration["IoT:AllowedIps"] ?? string.Empty;

            IoTSecurityManager.UpdateConfig(apiKey, allowedIps, requireKey);
        }
        catch
        {
            var apiKey = _configuration["IoT:ApiKey"] ?? string.Empty;
            var allowedIps = _configuration["IoT:AllowedIps"] ?? string.Empty;
            IoTSecurityManager.UpdateConfig(apiKey, allowedIps, false);
        }
    }

    private async Task<bool> IsAuthorizedIoTRequestAsync(int tagCount = 0, CancellationToken cancellationToken = default)
    {
        // 1. Cho phép nếu đã xác thực bằng JWT hợp lệ (Frontend / User đăng nhập)
        if (HttpContext.User?.Identity?.IsAuthenticated == true)
            return true;

        await EnsureIoTSecurityConfigLoadedAsync(cancellationToken);
        var (apiKey, allowedIps, requireApiKey) = IoTSecurityManager.GetCachedConfig();

        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null && remoteIp.IsIPv4MappedToIPv6)
            remoteIp = remoteIp.MapToIPv4();
        var ipStr = remoteIp?.ToString() ?? "Unknown";

        // Kiểm tra Header X-IoT-Api-Key nếu được cung cấp
        bool hasValidHeaderKey = false;
        if (HttpContext.Request.Headers.TryGetValue("X-IoT-Api-Key", out var providedKey) &&
            !string.IsNullOrWhiteSpace(providedKey) &&
            !string.IsNullOrWhiteSpace(apiKey) &&
            string.Equals(providedKey.ToString().Trim(), apiKey.Trim(), StringComparison.Ordinal))
        {
            hasValidHeaderKey = true;
        }
        // Phòng thủ linh hoạt: Trường hợp cấu hình Kepware gửi cả nhãn "Header Value"
        else if (!string.IsNullOrWhiteSpace(apiKey))
        {
            if (HttpContext.Request.Headers.TryGetValue("Header Value", out var fallbackVal) &&
                string.Equals(fallbackVal.ToString().Trim(), apiKey.Trim(), StringComparison.Ordinal))
            {
                hasValidHeaderKey = true;
            }
            else if (HttpContext.Request.Headers.TryGetValue("Header-Value", out var fallbackVal2) &&
                string.Equals(fallbackVal2.ToString().Trim(), apiKey.Trim(), StringComparison.Ordinal))
            {
                hasValidHeaderKey = true;
            }
        }

        // Nếu hệ thống bật chế độ bắt buộc kiểm tra API Key
        if (requireApiKey)
        {
            if (hasValidHeaderKey)
            {
                await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
                return true;
            }

            await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
            return false;
        }

        // Nếu có Header Key hợp lệ -> Luôn cho phép
        if (hasValidHeaderKey)
        {
            await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
            return true;
        }

        // Kiểm tra IP Whitelist (Loopback 127.0.0.1 / ::1, mạng nội bộ OT/LAN hoặc IP cấu hình của Kepware)
        if (remoteIp != null)
        {
            if (IPAddress.IsLoopback(remoteIp))
            {
                await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
                return true;
            }

            // Kiểm tra khớp với IP trong danh sách AllowedIps do SuperAdmin cài đặt
            if (!string.IsNullOrWhiteSpace(allowedIps))
            {
                var list = allowedIps.Split(',', ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (list.Any(ip => string.Equals(ip, ipStr, StringComparison.OrdinalIgnoreCase)))
                {
                    await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
                    return true;
                }
            }

            // Kiểm tra khớp với IP máy chủ Kepware trong BaseUrlGetDataFromServerIoT
            var iotServerUrl = _configuration["BaseUrlGetDataFromServerIoT"];
            if (!string.IsNullOrWhiteSpace(iotServerUrl) &&
                Uri.TryCreate(iotServerUrl, UriKind.Absolute, out var uri) &&
                string.Equals(uri.Host, ipStr, StringComparison.OrdinalIgnoreCase))
            {
                await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
                return true;
            }

            // Cho phép dải mạng LAN nội bộ (10.x.x.x, 172.16-31.x.x, 192.168.x.x) nếu cả API Key và Allowed IPs đều chưa đặt
            var bytes = remoteIp.GetAddressBytes();
            if (bytes.Length == 4)
            {
                bool isPrivateLan = bytes[0] == 10 ||
                                    (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                                    (bytes[0] == 192 && bytes[1] == 168);
                if (isPrivateLan && string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(allowedIps))
                {
                    await RecordPushAndBroadcastAsync(ipStr, true, tagCount, cancellationToken);
                    return true;
                }
            }
        }

        await RecordPushAndBroadcastAsync(ipStr, false, tagCount, cancellationToken);
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
            var (apiKey, allowedIps, requireApiKey) = IoTSecurityManager.GetCachedConfig();

            var lastPush = IoTSecurityManager.LastPushTime;
            bool isActiveRecently = lastPush.HasValue && (DateTimeOffset.Now - lastPush.Value).TotalMinutes < 5;

            var response = new IoTConfigResponse
            {
                ApiKey = apiKey,
                AllowedIps = allowedIps,
                RequireApiKey = requireApiKey,
                LastPushTime = lastPush,
                LastPushIp = IoTSecurityManager.LastPushIp,
                LastPushStatus = IoTSecurityManager.LastPushStatus,
                LastPushTagCount = IoTSecurityManager.LastPushTagCount,
                TotalPushCount = IoTSecurityManager.TotalPushCount,
                IsActiveRecently = isActiveRecently
            };

            return RequestResult<IoTConfigResponse>.Succeed(response);
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

            var pairs = new Dictionary<string, string>
            {
                [Constants.SystemConfig.IoT_ApiKey] = apiKey,
                [Constants.SystemConfig.IoT_AllowedIps] = allowedIps,
                [Constants.SystemConfig.IoT_RequireApiKey] = requireApiKey ? "true" : "false"
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

            // Cập nhật Cache tức thì
            IoTSecurityManager.UpdateConfig(apiKey, allowedIps, requireApiKey);

            var lastPush = IoTSecurityManager.LastPushTime;
            return RequestResult<IoTConfigResponse>.Succeed(new IoTConfigResponse
            {
                ApiKey = apiKey,
                AllowedIps = allowedIps,
                RequireApiKey = requireApiKey,
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

    [HttpPost]
    [Route(Common.Url.Default.IoT.MeasurementPointMonitoring)]
    public async Task<RequestResult<MeasurementPointMonitoringResponse>> ReceiveDataAsync([FromBody] IoTDataRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await IsAuthorizedIoTRequestAsync(request?.Values?.Count ?? 0, cancellationToken))
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Authorization", Error = "Truy cập IoT Gateway không hợp lệ (IP hoặc X-IoT-Api-Key không được phép)." }
                });
            }

            var measurementPointMonitoringCommand = new MeasurementPointMonitoringCommand()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(measurementPointMonitoringCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ReceiveDataAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.MeasurementPointMonitoringByDateTime)]
    public async Task<RequestResult<MeasurementPointMonitoringResponse>> ReceiveDataByDateTimeAsync([FromBody] IoTDataRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await IsAuthorizedIoTRequestAsync(request?.Values?.Count ?? 0, cancellationToken))
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Authorization", Error = "Truy cập IoT Gateway không hợp lệ (IP hoặc X-IoT-Api-Key không được phép)." }
                });
            }

            var measurementPointMonitoringCommand = new MeasurementPointMonitoringByDateTimeCommand()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(measurementPointMonitoringCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ReceiveDataByDateTimeAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.MeasurementPointAlert)]
    public async Task<RequestResult<MeasurementPointMonitoringResponse>> ReceiveDataAlertAsync([FromBody] IoTDataRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!await IsAuthorizedIoTRequestAsync(request?.Values?.Count ?? 0, cancellationToken))
            {
                return RequestResult<MeasurementPointMonitoringResponse>.Fail(new[]
                {
                    new ErrorItem { FieldName = "Authorization", Error = "Truy cập IoT Gateway không hợp lệ (IP hoặc X-IoT-Api-Key không được phép)." }
                });
            }

            var measurementPointMonitoringCommand = new MeasurementPointEventCommand()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(measurementPointMonitoringCommand, cancellationToken);
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
