using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.MeasurementPoints;

public class ChangeStatusMeasurementPointCommand : CommandBase, IRequest<RequestResult<MeasurementPointResponse>>
{
    public long Id { get; set; }
}