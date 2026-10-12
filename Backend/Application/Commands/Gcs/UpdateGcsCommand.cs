using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using MediatR;
using Nobisoft.Core.Domain.Commands;
using GcsResponse = Application.DataTransferObjects.Responses.Gcs.GcsResponse;

namespace Application.Commands.Gcs;

public class UpdateGcsCommand : CommandBase, IRequest<RequestResult<GcsResponse>>
{
    public UpdateGcsRequest Request { get; set; }
}