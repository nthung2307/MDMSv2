using Application.Common.ValueObjects.Sorting;

namespace Application.DataTransferObjects.Requests.Customers;

public class ExportCustomerCsvRequest
{
    public string? Code { get; set; }
    public string? CustomerCode { get; set; }
    public string? Status { get; set; }
    public ICollection<SearchModel>? SearchByFields { get; set; }
}
