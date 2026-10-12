using Application.Common.ValueObjects.CursorBased;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.Users;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.Users;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface ICustomerReadOnlyRepository : IRepository<CustomerEntity>
{
    Task<CursorPaginationResponse<CustomerDto>> GetListCustomersAsync(GetListCustomersRequest request, CancellationToken cancellationToken);
    Task<CursorPaginationResponse<CustomerDto>> GetListCustomersForReportAsync(GetListCustomersForReportRequest request, CancellationToken cancellationToken);
    Task<CustomerDto> GetCustomerDetailAsync(long id, CancellationToken cancellationToken);
    Task<bool> IsExistedCustomerCode(string code, long id, CancellationToken cancellationToken);
}