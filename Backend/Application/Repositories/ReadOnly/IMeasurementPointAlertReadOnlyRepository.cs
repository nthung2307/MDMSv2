using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IMeasurementPointAlertReadOnlyRepository : IRepository<MeasurementPointsAlertEntity>
{
    Task<CursorPaginationResponse<MeasurementPointAlertDto>> GetListMeasurementPointsAlertAsync(GetListMeasurementPointAlertRequest request, CancellationToken cancellationToken);
    Task<byte[]> ExportPdfMeasurementPointsAlertAsync(CancellationToken cancellationToken);
    Task<byte[]> ExportCsvMeasurementPointsAlertAsync(CancellationToken cancellationToken);
    Task<CursorPaginationResponse<MeasurementPointAlertDto>> GetListMeasurementPointsAlertForNotificationAsync(GetListMeasurementPointAlertRequest request, CancellationToken cancellationToken);
    Task<int> CountMeasurementPointAlert(CancellationToken cancellationToken);
}