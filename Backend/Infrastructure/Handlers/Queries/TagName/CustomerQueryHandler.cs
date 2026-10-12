using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Responses.TagName;
using Application.Queries.TagName;
using Application.Repositories.ReadOnly;
using MediatR;

namespace Infrastructure.Handlers.Queries.TagName;

public class CustomerQueryHandler :
    IRequestHandler<GetListTagByMeasurementPointQuery, List<TagNameDto>>
{
    private readonly ITagNameReadOnlyRepository _tagNameReadOnlyRepository;

    public CustomerQueryHandler(ITagNameReadOnlyRepository tagNameReadOnlyRepository)
    {
        _tagNameReadOnlyRepository = tagNameReadOnlyRepository;
    }
    public async Task<List<TagNameDto>> Handle(GetListTagByMeasurementPointQuery query, CancellationToken cancellationToken)
    {
        return await _tagNameReadOnlyRepository.GetListTagNamesAsync(query.MeasurementPointId, query.Type, cancellationToken);
    }
}