using System.ComponentModel.DataAnnotations;

namespace Application.DataTransferObjects.Requests.Customers;

public class CreateCustomerRequest
{
    public string CustomerCode { get; set; }
    public string FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; }
    public ICollection<long> UserIds { get; set; } = new List<long>();
    public string Status { get; set; }
}