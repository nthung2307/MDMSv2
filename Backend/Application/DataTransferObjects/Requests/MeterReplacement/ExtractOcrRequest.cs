namespace Application.DataTransferObjects.Requests.MeterReplacement;

public class ExtractOcrRequest
{
    public string? ImageBase64 { get; set; }
    public string? DocumentUrl { get; set; }
    public string? ApiKey { get; set; }
}

