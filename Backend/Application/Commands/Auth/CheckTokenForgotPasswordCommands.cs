using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.Auth;
using Application.DataTransferObjects.Responses.Auth;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Auth;

public class CheckTokenForgotPasswordCommands : CommandBase, IRequest<RequestResult<ForgotPasswordResponse>>
{
    public CheckTokenForgotPasswordRequest Request;
}