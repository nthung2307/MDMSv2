using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.SystemConfig;
using Application.DataTransferObjects.Responses.SystemConfig;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.SystemConfig;

public class CreateUpdateSystemConfigCommands : CommandBase, IRequest<RequestResult<CreateUpdateSystemConfigResponse>>
{
    public List<ConfigRequest> Configs { get; set; }
}