using Application.Commands.Auth;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.Auth;
using Application.DataTransferObjects.Responses.Auth;
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
[Area(Common.Url.Areas.Auth)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.Auth)]
public class AuthController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;

    public AuthController(IMapper mapper, IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService)
    {
        _mapper = mapper;
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
    }

    [HttpPost]
    [Route(Common.Url.Default.Auth.SignIn)]
    public async Task<RequestResult<SignInResponse>> SignInAsync(SignInRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var accountCommands = _mapper.Map<SignInCommands>(request);
            accountCommands.Tracker = _httpContextAccessorService.Tracker();
            return await _mediator.Send(accountCommands, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(SignInAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Auth.RefreshToken)]
    public async Task<RequestResult<SignInResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var accountCommands = _mapper.Map<RefreshTokenCommands>(request);
            accountCommands.Tracker = _httpContextAccessorService.Tracker();
            return await _mediator.Send(accountCommands, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(RefreshTokenAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Auth.ChangePassword)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<ChangePasswordResponse>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updateAccountCommand = new ChangePasswordCommands()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(updateAccountCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(SignInAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Auth.ChangeInformationAccount)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<ChangeInformationAccountResponse>> ChangeInformationAccountAsync(ChangeInformationAccountRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updateAccountCommand = new ChangeInformationAccountCommands()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(updateAccountCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ChangeInformationAccountAsync));
            throw;
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.Auth.CheckTokenForgotPassword)]
    public async Task<RequestResult<ForgotPasswordResponse>> CheckTokenForgotPasswordAsync([FromQuery] CheckTokenForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var formatPasswordCommand = new CheckTokenForgotPasswordCommands()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(formatPasswordCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(SignInAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Auth.ForgotPassword)]
    public async Task<RequestResult<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var formatPasswordCommand = new ForgotPasswordCommands()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(formatPasswordCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(SignInAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Auth.ConfirmForgotPassword)]
    public async Task<RequestResult<ForgotPasswordResponse>> ConfirmForgotPasswordAsync(ConfirmForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var forgotPasswordCommand = new ConfirmForgotPasswordCommands()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(forgotPasswordCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(SignInAsync));
            throw;
        }
    }
}