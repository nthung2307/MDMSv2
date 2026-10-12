using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Auth;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Auth;

public class RefreshTokenCommands : CommandBase, IRequest<RequestResult<SignInResponse>>
{
    public string RefreshToken { get; set; }
}