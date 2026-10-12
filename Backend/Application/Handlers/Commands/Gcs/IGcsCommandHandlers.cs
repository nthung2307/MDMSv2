using Application.Commands.Gcs;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Gcs;
using MediatR;

namespace Application.Handlers.Commands.Gcs;

public interface ICreateGcsCommandHandler : IRequestHandler<CreateGcsCommand, RequestResult<GcsResponse>>
{
}

public interface IUpdateGcsCommandHandler : IRequestHandler<UpdateGcsCommand, RequestResult<GcsResponse>>
{
}

public interface IDeleteGcsCommandHandler : IRequestHandler<DeleteGcsCommand, RequestResult<GcsResponse>>
{
}

public interface IChangeStatusGcsCommandHandler : IRequestHandler<ChangeStatusGcsCommand, RequestResult<GcsResponse>>
{
}

public interface IFinalMeterReadingCommandHandler : IRequestHandler<FinalMeterReadingCommand, RequestResult<GcsResponse>>
{
}

public interface IUpdateGcsTemporaryCommandHandler : IRequestHandler<UpdateGcsTemporaryCommand, RequestResult<GcsResponse>>
{
}
