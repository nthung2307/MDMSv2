namespace Nobisoft.Core.Configuration;

public class AwsCognitoConfiguration
{
    public string Region { get; set; } = "us-east-1";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecretValue { get; set; } = string.Empty;
    public string ClientSecretKey { get; set; } = string.Empty;

    public string UserPoolId { get; set; } = string.Empty;
    
    public string RedirectUri { get; set; } = string.Empty;
    public string OAuthTokenUrl { get; set; } = string.Empty;
    
}