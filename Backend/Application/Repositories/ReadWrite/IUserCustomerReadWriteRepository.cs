using Application.DataTransferObjects.Requests.Customers;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IUserCustomerReadWriteRepository : IRepository<UserCustomerEntity>
{
    Task<List<UserCustomerEntity>> GetUserCustomerByCustomerIdAsync(long id, CancellationToken cancellationToken);
    Task<List<UserCustomerEntity>> GetUserCustomerByUserIdAsync(long id, CancellationToken cancellationToken);
}