using FoxEssCloudPoller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;


CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

using var host = CreateHostBuilder(args).Build();
ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();
try
{
    logger.LogInformation("FoxEssCloudPoller starting...");
    logger.LogInformation($"Host EnvironmentName '{host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName}'");

    host.Services.GetRequiredService<DataPoller>().Run(5);

    logger.LogInformation("FoxEssCloudPoller stopped gracefully.");
}
catch (Exception ex)
{
    logger.LogError(ex, "An unhandled exception occurred.");
}



static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.AddSingleton<FoxEssCloudClient>();
        services.AddSingleton<PVOutputClient>();
        services.AddTransient<DataPoller>();
        services.AddTransient<IHandleNewInverterMeasurements, SendMeasurmentsToPVOutputHandler>();

    })
    .ConfigureAppConfiguration((context, configurationBuilder) =>
    {
        var env = context.HostingEnvironment;

        configurationBuilder
            .AddJsonFile($"appsettings.json", false, true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .AddCommandLine(args);
    })
    .ConfigureLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddConsole();
    });
