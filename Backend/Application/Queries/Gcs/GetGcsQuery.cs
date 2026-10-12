using Application.DataTransferObjects.Responses.Gcs;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Gcs;

public class GetGcsQuery : QueryBase, IRequest<GcsDto>
{
    public long Id { get; set; }
}