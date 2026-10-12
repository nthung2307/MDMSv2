using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Responses.Customers;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Customers;

public class GetListCustomersQuery : QueryBase, IRequest<CursorPaginationResponse<CustomerDto>>
{
    public GetListCustomersRequest Request { get; set; }
}