using Application.Common.ValueObjects.CursorBased;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Requests.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IMeasurementPointMonitoringReadOnlyRepository : IRepository<MeasurementPointMonitoringEntity>
{
    Task<CursorPaginationResponse<ListMeasurementPointMonitoringDto>> GetListMeasurementPointsMonitoringAsync(GetListMeasurementPointMonitoringRequest request, CancellationToken cancellationToken);
    Task<byte[]> ExportPdfMeasurementPointsMonitoringAsync(CancellationToken cancellationToken);
    Task<byte[]> ExportCsvMeasurementPointsMonitoringAsync(CancellationToken cancellationToken);
    Task<MeasurementPointMonitoringDto?> GetMeasurementPointDetailAsync(string tagName, CancellationToken cancellationToken);
    Task<MeasurementPointMonitoringByIoTDto?> GetMeasurementPointDetailByIoTAsync(string tagName, CancellationToken cancellationToken);
    Task<List<MeasurementPointMonitoringHourlyDto>> GetListMeasurementPointHourlyAsync(CancellationToken cancellationToken);
    Task<List<MeasurementPointMonitoring2HoursAgoDto>> GetListMeasurementPoint2HoursAgoAsync(CancellationToken cancellationToken);
    Task<List<MeasurementPointEnergyReportDto>> GetListMeasurementEnergyReportAsync(CancellationToken cancellationToken);
    Task<List<MeasurementPointEnergyReportDto>> GetListMeasurementEnergyReportWithLookbackAsync(int lookbackMinutes, CancellationToken cancellationToken);
    Task<List<MeasurementPointOperatingReportDto>> GetListMeasurementOperatingReportWithLookbackAsync(int lookbackMinutes, CancellationToken cancellationToken);
}