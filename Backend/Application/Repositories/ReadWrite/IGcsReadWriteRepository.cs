using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Requests.Gcs;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IGcsReadWriteRepository : IRepository<GcsEntity>
{
    Task<GcsEntity?> GetGcsByIdAsync(long id, CancellationToken cancellationToken);
    Task<bool> IsExistGcsCodeAsync(long? id, string code, CancellationToken cancellationToken);
    Task<byte[]> GenerateGcsPdf(ExportPdfAndExcelRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateGcsCsv(CancellationToken cancellationToken);
    Task<byte[]> GenerateGcsFinalPdf(ExportPdfAndExcelGcsFinalRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateGcsFinalCsv(ExportPdfAndExcelGcsFinalRequest request, CancellationToken cancellationToken);
}