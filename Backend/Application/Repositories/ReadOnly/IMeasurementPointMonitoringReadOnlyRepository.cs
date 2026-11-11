using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IMeasurementPointMonitoringReadOnlyRepository : IRepository<MeasurementPointCurrentSnapshotEntity>
{
    Task<CursorPaginationResponse<ListMeasurementPointMonitoringDto>> GetListMeasurementPointsMonitoringAsync(GetListMeasurementPointMonitoringRequest request, CancellationToken cancellationToken);
    Task<byte[]> ExportPdfMeasurementPointsMonitoringAsync(CancellationToken cancellationToken);
    Task<byte[]> ExportCsvMeasurementPointsMonitoringAsync(CancellationToken cancellationToken);
    Task<MeasurementPointMonitoringDto?> GetMeasurementPointDetailAsync(string tagName, CancellationToken cancellationToken);
    Task<MeasurementPointMonitoringByIoTDto?> GetMeasurementPointDetailByIoTAsync(string tagName, CancellationToken cancellationToken);
    Task<List<MeasurementPointEnergyReportDto>> GetListMeasurementEnergyReportAsync(CancellationToken cancellationToken);
    Task<List<MeasurementPointEnergyReportDto>> GetListMeasurementEnergyReportWithLookbackAsync(int lookbackMinutes, CancellationToken cancellationToken, DateTimeOffset? targetTime = null);
    Task<List<MeasurementPointOperatingReportDto>> GetListMeasurementOperatingReportWithLookbackAsync(int lookbackMinutes, CancellationToken cancellationToken, DateTimeOffset? targetTime = null);
}
