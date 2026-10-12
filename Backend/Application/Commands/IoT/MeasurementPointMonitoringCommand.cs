using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.IoT;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.IoT;

public class MeasurementPointMonitoringCommand : CommandBase, IRequest<RequestResult<MeasurementPointMonitoringResponse>>
{
    public IoTDataRequest Request { get; set; }
}