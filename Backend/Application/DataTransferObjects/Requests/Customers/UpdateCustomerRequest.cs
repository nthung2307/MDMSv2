using System.ComponentModel.DataAnnotations;
using Application.Commands.Customers;

namespace Application.DataTransferObjects.Requests.Customers;

public class UpdateCustomerRequest
{
    public long Id { get; set; }
    public string CustomerCode { get; set; }
    public string FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; }
    public string Status { get; set; }
    public ICollection<long> UserIds { get; set; } = new List<long>();
    public List<MeasurementPointCustomer> MeasurementPointCustomers { get; set; } = new List<MeasurementPointCustomer>();

}