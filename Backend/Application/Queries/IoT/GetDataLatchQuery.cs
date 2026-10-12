using Application.DataTransferObjects.Requests.IoT;
using Application.DataTransferObjects.Responses.IoT;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.IoT;

public class GetDataLatchQuery : QueryBase, IRequest<List<GetDataLatchResponse>>
{
    public GetDataLatchRequest Request { get; set; }
}