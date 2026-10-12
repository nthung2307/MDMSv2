using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.GcsCutOffCycle;
using Application.Queries.Gcs;
using Application.Queries.GcsCutOffCycle;
using Application.Repositories.ReadOnly;
using MediatR;

namespace Infrastructure.Handlers.Queries.GcsCutOffCycle;

public class GcsCutOffCycleQueryHandler :
    IRequestHandler<GetListGcsCutOffCycleQuery, CursorPaginationResponse<GcsCutOffCycleResponse>>,
    IRequestHandler<GetListYearGcsCutOffCycleQuery, List<int>>
{
    private readonly IGcsCutOffCycleReadOnlyRepository _gcsCutOffCycleReadOnlyRepository;

    public GcsCutOffCycleQueryHandler(IGcsCutOffCycleReadOnlyRepository gcsCutOffCycleReadOnlyRepository)
    {
        _gcsCutOffCycleReadOnlyRepository = gcsCutOffCycleReadOnlyRepository;
    }
    public async Task<CursorPaginationResponse<GcsCutOffCycleResponse>> Handle(GetListGcsCutOffCycleQuery request, CancellationToken cancellationToken)
    {
        return await _gcsCutOffCycleReadOnlyRepository.GetListGcsCutOffCyclesAsync(request.Request, cancellationToken);
    }

    public async Task<List<int>> Handle(GetListYearGcsCutOffCycleQuery request, CancellationToken cancellationToken)
    {
        return await _gcsCutOffCycleReadOnlyRepository.GetListYearGcsCutOffCyclesAsync(request.GcsId, cancellationToken);
    }
}