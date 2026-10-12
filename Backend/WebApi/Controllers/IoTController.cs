using Application.Commands.IoT;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.IoT;
using Application.Repositories.ReadOnly;
using MediatR;
using Microsoft.AspNetCore.Mvc;
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

    public IoTController(IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService, IGcsCutOffCycleReadOnlyRepository gcsCutOffCycleReadOnlyRepository, IMeasurementPointMonitoringReadOnlyRepository pointMonitoringReadOnlyRepository)
    {
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _gcsCutOffCycleReadOnlyRepository = gcsCutOffCycleReadOnlyRepository;
        _pointMonitoringReadOnlyRepository = pointMonitoringReadOnlyRepository;
    }

    [HttpPost]
    [Route(Common.Url.Default.IoT.MeasurementPointMonitoring)]
    public async Task<RequestResult<MeasurementPointMonitoringResponse>> ReceiveDataAsync([FromBody] IoTDataRequest request, CancellationToken cancellationToken)
    {
        try
        {
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
    public async Task<RequestResult<CursorPaginationResponse<GetDataLatchResponse>>> GetDataLatchAsync(GetDataLatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return await _gcsCutOffCycleReadOnlyRepository.GetDataLatchResponseAsync(request, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ReceiveDataAsync));
            throw;
        }
    }
}