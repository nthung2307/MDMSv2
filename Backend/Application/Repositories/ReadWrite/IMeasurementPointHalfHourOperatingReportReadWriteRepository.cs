using Application.DataTransferObjects.Requests.Report;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointHalfHourOperatingReportReadWriteRepository : IRepository<MeasurementPointHalfHourOperatingReportEntity>
{
    Task<byte[]> GeneratePdf(OperatingParameterReportRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateCsv(OperatingParameterReportRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateExcel(OperatingParameterReportRequest request, CancellationToken cancellationToken);
}
