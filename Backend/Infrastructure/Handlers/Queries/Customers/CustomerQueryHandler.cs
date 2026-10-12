using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Responses.Customers;
using Application.Queries.Customers;
using Application.Repositories.ReadOnly;
using MediatR;

namespace Infrastructure.Handlers.Queries.Customers;

public class CustomerQueryHandler :
    IRequestHandler<GetListCustomersQuery, CursorPaginationResponse<CustomerDto>>,
    IRequestHandler<GetListCustomersForReportQuery, CursorPaginationResponse<CustomerDto>>,
    IRequestHandler<GetCustomerQuery, CustomerDto>
{
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;

    public CustomerQueryHandler(ICustomerReadOnlyRepository customerReadOnlyRepository)
    {
        _customerReadOnlyRepository = customerReadOnlyRepository;
    }
    public async Task<CursorPaginationResponse<CustomerDto>> Handle(GetListCustomersQuery request, CancellationToken cancellationToken)
    {
        return await _customerReadOnlyRepository.GetListCustomersAsync(request.Request, cancellationToken);
    }

    public async Task<CursorPaginationResponse<CustomerDto>> Handle(GetListCustomersForReportQuery request, CancellationToken cancellationToken)
    {
        return await _customerReadOnlyRepository.GetListCustomersForReportAsync(request.Request, cancellationToken);
    }

    public async Task<CustomerDto> Handle(GetCustomerQuery query, CancellationToken cancellationToken)
    {
        return await _customerReadOnlyRepository.GetCustomerDetailAsync(query.Id, cancellationToken);
    }
}