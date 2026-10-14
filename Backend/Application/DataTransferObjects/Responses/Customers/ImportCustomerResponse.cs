namespace Application.DataTransferObjects.Responses.Customers;

public class ImportCustomerResponse
{
    public int TotalRows { get; set; }
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<ImportRowError> Errors { get; set; } = new();
}

public class ImportRowError
{
    public int RowNumber { get; set; }
    public string? CustomerCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
