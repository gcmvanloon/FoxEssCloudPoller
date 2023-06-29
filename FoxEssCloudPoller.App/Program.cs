using FoxEssCloudPoller;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;


CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

//run the DataPoller IHostedService implementation...
await CreateHostBuilder(args).RunConsoleAsync();


static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.AddSingleton<FoxEssCloudClient>();
        services.AddSingleton<PVOutputClient>();
        services.AddTransient<DataPoller>();
        services.AddTransient<IHandleNewInverterMeasurements, SendMeasurmentsToPVOutputHandler>();
        services.AddTransient<IHostedService, DataPoller>();

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
