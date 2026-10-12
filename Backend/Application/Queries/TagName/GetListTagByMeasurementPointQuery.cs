using Application.DataTransferObjects.Responses.TagName;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.TagName;

public class GetListTagByMeasurementPointQuery: QueryBase, IRequest<List<TagNameDto>>
{
    public long MeasurementPointId { get; set; }
    public string? Type { get; set; }
}