using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.MeasurementPointMonitoring;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.MeasurementPointMonitoring;

public class GetListMeasurementPointEventQuery : QueryBase, IRequest<CursorPaginationResponse<MeasurementPointEventDto>>
{
    public GetListMeasurementPointEventRequest Request { get; set; }
}