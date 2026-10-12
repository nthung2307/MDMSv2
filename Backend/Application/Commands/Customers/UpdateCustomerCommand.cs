using Application.Common.Responses.Common.Responses;
using Application.DataTransferObjects.Responses.Customers;
using MediatR;
using Nobisoft.Core.Domain.Commands;

namespace Application.Commands.Customers;

public class UpdateCustomerCommand : CommandBase, IRequest<RequestResult<UpdateCustomerResponse>>
{
    public long Id { get; set; }
    public string CustomerCode { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string Status { get; set; }
    public ICollection<long> UserIds { get; set; } = new List<long>();
    public List<MeasurementPointCustomer> MeasurementPointCustomers { get; set; }
}

public class MeasurementPointCustomer
{
    public long MeasurementPointId { get; set; }
    public string? Email { get; set; }
    public string? Email2 { get; set; }
    public string? Email3 { get; set; }
}