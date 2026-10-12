using Application.DataTransferObjects.Requests.Customers;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IUserReadWriteRepository : IRepository<UserEntity>
{
    Task<UserEntity?> GetUserByUserNameOrEmailAsync(string userName, CancellationToken cancellationToken);
    Task<UserEntity?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<UserEntity?> GetUserByUserIdAsync(long id, CancellationToken cancellationToken);
    Task<List<UserEntity>> GetUsersByUserIdsAsync(List<long> ids, CancellationToken cancellationToken);
    Task<byte[]> GenerateCustomerPdf(ExportPdfAndExcelRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateCustomerCsv(CancellationToken cancellationToken);
}