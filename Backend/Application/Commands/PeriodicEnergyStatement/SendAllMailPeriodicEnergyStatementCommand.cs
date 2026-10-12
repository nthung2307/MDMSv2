using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.PeriodicEnergyStatement;

public class SendAllMailPeriodicEnergyStatementCommand : CommandBase, IRequest<RequestResult<PeriodicEnergyStatementResponse>>
{
    public SendAllMailRequest Request { get; set; }
}