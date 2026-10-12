using Application.Commands.PeriodicEnergyStatement;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using Application.Queries.PeriodicEnergyStatement;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.Logger.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

[ApiController]
[Area(Common.Url.Areas.PeriodicEnergyStatement)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.PeriodicEnergyStatement)]
public class PeriodicEnergyStatementController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;

    public PeriodicEnergyStatementController(IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService)
    {
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
    }

    [HttpPost]
    [Route(Common.Url.Default.PeriodicEnergyStatement.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<CursorPaginationResponse<PeriodicEnergyStatementDto>>> GetListPeriodicEnergyStatementAsync(GetListPeriodicEnergyStatementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListPeriodicEnergyStatementQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var gcs = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<PeriodicEnergyStatementDto>>.Succeed(gcs);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListPeriodicEnergyStatementAsync));
            throw;
        }
    }
    
    [HttpGet]
    [Route(Common.Url.Default.PeriodicEnergyStatement.CheckStatusSendMail)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<bool>> CheckStatusSendMailAsync([FromQuery]SendAllMailRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new CheckStatusSendMailQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var gcs = await _mediator.Send(query, cancellationToken);
            return RequestResult<bool>.Succeed(gcs);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListPeriodicEnergyStatementAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.PeriodicEnergyStatement.SendMail)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<PeriodicEnergyStatementResponse>> SendMailPeriodicEnergyStatementAsync(SendMailRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new SendMailPeriodicEnergyStatementCommand()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(query, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListPeriodicEnergyStatementAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.PeriodicEnergyStatement.SendAllMail)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<PeriodicEnergyStatementResponse>> SendAllMailPeriodicEnergyStatementAsync(SendAllMailRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new SendAllMailPeriodicEnergyStatementCommand()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(query, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(SendAllMailPeriodicEnergyStatementAsync));
            throw;
        }
    }
}