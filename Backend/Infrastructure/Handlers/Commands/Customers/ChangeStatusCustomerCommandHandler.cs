using Application.Commands.Customers;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Customers;
using Application.Handlers.Commands.Customers;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Customers;

public class ChangeStatusCustomerCommandHandler : IChangeStatusCustomerCommandHandler
{
    private readonly ICustomerReadWriteRepository _customerReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly IUserCustomerReadWriteRepository _userCustomerReadWriteRepository;
    private readonly IUserReadWriteRepository _userReadWriteRepository;

    public ChangeStatusCustomerCommandHandler(ICurrentUserService currentUserService, ICustomerReadWriteRepository customerReadWriteRepository, ILoggerService loggerService, IUserCustomerReadWriteRepository userCustomerReadWriteRepository, IUserReadWriteRepository userReadWriteRepository)
    {
        _currentUserService = currentUserService;
        _customerReadWriteRepository = customerReadWriteRepository;
        _loggerService = loggerService;
        _userCustomerReadWriteRepository = userCustomerReadWriteRepository;
        _userReadWriteRepository = userReadWriteRepository;
    }

    public async Task<RequestResult<UpdateCustomerResponse>> Handle(ChangeStatusCustomerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var profile = await _customerReadWriteRepository.GetCustomerByCustomerIdAsync(command.Id, cancellationToken);
            if (profile == null)
            {
                return RequestResult<UpdateCustomerResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Không tìm thấy khách hàng."
                    }
                });
            }

            if (profile.Status == EntityStatus.Active)
            {
                if (profile.MeasurePoints.Any(x => x.Status == EntityStatus.Active))
                {
                    return RequestResult<UpdateCustomerResponse>.Fail(new[]
                    {
                        new ErrorItem()
                        {
                            FieldName = nameof(command.Id),
                            Error = "Vui lòng inactive tất cả các điểm đo trước khi inactive khách hàng."
                        }
                    });
                }

                var userCustomer= await _userCustomerReadWriteRepository.GetUserCustomerByCustomerIdAsync(command.Id, cancellationToken);
                if (userCustomer.Count > 0)
                {
                    var userIds = userCustomer.Select(uc => uc.UserId).ToList();
                    var users = await _userReadWriteRepository.GetUsersByUserIdsAsync(userIds, cancellationToken);
                    if (users.Count > 0)
                    {
                        foreach (var user in users)
                        {
                            user.Status = EntityStatus.Deactivate;
                        }

                        await _userReadWriteRepository.UpdateRangeAsync(users, cancellationToken);
                    }
                }
            }

            profile.Status = profile.Status == EntityStatus.Active ? EntityStatus.Deactivate : EntityStatus.Active;
            profile.ModifiedName = userName;
            profile.ModifiedTime = DateTimeOffset.Now;
            profile.ModifiedBy = currentId;
            await _customerReadWriteRepository.UpdateAsync(profile, cancellationToken);
            await _customerReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _userReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<UpdateCustomerResponse>.Succeed(new UpdateCustomerResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}