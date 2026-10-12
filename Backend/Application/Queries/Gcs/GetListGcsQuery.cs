using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Gcs;
using Application.DataTransferObjects.Responses.Gcs;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Gcs;

public class GetListGcsQuery : QueryBase, IRequest<CursorPaginationResponse<GcsDto>>
{
    public GetListGcsRequest Request { get; set; }
}