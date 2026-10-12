using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Auth;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Auth;

public class SignInCommands : CommandBase, IRequest<RequestResult<SignInResponse>>
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}