using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.Auth;
using Application.DataTransferObjects.Responses.Auth;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Auth;

public class ForgotPasswordCommands : CommandBase, IRequest<RequestResult<ForgotPasswordResponse>>
{
    public ForgotPasswordRequest Request;
}