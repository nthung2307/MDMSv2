using Application.Commands.Users;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Users;
using Application.Handlers.Commands.Users;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Users;

public class DeleteUserCommandHandler : IDeleteUserCommandHandler
{
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly IUserTokenReadWriteRepository _userTokenReadWriteRepository;

    public DeleteUserCommandHandler(ICurrentUserService currentUserService, IUserReadWriteRepository userReadWriteRepository, ILoggerService loggerService, IUserTokenReadWriteRepository userTokenReadWriteRepository)
    {
        _currentUserService = currentUserService;
        _userReadWriteRepository = userReadWriteRepository;
        _loggerService = loggerService;
        _userTokenReadWriteRepository = userTokenReadWriteRepository;
    }

    public async Task<RequestResult<DeleteUserResponse>> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var profile = await _userReadWriteRepository.GetUserByUserIdAsync(command.Id, cancellationToken);
            if (profile == null)
            {
                return RequestResult<DeleteUserResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Không tìm thấy người dùng."
                    }
                });
            }

            if (profile.Status == EntityStatus.Active)
            {
                return RequestResult<DeleteUserResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Đang ở trạng thái hoạt động."
                    }
                });
            }

            profile.Status = EntityStatus.Deleted;
            profile.Deleted = true;
            profile.DeletedBy = currentId;
            profile.DeletedTime = DateTimeOffset.Now;
            profile.DeletedName = userName;
            await _userReadWriteRepository.UpdateAsync(profile, cancellationToken);

            // Save all changes
            await _userReadWriteRepository.SaveChangesAsync(cancellationToken);

            var userTokens = await _userTokenReadWriteRepository.GetAllUserTokenAsync(command.Id, cancellationToken);
            if (userTokens.Count > 0)
            {
                await _userTokenReadWriteRepository.RemoveRangeAsync(userTokens, cancellationToken);
            }
            await _userTokenReadWriteRepository.SaveChangesAsync(cancellationToken);
            return RequestResult<DeleteUserResponse>.Succeed(new DeleteUserResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }
    }
}