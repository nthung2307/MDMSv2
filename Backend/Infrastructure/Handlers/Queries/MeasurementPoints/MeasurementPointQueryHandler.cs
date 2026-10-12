using Application.Common.ValueObjects.CursorBased;
using Application.Common.ValueObjects.OffsetBased;
using Application.DataTransferObjects.Responses.MeasurementPoints;
using Application.Queries.MeasurementPoints;
using Application.Repositories.ReadOnly;
using MediatR;

namespace Infrastructure.Handlers.Queries.MeasurementPoints;

public class MeasurementPointQueryHandler :
    IRequestHandler<GetListMeasurementPointsQuery, CursorPaginationResponse<ListMeasurementPointDto>>,
    IRequestHandler<GetListMeasurementPointsForReportQuery, CursorPaginationResponse<ListMeasurementPointDto>>,
    IRequestHandler<GetMeasurementPointDetailQuery, MeasurementPointDto>
{
    private readonly IMeasurementPointsReadOnlyRepository _measurementPointsReadOnlyRepository;

    public MeasurementPointQueryHandler(IMeasurementPointsReadOnlyRepository measurementPointsReadOnlyRepository)
    {
        _measurementPointsReadOnlyRepository = measurementPointsReadOnlyRepository;
    }
    public async Task<CursorPaginationResponse<ListMeasurementPointDto>> Handle(GetListMeasurementPointsQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointsReadOnlyRepository.GetListMeasurementPointsAsync(request.Request, cancellationToken);
    }

    public async Task<CursorPaginationResponse<ListMeasurementPointDto>> Handle(GetListMeasurementPointsForReportQuery request, CancellationToken cancellationToken)
    {
        return await _measurementPointsReadOnlyRepository.GetListMeasurementPointsForReportAsync(request.Request, cancellationToken);
    }

    public async Task<MeasurementPointDto> Handle(GetMeasurementPointDetailQuery query, CancellationToken cancellationToken)
    {
        return await _measurementPointsReadOnlyRepository.GetMeasurementPointDetailAsync(query.Id, cancellationToken);
    }
}