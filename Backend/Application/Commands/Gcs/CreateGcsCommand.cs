using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Responses.Gcs;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Gcs;

public class CreateGcsCommand : CommandBase, IRequest<RequestResult<GcsResponse>>
{
    public CreateGcsRequest Request { get; set; }
}