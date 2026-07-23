using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfMerge.Application.Interfaces;
using PdfMerge.Infrastructure.Configuration;
using PdfMerge.Infrastructure.Storage;

namespace PdfMerge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<BlobStorageOptions>()
            .Bind(configuration.GetSection(BlobStorageOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IBlobStorageService, BlobStorageService>();

        return services;
    }
}
