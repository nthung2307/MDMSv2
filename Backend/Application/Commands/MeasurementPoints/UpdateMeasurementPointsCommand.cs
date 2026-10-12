using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.MeasurementPoints;

public class UpdateMeasurementPointsCommand : CommandBase, IRequest<RequestResult<MeasurementPointResponse>>
{
    public UpdateMeasurementPointsRequest Request { get; set; }
}