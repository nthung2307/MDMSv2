using Application.Commands.SystemConfig;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.SystemConfig;
using Application.DataTransferObjects.Responses.SystemConfig;
using Application.Queries.SystemConfig;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.Logger.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

[ApiController]
[Area(Common.Url.Areas.SystemConfig)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.SystemConfig)]
public class SystemConfigController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;

    public SystemConfigController(IMapper mapper, IMediator mediator,
        IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService)
    {
        _mapper = mapper;
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
    }
    
    [HttpPost]
    [Route(Common.Url.Default.SystemConfig.Create)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<CreateUpdateSystemConfigResponse>> CreateUpdateSystemConfigAsync(CreateUpdateSystemConfigRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var systemConfigCommands = _mapper.Map<CreateUpdateSystemConfigCommands>(request);
            systemConfigCommands.Tracker = _httpContextAccessorService.Tracker();
            return await _mediator.Send(systemConfigCommands, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(CreateUpdateSystemConfigAsync));
            throw;
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.SystemConfig.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<List<SystemConfigDto>>> GetSystemConfigsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListConfigQuery()
            {
                Tracker = _httpContextAccessorService.Tracker()
            };
            var configs = await _mediator.Send(query, cancellationToken);
            return RequestResult<List<SystemConfigDto>>.Succeed(configs);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetSystemConfigsAsync));
            throw;
        }
    }
}