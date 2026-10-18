using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Report;

public class OperatingParameterReportQuery : QueryBase, IRequest<CursorPaginationResponse<OperatingParameterReportResponse>>
{
    public OperatingParameterReportRequest Request { get; set; } = new();
}

public class OperatingParameterReportListQuery : QueryBase, IRequest<List<OperatingParameterReportResponse>>
{
    public OperatingParameterReportRequest Request { get; set; } = new();
}
