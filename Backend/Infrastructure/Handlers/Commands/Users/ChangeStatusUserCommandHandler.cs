using Application.Commands.Users;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Users;
using Application.Handlers.Commands.Users;
using Application.Repositories.ReadOnly;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Constants;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Users;

public class ChangeStatusUserCommandHandler : IChangeStatusUserCommandHandler
{
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggerService _loggerService;
    private readonly IUserReadOnlyRepository _userReadOnlyRepository;
    private readonly IUserTokenReadWriteRepository _userTokenReadWriteRepository;

    public ChangeStatusUserCommandHandler(ICurrentUserService currentUserService, IUserReadWriteRepository userReadWriteRepository, ILoggerService loggerService, IUserReadOnlyRepository userReadOnlyRepository, IUserTokenReadWriteRepository userTokenReadWriteRepository)
    {
        _currentUserService = currentUserService;
        _userReadWriteRepository = userReadWriteRepository;
        _loggerService = loggerService;
        _userReadOnlyRepository = userReadOnlyRepository;
        _userTokenReadWriteRepository = userTokenReadWriteRepository;
    }

    public async Task<RequestResult<UpdateUserResponse>> Handle(ChangeStatusUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            var userName = _currentUserService.UserName;

            var profile = await _userReadWriteRepository.GetUserByUserIdAsync(command.Id, cancellationToken);
            if (profile == null)
            {
                return RequestResult<UpdateUserResponse>.Fail(new[]
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(command.Id),
                        Error = "Không tìm thấy người dùng."
                    }
                });
            }

            profile.Status = profile.Status == EntityStatus.Active ? EntityStatus.Deactivate : EntityStatus.Active;
            profile.ModifiedName = userName;
            profile.ModifiedTime = DateTimeOffset.Now;
            profile.ModifiedBy = currentId;
            await _userReadWriteRepository.UpdateAsync(profile, cancellationToken);
            await _userReadWriteRepository.SaveChangesAsync(cancellationToken);
            if (profile.Status != EntityStatus.Active)
            {
                var userTokens = await _userTokenReadWriteRepository.GetAllUserTokenAsync(command.Id, cancellationToken);
                if (userTokens.Count > 0)
                {
                    await _userTokenReadWriteRepository.RemoveRangeAsync(userTokens, cancellationToken);
                }
                await _userTokenReadWriteRepository.SaveChangesAsync(cancellationToken);
            }

            return RequestResult<UpdateUserResponse>.Succeed(new UpdateUserResponse());
        } catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            throw;
        }
    }
}