namespace Nobisoft.Core.Configuration;

public class ElasticApmConfiguration
{
    public string ServiceName { get; set; } = "BinningAppElasticApm";
    public string SecretToken { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}