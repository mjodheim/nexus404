using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nexus404.Middleware.Interfaces;
using Nexus404.Middleware.Services;

namespace Nexus404.Middleware;

public static class Nexus404Extensions
{
    public static IServiceCollection AddNexus404(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient<IAiAnalysisService, PythonInteropService>();

        return services;
    }

    public static IApplicationBuilder UseNexus404(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.UseMiddleware<Nexus404Middleware>();
    }
}
