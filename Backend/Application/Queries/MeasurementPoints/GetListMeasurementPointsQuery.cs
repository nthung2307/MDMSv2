using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.MeasurementPoints;

public class GetListMeasurementPointsQuery : QueryBase, IRequest<CursorPaginationResponse<ListMeasurementPointDto>>
{
    public GetListMeasurementPointsRequest Request { get; set; }
}