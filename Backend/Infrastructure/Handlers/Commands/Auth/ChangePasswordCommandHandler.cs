using Application.Commands.Auth;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Auth;
using Application.Handlers.Commands.Auth;
using Application.Repositories.ReadWrite;
using Application.Services;
using Nobisoft.Core.Authorization.Interfaces;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Auth;

public class ChangePasswordCommandHandler : IChangePasswordCommandHandlers
{
    private readonly ILoggerService _loggerService;
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly IPasswordGeneratorService _passwordGeneratorService;
    private readonly ICurrentUserService _currentUserService;

    public ChangePasswordCommandHandler(ILoggerService loggerService, IUserReadWriteRepository userReadWriteRepository, IPasswordGeneratorService passwordGeneratorService, ICurrentUserService currentUserService)
    {
        _loggerService = loggerService;
        _userReadWriteRepository = userReadWriteRepository;
        _passwordGeneratorService = passwordGeneratorService;
        _currentUserService = currentUserService;
    }

    public async Task<RequestResult<ChangePasswordResponse>> Handle(ChangePasswordCommands request, CancellationToken cancellationToken)
    {
        try
        {
            var currentId = _currentUserService.UserId;
            // Find the user with status is not inactive
            var user = await _userReadWriteRepository.GetUserByUserIdAsync(currentId, cancellationToken);
            if (user == null)
                return RequestResult<ChangePasswordResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(currentId),
                        Error = $"Tài khoản không tìm thấy, không hoạt động hoặc đã bị xóa"
                    }
                });

            if (request.Request.NewPassword != request.Request.ConfirmPassword)
            {
                return RequestResult<ChangePasswordResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(currentId),
                        Error = "Mật khẩu mới không khớp"
                    }
                });
            }

            // Verify password or temporary password, if both of them are wrong, return
            if (!_passwordGeneratorService.VerifyHashPassword(user.Password, request.Request.OldPassword))
            {
                return RequestResult<ChangePasswordResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.OldPassword),
                        Error = "Mật khẩu cũ không đúng"
                    }
                });
            }

            user.Password = _passwordGeneratorService.HashPassword(request.Request.NewPassword);
            user.ModifiedBy = currentId;
            user.ModifiedName = user.UserName;
            user.ModifiedTime = DateTimeOffset.Now;
            await _userReadWriteRepository.UpdateAsync(user, cancellationToken);
            await _userReadWriteRepository.SaveChangesAsync(cancellationToken);

            return RequestResult<ChangePasswordResponse>.Succeed(new ChangePasswordResponse());
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}