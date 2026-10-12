using Application.Commands.MeasurementPoints;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using MediatR;

namespace Application.Handlers.Commands.MeasurementPoints;

public interface ICreateMeasurementPointsCommandHandler : IRequestHandler<CreateMeasurementPointsCommands, RequestResult<MeasurementPointResponse>>
{
}

public interface IUpdateMeasurementPointsCommandHandler : IRequestHandler<UpdateMeasurementPointsCommand, RequestResult<MeasurementPointResponse>>
{
}

public interface IDeleteMeasurementPointsCommandHandler : IRequestHandler<DeleteMeasurementPointsCommand, RequestResult<MeasurementPointResponse>>
{
}

public interface IChangeStatusMeasurementPointsCommandHandler : IRequestHandler<ChangeStatusMeasurementPointCommand, RequestResult<MeasurementPointResponse>>
{
}
