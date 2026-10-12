using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.GcsCutOffCycle;
using Application.DataTransferObjects.Requests.GcsMonthlyDetail;
using Application.DataTransferObjects.Responses.GcsMonthlyDetail;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.GcsMonthlyDetail;

public class GetListGcsMonthlyDetailQuery : QueryBase, IRequest<CursorPaginationResponse<GcsMonthlyDetailDto>>
{
    public GetListGcsMonthlyDetailRequest Request { get; set; }
}