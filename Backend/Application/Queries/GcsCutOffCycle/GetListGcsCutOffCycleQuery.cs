using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Requests.GcsCutOffCycle;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.GcsCutOffCycle;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.GcsCutOffCycle;

public class GetListGcsCutOffCycleQuery : QueryBase, IRequest<CursorPaginationResponse<GcsCutOffCycleResponse>>
{
    public GetListGcsCutOffCycleRequest Request { get; set; }
}