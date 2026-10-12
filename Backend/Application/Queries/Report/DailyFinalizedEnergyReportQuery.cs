using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Report;

public class DailyFinalizedEnergyReportQuery : QueryBase, IRequest<CursorPaginationResponse<FinalizedEnergyReportResponse>>
{
    public DailyFinalizedEnergyReportRequest Request { get; set; }
}