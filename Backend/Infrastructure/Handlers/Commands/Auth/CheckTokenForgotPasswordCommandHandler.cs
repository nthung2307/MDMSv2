using Application.Commands.Auth;
using Application.Common.Responses.Common.Responses;
using Application.Common.ValueObjects;
using Application.DataTransferObjects.Responses.Auth;
using Application.Handlers.Commands.Auth;
using Application.Repositories.ReadWrite;
using Nobisoft.Core.Logger.Interfaces;

namespace Infrastructure.Handlers.Commands.Auth;

public class CheckTokenForgotPasswordCommandHandler : ICheckTokenForgotPasswordCommandHandlers
{
    private readonly ILoggerService _loggerService;
    private readonly IUserTokenReadWriteRepository _userTokenReadWriteRepository;

    public CheckTokenForgotPasswordCommandHandler(ILoggerService loggerService, IUserTokenReadWriteRepository userTokenReadWriteRepository)
    {
        _loggerService = loggerService;
        _userTokenReadWriteRepository = userTokenReadWriteRepository;
    }

    public async Task<RequestResult<ForgotPasswordResponse>> Handle(CheckTokenForgotPasswordCommands request, CancellationToken cancellationToken)
    {
        try
        {
            var userToken = await _userTokenReadWriteRepository.CheckTokenForgotPasswordAsync(request.Request.Token, cancellationToken);
            if (!userToken)
            {
                return RequestResult<ForgotPasswordResponse>.Fail(new []
                {
                    new ErrorItem()
                    {
                        FieldName = nameof(request.Request.Token),
                        Error = "Không tìm thấy token hoặc token đã hết hạn."
                    }
                });
            }
            return RequestResult<ForgotPasswordResponse>.Succeed(new ForgotPasswordResponse());
        } catch (Exception e)
        {
            _loggerService.LogError(e, nameof(Handle));
            throw;
        }
    }
}