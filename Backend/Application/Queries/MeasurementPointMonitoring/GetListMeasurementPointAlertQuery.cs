using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.MeasurementPointMonitoring;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.MeasurementPointMonitoring;

public class GetListMeasurementPointAlertQuery : QueryBase, IRequest<CursorPaginationResponse<MeasurementPointAlertDto>>
{
    public GetListMeasurementPointAlertRequest Request { get; set; }
}