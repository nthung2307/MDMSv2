using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Customers;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Customers;

public class ChangeStatusCustomerCommand : CommandBase, IRequest<RequestResult<UpdateCustomerResponse>>
{
    public long Id { get; set; }
}