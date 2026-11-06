namespace Application.DataTransferObjects.Responses.Gcs;

public class ImportGcsTemporaryExcelResponse
{
    public int TotalRows { get; set; }
    public int UpdatedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
