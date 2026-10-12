using Application.DataTransferObjects.Responses.MeasurementPoints;
using Domain.Entities;

namespace Application.DataTransferObjects.Responses.Customers;

public class CustomerDto
{
    public long Id { get; set; }
    public string CustomerCode { get; set; }
    public string FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Status { get; set; }
    public List<MeasurementPointDto> MeasurePoints { get; set; }
    public List<UserCustomerEntity> UserCustomer { get; set; } = new List<UserCustomerEntity>();
    public DateTimeOffset? CreatedTime { get; set; }
}