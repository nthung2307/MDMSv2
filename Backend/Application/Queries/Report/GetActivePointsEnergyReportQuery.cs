using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Report;

public class GetActivePointsEnergyReportQuery : QueryBase, IRequest<List<ActivePointsEnergyReportResponse>>
{
    public ActivePointsEnergyReportRequest Request { get; set; } = null!;
}
