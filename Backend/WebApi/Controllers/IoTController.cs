using System.Net;
using Application.Commands.IoT;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.IoT;
using Application.Repositories.ReadOnly;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
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
    private readonly IConfiguration _configuration;

    public IoTController(
        IMediator mediator,
        IHttpContextAccessorService httpContextAccessorService,
        ILoggerService loggerService,
        IGcsCutOffCycleReadOnlyRepository gcsCutOffCycleReadOnlyRepository,
        IMeasurementPointMonitoringReadOnlyRepository pointMonitoringReadOnlyRepository,
        IConfiguration configuration)
    {
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _gcsCutOffCycleReadOnlyRepository = gcsCutOffCycleReadOnlyRepository;
        _pointMonitoringReadOnlyRepository = pointMonitoringReadOnlyRepository;
        _configuration = configuration;
    }

    private bool IsAuthorizedIoTRequest()
    {
        // 1. Cho phép nếu đã xác thực bằng JWT hợp lệ
        if (HttpContext.User?.Identity?.IsAuthenticated == true)
            return true;

        // 2. Kiểm tra Header X-IoT-Api-Key nếu được cung cấp
        var configuredApiKey = _configuration["IoT:ApiKey"];
        if (HttpContext.Request.Headers.TryGetValue("X-IoT-Api-Key", out var providedKey) &&
            !string.IsNullOrWhiteSpace(providedKey))
        {
            if (!string.IsNullOrWhiteSpace(configuredApiKey) &&
                string.Equals(providedKey.ToString().Trim(), configuredApiKey.Trim(), StringComparison.Ordinal))
            {
                return true;
            }
        }

        // 3. Kiểm tra IP Whitelist (Loopback 127.0.0.1 / ::1, mạng nội bộ OT/LAN hoặc IP cấu hình của Kepware)
        var remoteIp = HttpContext.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            if (remoteIp.IsIPv4MappedToIPv6)
                remoteIp = remoteIp.MapToIPv4();

            if (IPAddress.IsLoopback(remoteIp))
                return true;

            var ipStr = remoteIp.ToString();

            // Kiểm tra khớp với IP máy chủ Kepware trong BaseUrlGetDataFromServerIoT hoặc IoT:AllowedIps
            var iotServerUrl = _configuration["BaseUrlGetDataFromServerIoT"];
            if (!string.IsNullOrWhiteSpace(iotServerUrl) &&
                Uri.TryCreate(iotServerUrl, UriKind.Absolute, out var uri) &&
                string.Equals(uri.Host, ipStr, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var allowedIps = _configuration["IoT:AllowedIps"];
            if (!string.IsNullOrWhiteSpace(allowedIps))
            {
                var list = allowedIps.Split(',', ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (list.Any(ip => string.Equals(ip, ipStr, StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            // Cho phép dải mạng LAN nội bộ (10.x.x.x, 172.16-31.x.x, 192.168.x.x) nếu chưa bắt buộc khóa chặt API Key
            var bytes = remoteIp.GetAddressBytes();
            if (bytes.Length == 4)
            {
                bool isPrivateLan = bytes[0] == 10 ||
                                    (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                                    (bytes[0] == 192 && bytes[1] == 168);
                if (isPrivateLan && string.IsNullOrWhiteSpace(configuredApiKey))
                    return true;
            }
        }

        return false;
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.MeasurementPointMonitoring)]
    public async Task<RequestResult<MeasurementPointMonitoringResponse>> ReceiveDataAsync([FromBody] IoTDataRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!IsAuthorizedIoTRequest())
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
            if (!IsAuthorizedIoTRequest())
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
            _loggerService.LogError(e, nameof(ReceiveDataAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.MeasurementPointAlert)]
    public async Task<RequestResult<MeasurementPointMonitoringResponse>> ReceiveDataAlertAsync([FromBody] IoTDataRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!IsAuthorizedIoTRequest())
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
            _loggerService.LogError(e, nameof(ReceiveDataAsync));
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