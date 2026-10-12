using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Report;

public class GetReportConsumptionOutputChartQuery : QueryBase, IRequest<ConsumptionOutputChartResponse?>
{
    public ConsumptionOutputChartRequest Request { get; set; }
}