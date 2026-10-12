using Application.Commands.TagName;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.TagName;
using Application.DataTransferObjects.Responses.TagName;
using Application.Queries.TagName;
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
[Area(Common.Url.Areas.TagName)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.TagName)]
public class TagNameController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;

    public TagNameController(IMapper mapper, IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService)
    {
        _mapper = mapper;
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
    }

    [HttpPost]
    [Route(Common.Url.Default.TagName.Create)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<TagNameResponse>> CreateTagNameAsync(CreateListTagNameRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tagNameCommands = _mapper.Map<CreateTagNameCommands>(request);
            tagNameCommands.Tracker = _httpContextAccessorService.Tracker();
            return await _mediator.Send(tagNameCommands, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(CreateTagNameAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.TagName.Update)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<TagNameResponse>> UpdateTagNameAsync(UpdateListTagNameRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updateTagNameCommand = _mapper.Map<UpdateTagNameCommand>(request);
            updateTagNameCommand.Tracker = _httpContextAccessorService.Tracker();
            return await _mediator.Send(updateTagNameCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(UpdateTagNameAsync));
            throw;
        }
    }

    [HttpDelete]
    [Route(Common.Url.Default.TagName.Delete)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<TagNameResponse>> DeleteTagNameAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var deleteTagNameCommand = new DeleteTagNameCommand()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(deleteTagNameCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(DeleteTagNameAsync));
            throw;
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.TagName.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<List<TagNameDto>>> GetListTagNamesAsync(long measurementPoint, string? type, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListTagByMeasurementPointQuery()
            {
                MeasurementPointId = measurementPoint,
                Type = type,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<List<TagNameDto>>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListTagNamesAsync));
            throw;
        }
    }

}