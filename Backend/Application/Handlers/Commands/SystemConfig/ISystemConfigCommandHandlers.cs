using Application.Commands.SystemConfig;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.SystemConfig;
using MediatR;

namespace Application.Handlers.Commands.SystemConfig;

public interface ICreateUpdateSystemConfigCommandHandler : IRequestHandler<CreateUpdateSystemConfigCommands, RequestResult<CreateUpdateSystemConfigResponse>>
{
}