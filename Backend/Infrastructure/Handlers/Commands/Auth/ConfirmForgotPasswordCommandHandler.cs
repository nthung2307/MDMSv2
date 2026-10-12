using Application.Commands.Auth;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Auth;
using Application.Handlers.Commands.Auth;
using Application.Repositories.ReadWrite;
using Application.Services;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Auth;

public class ConfirmForgotPasswordCommandHandler : IConfirmForgotPasswordCommandHandlers
{
    private readonly ILoggerService _loggerService;
    private readonly IUserReadWriteRepository _userReadWriteRepository;
    private readonly IPasswordGeneratorService _passwordGeneratorService;
    private readonly IUserTokenReadWriteRepository _userTokenReadWriteRepository;

    public ConfirmForgotPasswordCommandHandler(ILoggerService loggerService, IUserReadWriteRepository userReadWriteRepository, IPasswordGeneratorService passwordGeneratorService, IUserTokenReadWriteRepository userTokenReadWriteRepository)
    {
        _loggerService = loggerService;
        _userReadWriteRepository = userReadWriteRepository;
        _passwordGeneratorService = passwordGeneratorService;
        _userTokenReadWriteRepository = userTokenReadWriteRepository;
    }

    public async Task<RequestResult<ForgotPasswordResponse>> Handle(ConfirmForgotPasswordCommands request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userReadWriteRepository.GetUserByEmailAsync(request.Request.Email, cancellationToken);
            if (user == null)
                return RequestResult<ForgotPasswordResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.Email),
                        Error = "Tài khoản không tìm thấy, không hoạt động hoặc đã bị xóa"
                    }
                });

            if (request.Request.ConfirmPassword != request.Request.Password)
            {
                return RequestResult<ForgotPasswordResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.Email),
                        Error = "Mật khẩu mới không khớp"
                    }
                });
            }

            var userToken = await _userTokenReadWriteRepository.GetUserTokenToForgotPasswordAsync(user.Id, request.Request.Token, cancellationToken);
            if (userToken == null)
            {
                return RequestResult<ForgotPasswordResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.Email),
                        Error = "Không tìm thấy token của người dùng"
                    }
                });
            }

            user.Password = _passwordGeneratorService.HashPassword(request.Request.Password);
            user.ModifiedBy = user.Id;
            user.ModifiedName = user.UserName;
            user.ModifiedTime = DateTimeOffset.Now;
            await _userReadWriteRepository.UpdateAsync(user, cancellationToken);
            await _userReadWriteRepository.SaveChangesAsync(cancellationToken);
            await _userTokenReadWriteRepository.RemoveAsync(userToken, cancellationToken);
            await _userTokenReadWriteRepository.SaveChangesAsync(cancellationToken);

            return RequestResult<ForgotPasswordResponse>.Succeed(new ForgotPasswordResponse());
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}