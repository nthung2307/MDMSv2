using Application.Commands.Users;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Users;
using MediatR;

namespace Application.Handlers.Commands.Users;

public interface ICreateUserCommandHandler : IRequestHandler<CreateUserCommands, RequestResult<CreateUserResponse>>
{
}

public interface IUpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, RequestResult<UpdateUserResponse>>
{
}

public interface IDeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, RequestResult<DeleteUserResponse>>
{
}

public interface IChangeStatusUserCommandHandler : IRequestHandler<ChangeStatusUserCommand, RequestResult<UpdateUserResponse>>
{
}