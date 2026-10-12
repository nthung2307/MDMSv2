using Application.Commands.Customers;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Customers;
using Application.Handlers.Commands.Customers;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Domain.Entities;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Customers;

public class AddCustomerCommandHandler : ICreateCustomerCommandHandler
{
    private readonly ISnowflakeIdService _snowflakeIdService;
    private readonly ICustomerReadWriteRepository _customerReadWriteRepository;
    private readonly ILoggerService _loggerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserCustomerReadWriteRepository _userCustomerReadWriteRepository;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;

    public AddCustomerCommandHandler(ISnowflakeIdService snowflakeIdService, ILoggerService loggerService, ICurrentUserService currentUserService, ICustomerReadWriteRepository customerReadWriteRepository, IUserCustomerReadWriteRepository userCustomerReadWriteRepository, IUserReadOnlyRepository userReadOnlyRepository, ICustomerReadOnlyRepository customerReadOnlyRepository)
    {
        _snowflakeIdService = snowflakeIdService;
        _loggerService = loggerService;
        _currentUserService = currentUserService;
        _customerReadWriteRepository = customerReadWriteRepository;
        _userCustomerReadWriteRepository = userCustomerReadWriteRepository;
        _userReadOnlyRepository = userReadOnlyRepository;
        _customerReadOnlyRepository = customerReadOnlyRepository;
    }

    public async Task<RequestResult<CreateCustomerResponse>> Handle(CreateCustomerCommands request, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var existCustomerCode = await _customerReadOnlyRepository.IsExistedCustomerCode(request.CustomerCode, 0, cancellationToken);
            if (existCustomerCode)
            {
                return RequestResult<CreateCustomerResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.CustomerCode),
                        Error = "Mã khách hàng đã tồn tại trong hệ thống. Vui lòng thử lại!"
                    }
                });
            }

            var userId = await _snowflakeIdService.GenerateId(cancellationToken);
            var response = new CustomerEntity()
            {
                Id = userId,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                CustomerCode = request.CustomerCode,
                Status = request.Status,
                CreatedBy = currentId,
                CreatedName = userName,
                CreatedTime = DateTimeOffset.UtcNow,
                ModifiedBy = currentId,
                ModifiedName = userName,
                ModifiedTime = DateTimeOffset.UtcNow
            };

            var existUsers = await _userReadOnlyRepository.IsExistUsersAsync(request.UserIds.ToList(), cancellationToken);
            if (!existUsers)
            {
                return RequestResult<CreateCustomerResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.UserIds),
                        Error = "Không tìm thấy người dùng."
                    }
                });
            }

            foreach(var item in request.UserIds)
            {
                var userCustomer = new UserCustomerEntity()
                {
                    Id = await _snowflakeIdService.GenerateId(cancellationToken),
                    UserId = item,
                    CustomerId = userId,
                };
                await _userCustomerReadWriteRepository.AddAsync(userCustomer, cancellationToken);
            }

            await _customerReadWriteRepository.AddAsync(response, cancellationToken);
            await _customerReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _userCustomerReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<CreateCustomerResponse>.Succeed(new CreateCustomerResponse());
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}