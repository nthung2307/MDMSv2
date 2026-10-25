using System.Text;
using Application.Commands.Users;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.Users;
using Application.DataTransferObjects.Responses.Users;
using Application.Queries.Users;
using Application.Repositories.ReadWrite;
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
[Area(Common.Url.Areas.Users)]
[ApiVersion(Constants.ApiVersion.V1_0)]
[SwaggerTag(Common.Url.Areas.Users)]
public class UserController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;
    private readonly IUserReadWriteRepository _userReadWriteRepository;

    public UserController(IMapper mapper, IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService, IUserReadWriteRepository userReadWriteRepository)
    {
        _mapper = mapper;
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _userReadWriteRepository = userReadWriteRepository;
    }

    [HttpPost]
    [Route(Common.Url.Default.Account.Create)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<CreateUserResponse>> CreateAccountAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var accountCommands = _mapper.Map<CreateUserCommands>(request);
            accountCommands.Tracker = _httpContextAccessorService.Tracker();
            return await _mediator.Send(accountCommands, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(CreateAccountAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Account.Update)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<UpdateUserResponse>> UpdateAccountAsync(UpdateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updateAccountCommand = new UpdateUserCommand()
            {
                UpdateUserRequest = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(updateAccountCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(UpdateAccountAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Account.ChangeStatus)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<UpdateUserResponse>> ChangeStatusAccountAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var changeStatusUser = new ChangeStatusUserCommand()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(changeStatusUser, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ChangeStatusAccountAsync));
            throw;
        }
    }

    [HttpDelete]
    [Route(Common.Url.Default.Account.Delete)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin])]
    public async Task<RequestResult<DeleteUserResponse>> DeleteAccountAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var deleteAccountCommand = new DeleteUserCommand()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(deleteAccountCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(DeleteAccountAsync));
            throw;
        }
    }
    
    [HttpPost]
    [Route(Common.Url.Default.Account.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<CursorPaginationResponse<UserDto>>> GetListUsersAsync(GetListUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListUserQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<UserDto>>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListUsersAsync));
            throw;
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.Account.View)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<UserDto>> GetUserDetailAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetUserQuery()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<UserDto>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetUserDetailAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Account.ExportPdf)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfAccountAsync(ExportPdfAndExcelRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _userReadWriteRepository.GenerateCustomerPdf(request, cancellationToken);
            return File(pdfData, "application/pdf", $"Export-User-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfAccountAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Account.ExportCsv)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvAccountsAsync([FromBody] ExportUserCsvRequest? request, CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _userReadWriteRepository.GenerateCustomerCsv(request, cancellationToken);
            return File(
                fileContents: csvData,
                contentType: "text/csv; charset=utf-8",
                fileDownloadName: $"Export-User-{DateTime.Now:s}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvAccountsAsync));
            throw;
        }
    }
}