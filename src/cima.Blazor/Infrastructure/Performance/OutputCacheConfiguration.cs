using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace cima.Blazor.Infrastructure.Performance;

/// <summary>
/// Configuración de Output Caching para endpoints de API
/// </summary>
public static class OutputCacheConfiguration
{
    public static IServiceCollection AddCimaOutputCache(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            // Política por defecto: no cachear
            options.DefaultExpirationTimeSpan = TimeSpan.Zero;

            // Política para listings públicos (5 minutos)
            options.AddPolicy("PublicListings", builder =>
            {
                builder.Expire(TimeSpan.FromMinutes(5));
                builder.SetVaryByQuery("*");
                builder.Tag("listings");
            });

            // Política para featured/destacados (10 minutos)
            options.AddPolicy("FeaturedListings", builder =>
            {
                builder.Expire(TimeSpan.FromMinutes(10));
                builder.Tag("featured");
            });

            // Política para estadísticas (15 minutos)
            options.AddPolicy("Statistics", builder =>
            {
                builder.Expire(TimeSpan.FromMinutes(15));
                builder.Tag("statistics");
            });

            // Política para assets estáticos (1 hora)
            options.AddPolicy("StaticAssets", builder =>
            {
                builder.Expire(TimeSpan.FromHours(1));
                builder.SetVaryByHeader(HeaderNames.AcceptEncoding);
            });

            // Política para SEO/sitemap (30 minutos)
            options.AddPolicy("Sitemap", builder =>
            {
                builder.Expire(TimeSpan.FromMinutes(30));
                builder.Tag("sitemap");
            });
        });

        return services;
    }

    public static IApplicationBuilder UseCimaOutputCache(this IApplicationBuilder app)
    {
        return app.UseOutputCache();
    }
}

/// <summary>
/// Extension methods para invalidación de cache
/// </summary>
public static class CacheInvalidationExtensions
{
    public static async Task InvalidateListingsCache(this IOutputCacheStore store, CancellationToken ct = default)
    {
        await store.EvictByTagAsync("listings", ct);
        await store.EvictByTagAsync("featured", ct);
    }

    public static async Task InvalidateStatisticsCache(this IOutputCacheStore store, CancellationToken ct = default)
    {
        await store.EvictByTagAsync("statistics", ct);
    }

    public static async Task InvalidateAllCache(this IOutputCacheStore store, CancellationToken ct = default)
    {
        await store.EvictByTagAsync("listings", ct);
        await store.EvictByTagAsync("featured", ct);
        await store.EvictByTagAsync("statistics", ct);
        await store.EvictByTagAsync("sitemap", ct);
    }
}
