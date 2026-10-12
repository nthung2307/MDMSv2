namespace Nobisoft.Core.Configuration;

public class S3Configuration
{
    public string BucketName { get; set; } = string.Empty;
    public string BucketNameExportExcel { get; set; } = string.Empty;
    public string ExportExcelFunctionName { get; set; } = string.Empty;
    public string CrawlDataMesFunctionName { get; set; } = string.Empty;
    public string ImportPredefineUsersFunctionName { get; set; } = string.Empty;
    public string CustomDomain { get; set; } = string.Empty;
}