namespace Nobisoft.Core.Configuration;

public class JiraConfiguration
{
    public string Url { get; set; } = "https://jira.nobisoft.com.vn";
    public string ApiPrefix { get; set; } = "rest/api";
    public string ApiVersion { get; set; } = "2";
    public string BasicAuthentication { get; set; } = "";
    public string ProjectField { get; set; } = "";
    public string CtcField { get; set; } = "";
    public string ConditionCodeField { get; set; } = "";
    public string SummaryField { get; set; } = "";
    public string DescriptionField { get; set; } = "";
    public string PartNameField { get; set; } = "";
    public string CreatedField { get; set; } = "";
    public string ReporterField { get; set; } = "";
    public string StatusField { get; set; } = "";
    public string IcaField { get; set; } = "";
    public string PcaField { get; set; } = "";
}