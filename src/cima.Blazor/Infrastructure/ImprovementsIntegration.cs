using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using cima.Blazor.Infrastructure.Caching;
using cima.Blazor.Infrastructure.Observability;
using cima.Blazor.Infrastructure.RateLimiting;

namespace cima.Blazor.Infrastructure;

/// <summary>
/// Clase de extensi�n para integrar todas las mejoras en el m�dulo Blazor
/// </summary>
public static class ImprovementsIntegration
{
    /// <summary>
    /// Configura todos los servicios de las mejoras implementadas
    /// Llamar en ConfigureServices del m�dulo Blazor
    /// </summary>
    public static IServiceCollection AddCimaImprovements(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // 2. Rate Limiting - Protección contra abusos
        services.AddCimaRateLimiting(configuration);

        // 3. Redis Cache - Caché distribuido
        services.AddCimaRedisCache(configuration);

        // 17 & 18. OpenTelemetry - Distributed tracing y m�tricas de negocio
        services.AddCimaOpenTelemetry(configuration, environment);

        return services;
    }

    /// <summary>
    /// Configura el middleware de las mejoras implementadas
    /// Llamar en OnApplicationInitialization del m�dulo Blazor (despu�s de UseRouting)
    /// </summary>
    public static IApplicationBuilder UseCimaImprovements(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        // Rate Limiting middleware
        app.UseRateLimiter();

        // OpenTelemetry Prometheus endpoint
        app.UseCimaOpenTelemetry(configuration);

        return app;
    }
}
