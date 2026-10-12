using Application.Commands.PeriodicEnergyStatement;
using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using MediatR;

namespace Application.Handlers.Commands.PeriodicEnergyStatement;

public interface ISendMailCommand : IRequestHandler<SendMailPeriodicEnergyStatementCommand, RequestResult<PeriodicEnergyStatementResponse>>
{
}

public interface ISendAllMailCommand : IRequestHandler<SendAllMailPeriodicEnergyStatementCommand, RequestResult<PeriodicEnergyStatementResponse>>
{
}