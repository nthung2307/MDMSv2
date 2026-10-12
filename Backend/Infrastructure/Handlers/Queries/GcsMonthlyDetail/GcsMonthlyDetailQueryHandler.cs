using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Responses.Gcs;
using Application.DataTransferObjects.Responses.GcsCutOffCycle;
using Application.DataTransferObjects.Responses.GcsMonthlyDetail;
using Application.Queries.Gcs;
using Application.Queries.GcsCutOffCycle;
using Application.Queries.GcsMonthlyDetail;
using Application.Repositories.ReadOnly;
using MediatR;

namespace Infrastructure.Handlers.Queries.GcsCutOffCycle;

public class GcsMonthlyDetailQueryHandler :
    IRequestHandler<GetListGcsMonthlyDetailQuery, CursorPaginationResponse<GcsMonthlyDetailDto>>
{
    private readonly IGcsMonthlyDetailReadOnlyRepository _gcsMonthlyDetailReadOnlyRepository;

    public GcsMonthlyDetailQueryHandler(IGcsMonthlyDetailReadOnlyRepository gcsMonthlyDetailReadOnlyRepository)
    {
        _gcsMonthlyDetailReadOnlyRepository = gcsMonthlyDetailReadOnlyRepository;
    }
    public async Task<CursorPaginationResponse<GcsMonthlyDetailDto>> Handle(GetListGcsMonthlyDetailQuery request, CancellationToken cancellationToken)
    {
        return await _gcsMonthlyDetailReadOnlyRepository.GetListGcsMonthlyDetailWithPaginationAsync(request.Request, cancellationToken);
    }

}