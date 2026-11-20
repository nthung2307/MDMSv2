using MDMS.HES.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseSystemd()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        var baseSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(baseSettingsPath))
        {
            config.AddJsonFile(baseSettingsPath, optional: true, reloadOnChange: true);
        }
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<HesWorker>();
    })
    .ConfigureLogging((hostContext, logging) =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.AddDebug();
    });

var host = builder.Build();
await host.RunAsync();
