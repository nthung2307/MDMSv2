using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Gcs;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Gcs;

public class ChangeStatusGcsCommand : CommandBase, IRequest<RequestResult<GcsResponse>>
{
    public long Id { get; set; }
}