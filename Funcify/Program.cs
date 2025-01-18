using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Funcify.Services;
using Extensions;
using Microsoft.Extensions.Configuration;
using Funcify.Actions;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddAzureServices(
            context.Configuration
        );
        
        services.AddSingleton<CreateProduct>();
        services.AddSingleton<UploadImage>();
        services.AddSingleton<UpdateProduct>();
    })
    .Build();



host.Run();
