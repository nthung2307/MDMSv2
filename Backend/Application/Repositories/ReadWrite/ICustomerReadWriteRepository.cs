using Application.DataTransferObjects.Requests.Customers;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface ICustomerReadWriteRepository : IRepository<CustomerEntity>
{
    Task<CustomerEntity?> GetCustomerByCustomerIdAsync(long id, CancellationToken cancellationToken);
    Task<byte[]> GenerateCustomerPdf(ExportPdfAndExcelRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateCustomerCsv(CancellationToken cancellationToken);
}