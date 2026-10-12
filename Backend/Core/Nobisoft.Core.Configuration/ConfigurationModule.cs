using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Nobisoft.Core.Configuration;

public class ConfigurationModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var configurationBuilder = new ConfigurationBuilder();
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        // Always add the base settings file
        configurationBuilder.AddJsonFile("appsettings.json", optional: false);
        
        // Only add the environment-specific settings file if an environment is specified
        if (!string.IsNullOrEmpty(environmentName))
        {
            configurationBuilder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
        }
        
        configurationBuilder.AddEnvironmentVariables();
        var configuration = configurationBuilder.Build();
        
        // builder.Register(c => Options.Create(configuration.GetSection("AppDb").Get<AppDbConfiguration>())).SingleInstance();
        // builder.Register(c => Options.Create(configuration.GetSection("AwsCognito").Get<AwsCognitoConfiguration>())).SingleInstance();
        // builder.Register(c => Options.Create(configuration.GetSection("Jwt").Get<JwtConfiguration>())).SingleInstance();
        // builder.Register(c => Options.Create(configuration.GetSection("Jira").Get<JiraConfiguration>())).SingleInstance();
        // builder.Register(c => Options.Create(configuration.GetSection("S3").Get<S3Configuration>())).SingleInstance();
        builder.Register(c => Options.Create(configuration.GetSection("File").Get<FileConfiguration>())).SingleInstance();
        // builder.Register(c => Options.Create(configuration.GetSection("Application").Get<ApplicationConfiguration>())).SingleInstance();
    }
}