using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.PeriodicEnergyStatement;

public class SendMailPeriodicEnergyStatementCommand : CommandBase, IRequest<RequestResult<PeriodicEnergyStatementResponse>>
{
    public SendMailRequest Request { get; set; }
}