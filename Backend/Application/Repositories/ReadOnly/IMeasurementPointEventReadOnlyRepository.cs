using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.MeasurementPoints;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Domain.Entities;
using Nobisoft.Core.Database.Repository;

namespace Application.Repositories.ReadOnly;

public interface IMeasurementPointEventReadOnlyRepository : IRepository<MeasurementPointsEventEntity>
{
    Task<CursorPaginationResponse<MeasurementPointEventDto>> GetListMeasurementPointsEventAsync(GetListMeasurementPointEventRequest request, CancellationToken cancellationToken);
    Task<byte[]> ExportPdfMeasurementPointsEventAsync(CancellationToken cancellationToken);
    Task<byte[]> ExportCsvMeasurementPointsEventAsync(CancellationToken cancellationToken);
}