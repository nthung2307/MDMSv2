using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.TagName;
using Application.DataTransferObjects.Responses.TagName;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.TagName;

public class UpdateTagNameCommand : CommandBase, IRequest<RequestResult<TagNameResponse>>
{
    public List<UpdateTagNameRequest> TagNames { get; set; }
    public long MeasurementPointId { get; set; }
    public string Type { get; set; }
}