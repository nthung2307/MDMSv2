using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Users;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Users;

public class DeleteUserCommand : CommandBase, IRequest<RequestResult<DeleteUserResponse>>
{
    public long Id { get; set; }
}