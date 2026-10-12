namespace Nobisoft.Core.Configuration;

public class CrawlDataConfiguration
{
    public string SaleForceEndpoint { get; set; } = string.Empty;
    public string DMSEndpoint { get; set; } = string.Empty;
    public string BasicAuthentication { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string MESEndpoint { get; set; } = string.Empty;
    public string MulesoftUrl { get; set; } = string.Empty;
    public string FunctionNameMES { get; set; } = string.Empty;
    public string FunctionNameSF { get; set; } = string.Empty;
    public string FunctionNameDMS { get; set; } = string.Empty;
    public string MIS_DMS { get; set; } = string.Empty;
    public string MIS_MES { get; set; } = string.Empty;
    public string DMSTypeParam { get; set; } = string.Empty;
    public string MESTypeParam { get; set; } = string.Empty;
    public string SFTypeParam { get; set; } = string.Empty;
    public string DMSScheduler { get; set; } = string.Empty;
    public string MESScheduler { get; set; } = string.Empty;
    public string SFScheduler { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string DmsDownloadFile { get; set; } = string.Empty;
}