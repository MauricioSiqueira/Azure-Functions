using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PdfMerge.Application.Interfaces;
using PdfMerge.Application.Services;

namespace PdfMerge.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPdfMergeService, PdfMergeService>();
        return services;
    }
}
