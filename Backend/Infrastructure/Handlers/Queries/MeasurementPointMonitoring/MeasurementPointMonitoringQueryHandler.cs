using Application.Common.ValueObjects.CursorBased;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Responses.MeasurementPointMonitoring;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.Queries.MeasurementPointMonitoring;
using Application.Queries.MeasurementPoints;
using Application.Repositories.ReadOnly;
using Domain.Entities;
using MediatR;

namespace Infrastructure.Handlers.Queries.MeasurementPointMonitoring;

public class MeasurementPointMonitoringQueryHandler :
    IRequestHandler<GetListMeasurementPointMonitoringQuery, CursorPaginationResponse<ListMeasurementPointMonitoringDto>>,
    IRequestHandler<GetListMeasurementPointEventQuery, CursorPaginationResponse<MeasurementPointEventDto>>,
    IRequestHandler<GetListMeasurementPointAlertQuery, CursorPaginationResponse<MeasurementPointAlertDto>>,
    IRequestHandler<GetMeasurementPointMonitoringQuery, MeasurementPointMonitoringDto?>
{
    private readonly IMeasurementPointMonitoringReadOnlyRepository _measurementPointMonitoringReadOnlyRepository;
    private readonly IMeasurementPointAlertReadOnlyRepository _measurementPointAlertReadOnlyRepository;
    private readonly IMeasurementPointEventReadOnlyRepository _measurementPointEventReadOnlyRepository;

    public MeasurementPointMonitoringQueryHandler(IMeasurementPointMonitoringReadOnlyRepository measurementPointMonitoringReadOnlyRepository, IMeasurementPointAlertReadOnlyRepository measurementPointAlertReadOnlyRepository, IMeasurementPointEventReadOnlyRepository measurementPointEventReadOnlyRepository)
    {
        _measurementPointMonitoringReadOnlyRepository = measurementPointMonitoringReadOnlyRepository;
        _measurementPointAlertReadOnlyRepository = measurementPointAlertReadOnlyRepository;
        _measurementPointEventReadOnlyRepository = measurementPointEventReadOnlyRepository;
    }
    public async Task<CursorPaginationResponse<ListMeasurementPointMonitoringDto>> Handle(GetListMeasurementPointMonitoringQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointMonitoringReadOnlyRepository.GetListMeasurementPointsMonitoringAsync(request.Request, cancellationToken);
    }

    public async Task<MeasurementPointMonitoringDto?> Handle(GetMeasurementPointMonitoringQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointMonitoringReadOnlyRepository.GetMeasurementPointDetailAsync(request.TagName, cancellationToken);
    }

    public async Task<CursorPaginationResponse<MeasurementPointEventDto>> Handle(GetListMeasurementPointEventQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointEventReadOnlyRepository.GetListMeasurementPointsEventAsync(request.Request, cancellationToken);
    }

    public async Task<CursorPaginationResponse<MeasurementPointAlertDto>> Handle(GetListMeasurementPointAlertQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointAlertReadOnlyRepository.GetListMeasurementPointsAlertAsync(request.Request, cancellationToken);
    }
}