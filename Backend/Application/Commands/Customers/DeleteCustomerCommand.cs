using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.Users;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Customers;

public class DeleteCustomerCommand : CommandBase, IRequest<RequestResult<DeleteCustomerResponse>>
{
    public long Id { get; set; }
}