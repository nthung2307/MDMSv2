using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.TagName;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.TagName;

public class DeleteTagNameCommand : CommandBase, IRequest<RequestResult<TagNameResponse>>
{
    public long Id { get; set; }
}