using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PdfMerge.Application;
using PdfMerge.Functions.Middleware;
using PdfMerge.Infrastructure;
using Syncfusion.Licensing;

namespace PdfMerge.Functions;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureFunctionsWebApplication(worker =>
            {
                worker.UseMiddleware<ExceptionHandlingMiddleware>();
            })
            .ConfigureServices((context, services) =>
            {
                services
                    .AddApplicationServices()
                    .AddInfrastructureServices(context.Configuration);

                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights();

                SyncfusionLicenseProvider.RegisterLicense(context.Configuration["SyncfusionLicense"]);
            });
        
}
