using Application.DataTransferObjects.Responses.Customers;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.Customers;

public class GetCustomerQuery : QueryBase, IRequest<CustomerDto>
{
    public long Id { get; set; }
}