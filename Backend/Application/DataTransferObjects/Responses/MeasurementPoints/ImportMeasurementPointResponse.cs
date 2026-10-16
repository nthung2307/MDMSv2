namespace Application.DataTransferObjects.Responses.MeasurementPoints;

public class ImportMeasurementPointResponse
{
    public int TotalRows { get; set; }
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<ImportMeasurementPointRowError> Errors { get; set; } = new();
}

public class ImportMeasurementPointRowError
{
    public int RowNumber { get; set; }
    public string? PointCode { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
