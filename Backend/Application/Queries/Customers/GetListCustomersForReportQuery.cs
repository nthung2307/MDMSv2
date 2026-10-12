using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Responses.Customers;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Customers;

public class GetListCustomersForReportQuery : QueryBase, IRequest<CursorPaginationResponse<CustomerDto>>
{
    public GetListCustomersForReportRequest Request { get; set; }
}