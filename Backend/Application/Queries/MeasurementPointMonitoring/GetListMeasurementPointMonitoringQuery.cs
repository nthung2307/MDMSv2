using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.MeasurementPointMonitoring;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.MeasurementPointMonitoring;

public class GetListMeasurementPointMonitoringQuery : QueryBase, IRequest<CursorPaginationResponse<ListMeasurementPointMonitoringDto>>
{
    public GetListMeasurementPointMonitoringRequest Request { get; set; }
}