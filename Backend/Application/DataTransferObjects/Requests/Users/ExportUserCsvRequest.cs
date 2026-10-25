using Application.Common.ValueObjects.Sorting;

namespace Application.DataTransferObjects.Requests.Users;

public class ExportUserCsvRequest
{
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? RoleName { get; set; }
    public string? Status { get; set; }
    public ICollection<SearchModel>? SearchByFields { get; set; }
}
