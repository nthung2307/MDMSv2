using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IGcsMonthlyDetailReadWriteRepository : IRepository<GcsMonthlyDetailEntity>
{
    // Task<CustomerEntity?> GetCustomerByCustomerIdAsync(long id, CancellationToken cancellationToken);
}