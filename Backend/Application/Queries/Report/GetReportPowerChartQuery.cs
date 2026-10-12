using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Report;

public class GetReportPowerChartQuery : QueryBase, IRequest<PowerChartResponse?>
{
    public PowerChartRequest Request { get; set; }
}