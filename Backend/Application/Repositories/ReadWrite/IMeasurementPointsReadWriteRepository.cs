using Application.DataTransferObjects.Requests.Customers;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointsReadWriteRepository : IRepository<MeasurementPointsEntity>
{
    Task<MeasurementPointsEntity?> GetMeasurementPointByIdAsync(long id, CancellationToken cancellationToken);
    Task<MeasurementPointsEntity?> GetMeasurementPointForUpdateByIdAsync(long id, CancellationToken cancellationToken);
    Task<List<MeasurementPointsEntity>> GetMeasurementPointByIdsAsync(List<long> ids, long customerId, CancellationToken cancellationToken);
    Task<byte[]> GenerateCustomerPdf(ExportPdfAndExcelRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateCustomerCsv(CancellationToken cancellationToken);

}