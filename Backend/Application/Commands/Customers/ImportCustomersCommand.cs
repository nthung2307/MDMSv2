using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Customers;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Customers;

public class ImportCustomersCommand : CommandBase, IRequest<RequestResult<ImportCustomerResponse>>
{
    public Stream FileStream { get; set; } = null!;
}
