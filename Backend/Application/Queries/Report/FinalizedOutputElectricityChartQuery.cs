using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Report;

public class FinalizedOutputElectricityChartQuery : QueryBase, IRequest<FinalizedOutputElectricityChartResponse?>
{
    public FinalizedOutputElectricityChartRequest Request { get; set; }
}