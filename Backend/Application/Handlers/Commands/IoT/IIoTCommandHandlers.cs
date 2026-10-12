using Application.Commands.IoT;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.IoT;
using MediatR;

namespace Application.Handlers.Commands.IoT;

public interface IMeasurementPointMonitoringCommandHandlers : IRequestHandler<MeasurementPointMonitoringCommand, RequestResult<MeasurementPointMonitoringResponse>>
{
}

public interface IMeasurementPointMonitoringByDateTimeCommandHandlers : IRequestHandler<MeasurementPointMonitoringByDateTimeCommand, RequestResult<MeasurementPointMonitoringResponse>>
{
}

public interface IMeasurementPointEventCommandHandlers : IRequestHandler<MeasurementPointEventCommand, RequestResult<MeasurementPointMonitoringResponse>>
{
}