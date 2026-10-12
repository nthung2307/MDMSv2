using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Requests.Customers;
using Application.DataTransferObjects.Responses.Customers;
using Application.DataTransferObjects.Responses.Users;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Customers;

public class CreateCustomerCommands : CommandBase, IRequest<RequestResult<CreateCustomerResponse>>
{
    public string CustomerCode { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public ICollection<long> UserIds { get; set; } = new List<long>();
    public string Status { get; set; }
}