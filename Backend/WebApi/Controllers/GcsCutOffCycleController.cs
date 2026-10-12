using Application.Commands.Gcs;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Requests.GcsCutOffCycle;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.GcsCutOffCycle;
using Application.DataTransferObjects.Responses.IoT;
using Application.Queries.Gcs;
using Application.Queries.GcsCutOffCycle;
using Application.Repositories.ReadOnly;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nobisoft.Core.Authorization;
using Nobisoft.Core.Authorization.Attributes;
using Nobisoft.Core.HttpContextAccessor.Interfaces;
using Nobisoft.Core.Logger.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Controllers;

[ApiController]
[Area(Common.Url.Areas.GcsCutOffCycle)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.GcsCutOffCycle)]
public class GcsCutOffCycleController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;

    public GcsCutOffCycleController(IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService)
    {
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
    }

    [HttpPost]
    [Route(Common.Url.Default.GcsCutOffCycle.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<CursorPaginationResponse<GcsCutOffCycleResponse>>> GetListGcsAsync(GetListGcsCutOffCycleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListGcsCutOffCycleQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var gcs = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<GcsCutOffCycleResponse>>.Succeed(gcs);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListGcsAsync));
            throw;
        }
    }
    
    [HttpGet]
    [Route(Common.Url.Default.GcsCutOffCycle.ListYear)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<List<int>>> GetListYearGcsCutOffAsync(long gcsId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListYearGcsCutOffCycleQuery()
            {
                GcsId = gcsId,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var gcs = await _mediator.Send(query, cancellationToken);
            return RequestResult<List<int>>.Succeed(gcs);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListGcsAsync));
            throw;
        }
    }
}