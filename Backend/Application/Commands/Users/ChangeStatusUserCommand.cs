using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Users;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Users;

public class ChangeStatusUserCommand : CommandBase, IRequest<RequestResult<UpdateUserResponse>>
{
    public long Id { get; set; }
}