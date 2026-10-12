using Application.Commands.TagName;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.TagName;
using MediatR;

namespace Application.Handlers.Commands.TagName;

public interface ICreateTagNameCommandHandler : IRequestHandler<CreateTagNameCommands, RequestResult<TagNameResponse>>
{
}

public interface IUpdateTagNameCommandHandler : IRequestHandler<UpdateTagNameCommand, RequestResult<TagNameResponse>>
{
}

public interface IDeleteTagNameCommandHandler : IRequestHandler<DeleteTagNameCommand, RequestResult<TagNameResponse>>
{
}
