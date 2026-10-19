using Application.DataTransferObjects.Requests.Report;
using Application.DataTransferObjects.Responses.Report;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadWrite;

public interface IMeasurementPointHalfHourEnergyReportReadWriteRepository : IRepository<MeasurementPointHalfHourEnergyReportEntity>
{
    Task<byte[]> GeneratePdf(HalfHourFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateCsv(HalfHourFinalizedEnergyReportRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateActivePointsEnergyCsvAsync(ActivePointsEnergyReportRequest request, CancellationToken cancellationToken);
    Task<byte[]> GenerateActivePointsEnergyExcelAsync(ActivePointsEnergyReportRequest request, CancellationToken cancellationToken);
}