using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Responses.Gcs;
using Application.Queries.Gcs;
using Application.Repositories.ReadOnly;
using MediatR;

namespace Infrastructure.Handlers.Queries.Gcs;

public class GcsQueryHandler :
    IRequestHandler<GetListGcsQuery, CursorPaginationResponse<GcsDto>>,
    IRequestHandler<GetGcsQuery, GcsDto>
{
    private readonly IGcsReadOnlyRepository _gcsReadOnlyRepository;

    public GcsQueryHandler(IGcsReadOnlyRepository gcsReadOnlyRepository)
    {
        _gcsReadOnlyRepository = gcsReadOnlyRepository;
    }
    public async Task<CursorPaginationResponse<GcsDto>> Handle(GetListGcsQuery request, CancellationToken cancellationToken)
    {
        return await _gcsReadOnlyRepository.GetListGcsAsync(request.Request, cancellationToken);
    }

    public async Task<GcsDto> Handle(GetGcsQuery query, CancellationToken cancellationToken)
    {
        return await _gcsReadOnlyRepository.GetGcsAsync(query.Id, cancellationToken);
    }
}