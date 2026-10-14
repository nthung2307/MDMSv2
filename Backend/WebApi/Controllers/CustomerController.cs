using Application.Commands.Customers;
using Application.Common;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Responses.Customers;
using Application.Queries.Customers;
using Application.Repositories.ReadWrite;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
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
public class CustomerController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly IHttpContextAccessorService _httpContextAccessorService;
    private readonly ILoggerService _loggerService;
    private readonly ICustomerReadWriteRepository _customerReadWriteRepository;

    public CustomerController(IMapper mapper, IMediator mediator, IHttpContextAccessorService httpContextAccessorService, ILoggerService loggerService, ICustomerReadWriteRepository customerReadWriteRepository)
    {
        _mapper = mapper;
        _mediator = mediator;
        _httpContextAccessorService = httpContextAccessorService;
        _loggerService = loggerService;
        _customerReadWriteRepository = customerReadWriteRepository;
    }

    [HttpPost]
    [Route(Common.Url.Default.Customer.Create)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<CreateCustomerResponse>> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var accountCommands = _mapper.Map<CreateCustomerCommands>(request);
            accountCommands.Tracker = _httpContextAccessorService.Tracker();
            return await _mediator.Send(accountCommands, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(CreateCustomerAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Customer.Update)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Customer, DefaultRoles.Staff])]
    public async Task<RequestResult<UpdateCustomerResponse>> UpdateCustomerAsync(UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var updateAccountCommand = _mapper.Map<UpdateCustomerCommand>(request);
            updateAccountCommand.Tracker = _httpContextAccessorService.Tracker();
            return await _mediator.Send(updateAccountCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(UpdateCustomerAsync));
            throw;
        }
    }

    [HttpDelete]
    [Route(Common.Url.Default.Customer.Delete)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<DeleteCustomerResponse>> DeleteCustomerAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var deleteAccountCommand = new DeleteCustomerCommand()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(deleteAccountCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(DeleteCustomerAsync));
            throw;
        }
    }

    [HttpPut]
    [Route(Common.Url.Default.Customer.ChangeStatus)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<UpdateCustomerResponse>> ChangeStatusCustomerAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var changeStatusAccountCommand = new ChangeStatusCustomerCommand()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            return await _mediator.Send(changeStatusAccountCommand, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ChangeStatusCustomerAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Customer.List)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<RequestResult<CursorPaginationResponse<CustomerDto>>> GetListCustomersAsync(GetListCustomersRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListCustomersQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<CustomerDto>>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListCustomersAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Customer.ListForReport)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<CursorPaginationResponse<CustomerDto>>> GetListCustomersForReportAsync(GetListCustomersForReportRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetListCustomersForReportQuery()
            {
                Request = request,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<CursorPaginationResponse<CustomerDto>>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetListCustomersAsync));
            throw;
        }
    }

    [HttpGet]
    [Route(Common.Url.Default.Customer.View)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff, DefaultRoles.Customer])]
    public async Task<RequestResult<CustomerDto>> GetCustomerDetailAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetCustomerQuery()
            {
                Id = id,
                Tracker = _httpContextAccessorService.Tracker()
            };
            var account = await _mediator.Send(query, cancellationToken);
            return RequestResult<CustomerDto>.Succeed(account);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(GetCustomerDetailAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Customer.ExportPdf)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportPdfAccountAsync(ExportPdfAndExcelRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var pdfData = await _customerReadWriteRepository.GenerateCustomerPdf(request, cancellationToken);
            return File(pdfData, "application/pdf", $"Export-Customer-{DateTime.Now.ToString("s")}.pdf");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportPdfAccountAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Customer.ExportCsv)]
    // [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin, DefaultRoles.Staff])]
    public async Task<IActionResult> ExportCsvCustomersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var csvData = await _customerReadWriteRepository.GenerateCustomerCsv(cancellationToken);
            return File(
                fileContents: csvData,
                contentType: "application/octet-stream; charset=utf-8",
                fileDownloadName: $"Export-Customer-{DateTime.Now:s}.csv");
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ExportCsvCustomersAsync));
            throw;
        }
    }

    [HttpPost]
    [Route(Common.Url.Default.Customer.ImportCsv)]
    [Permissions(Roles = [DefaultRoles.SuperAdmin, DefaultRoles.Admin])]
    public async Task<RequestResult<ImportCustomerResponse>> ImportCsvCustomersAsync([FromForm(Name = "file")] IFormFile? file, CancellationToken cancellationToken)
    {
        try
        {
            var uploadFile = file ?? (Request.HasFormContentType ? Request.Form?.Files?["file"] ?? Request.Form?.Files?.FirstOrDefault() : null);
            if (uploadFile == null || uploadFile.Length == 0)
            {
                return RequestResult<ImportCustomerResponse>.Fail(new[]
                {
                    new ErrorItem { FieldName = "file", Error = "Vui lòng chọn tệp CSV hợp lệ!" }
                });
            }

            var fileName = uploadFile.FileName ?? string.Empty;
            if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return RequestResult<ImportCustomerResponse>.Fail(new[]
                {
                    new ErrorItem { FieldName = "file", Error = "Hệ thống chỉ hỗ trợ định dạng tệp .csv!" }
                });
            }

            using var stream = uploadFile.OpenReadStream();
            var command = new ImportCustomersCommand
            {
                FileStream = stream,
                Tracker = _httpContextAccessorService.Tracker()
            };

            return await _mediator.Send(command, cancellationToken);
        }
        catch (Exception e)
        {
            _loggerService.LogError(e, nameof(ImportCsvCustomersAsync));
            throw;
        }
    }
}