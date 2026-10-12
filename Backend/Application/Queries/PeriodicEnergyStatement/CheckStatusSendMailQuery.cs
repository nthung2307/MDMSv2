using Application.Common.ValueObjects.CursorBased;
using Application.DataTransferObjects.Requests.PeriodicEnergyStatement;
using Application.DataTransferObjects.Responses.PeriodicEnergyStatement;
using MediatR;
using Nobisoft.Core.Domain.Queries;

namespace Application.Queries.PeriodicEnergyStatement;

public class CheckStatusSendMailQuery : QueryBase, IRequest<bool>
{
    public SendAllMailRequest Request { get; set; }
}