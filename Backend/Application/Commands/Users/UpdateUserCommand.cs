using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.Users;
using Application.DataTransferObjects.Responses.Users;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Users;

public class UpdateUserCommand : CommandBase, IRequest<RequestResult<UpdateUserResponse>>
{
    public UpdateUserRequest UpdateUserRequest { get; set; }
}