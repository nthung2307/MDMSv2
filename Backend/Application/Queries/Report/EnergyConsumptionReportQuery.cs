using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Report;

public class EnergyConsumptionReportQuery : QueryBase, IRequest<EnergyConsumptionReportResponse>
{
    public EnergyConsumptionReportRequest Request { get; set; } = new();
}
