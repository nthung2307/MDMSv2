using Application.Commands.Auth;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Auth;
using MediatR;

namespace Application.Handlers.Commands.Auth;

public interface IAuthCommandHandlers : IRequestHandler<SignInCommands, RequestResult<SignInResponse>>
{
}

public interface IChangePasswordCommandHandlers : IRequestHandler<ChangePasswordCommands, RequestResult<ChangePasswordResponse>>
{
}

public interface IChangeInformationAccountCommandHandlers : IRequestHandler<ChangeInformationAccountCommands, RequestResult<ChangeInformationAccountResponse>>
{
}

public interface IRefreshTokenCommandHandlers : IRequestHandler<RefreshTokenCommands, RequestResult<SignInResponse>>
{
}

public interface IForgotPasswordCommandHandlers : IRequestHandler<ForgotPasswordCommands, RequestResult<ForgotPasswordResponse>>
{
}

public interface IConfirmForgotPasswordCommandHandlers : IRequestHandler<ConfirmForgotPasswordCommands, RequestResult<ForgotPasswordResponse>>
{
}

public interface ICheckTokenForgotPasswordCommandHandlers : IRequestHandler<CheckTokenForgotPasswordCommands, RequestResult<ForgotPasswordResponse>>
{
}