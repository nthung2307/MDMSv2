using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Report;

public class ListFinalizedOutputElectricityQuery : QueryBase, IRequest<List<ListFinalizedOutputElectricityResponse>>
{
    public ListFinalizedOutputElectricityRequest Request { get; set; }
}