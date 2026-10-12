using Application.Commands.Customers;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.Users;
using Application.Handlers.Commands.Customers;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Customers;

public class DeleteCustomerCommandHandler : IDeleteCustomerCommandHandler
{
    private readonly ICustomerReadWriteRepository _customerReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;


    public DeleteCustomerCommandHandler(ICurrentUserService currentUserService, ICustomerReadWriteRepository customerReadWriteRepository, ILoggerService loggerService)
    {
        _currentUserService = currentUserService;
        _customerReadWriteRepository = customerReadWriteRepository;
        _loggerService = loggerService;
    }

    public async Task<RequestResult<DeleteCustomerResponse>> Handle(DeleteCustomerCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var profile = await _customerReadWriteRepository.GetCustomerByCustomerIdAsync(command.Id, cancellationToken);
            if (profile == null)
            {
                return RequestResult<DeleteCustomerResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Khách hàng không tồn tại."
                    }
                });
            }

            if (profile.Status == EntityStatus.Active)
            {
                return RequestResult<DeleteCustomerResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Đang ở trạng thái hoạt động hoặc còn liên kết với điểm đo."
                    }
                });
            }

            if (profile.MeasurePoints.Count > 0 && profile.MeasurePoints.Any(mp => mp.Status != EntityStatus.Deleted))
            {
                return RequestResult<DeleteCustomerResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Đang ở trạng thái hoạt động hoặc còn liên kết với điểm đo."
                    }
                });
            }

            profile.Status = EntityStatus.Deleted;
            profile.Deleted = true;
            profile.DeletedBy = currentId;
            profile.DeletedTime = DateTimeOffset.Now;
            profile.DeletedName = userName;
            
            await _customerReadWriteRepository.UpdateAsync(profile, cancellationToken);
            await _customerReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<DeleteCustomerResponse>.Succeed(new DeleteCustomerResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }

    }
}