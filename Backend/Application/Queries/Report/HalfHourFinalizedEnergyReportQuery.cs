using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Report;

public class HalfHourFinalizedEnergyReportQuery : QueryBase, IRequest<CursorPaginationResponse<FinalizedEnergyReportResponse>>
{
    public HalfHourFinalizedEnergyReportRequest Request { get; set; }
}