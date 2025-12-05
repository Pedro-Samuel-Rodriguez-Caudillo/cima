using System;
using System.Linq;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace cima.Blazor.Infrastructure.RateLimiting;

/// <summary>
/// Configuración de Rate Limiting para proteger la API contra abusos
/// </summary>
public static class RateLimitingConfiguration
{
    public const string FixedPolicy = "fixed";
    public const string SlidingPolicy = "sliding";
    public const string ApiPolicy = "api";
    public const string AuthPolicy = "auth";
    public const string ContactFormPolicy = "contact";
    public const string SearchPolicy = "search";

    /// <summary>
    /// Configura las políticas de Rate Limiting
    /// </summary>
    public static IServiceCollection AddCimaRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rateLimitOptions = configuration
            .GetSection("RateLimiting")
            .Get<RateLimitOptions>() ?? new RateLimitOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
            options.OnRejected = async (context, cancellationToken) =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("RateLimiting");
                
                var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var path = context.HttpContext.Request.Path;
                
                logger.LogWarning(
                    "Rate limit exceeded for IP {IpAddress} on path {Path}",
                    ipAddress, path);
                
                context.HttpContext.Response.ContentType = "application/json";
                
                var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? (int)retryAfter.TotalSeconds
                    : 60;
                
                context.HttpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
                
                var response = new
                {
                    error = "TooManyRequests",
                    message = "Has excedido el límite de peticiones. Por favor, espera antes de intentar de nuevo.",
                    retryAfterSeconds
                };
                
                await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            };

            // Fixed Window - General API (100 req/min)
            options.AddFixedWindowLimiter(FixedPolicy, opt =>
            {
                opt.PermitLimit = rateLimitOptions.FixedWindow.PermitLimit;
                opt.Window = TimeSpan.FromSeconds(rateLimitOptions.FixedWindow.WindowSeconds);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = rateLimitOptions.FixedWindow.QueueLimit;
            });

            // Sliding Window - Endpoints sensibles (50 req/30s)
            options.AddSlidingWindowLimiter(SlidingPolicy, opt =>
            {
                opt.PermitLimit = rateLimitOptions.SlidingWindow.PermitLimit;
                opt.Window = TimeSpan.FromSeconds(rateLimitOptions.SlidingWindow.WindowSeconds);
                opt.SegmentsPerWindow = rateLimitOptions.SlidingWindow.SegmentsPerWindow;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = rateLimitOptions.SlidingWindow.QueueLimit;
            });

            // API Policy - Basada en IP (200 req/min)
            options.AddPolicy(ApiPolicy, httpContext =>
            {
                var ipAddress = GetClientIpAddress(httpContext);
                
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.Api.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitOptions.Api.WindowSeconds),
                        SegmentsPerWindow = rateLimitOptions.Api.SegmentsPerWindow,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateLimitOptions.Api.QueueLimit
                    });
            });

            // Auth Policy - Muy restrictiva (10 req/min - previene brute force)
            options.AddPolicy(AuthPolicy, httpContext =>
            {
                var ipAddress = GetClientIpAddress(httpContext);
                
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.Auth.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitOptions.Auth.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0 // No queue para auth
                    });
            });

            // Contact Form Policy - Previene spam (5 req/5min por IP)
            options.AddPolicy(ContactFormPolicy, httpContext =>
            {
                var ipAddress = GetClientIpAddress(httpContext);
                
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"contact_{ipAddress}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            // Search Policy - Limita búsquedas costosas (30 req/min)
            options.AddPolicy(SearchPolicy, httpContext =>
            {
                var ipAddress = GetClientIpAddress(httpContext);
                
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: $"search_{ipAddress}",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 3,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 2
                    });
            });

            // Global Limiter - Último recurso (1000 req/5min por IP)
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                // Excluir assets estáticos del rate limiting global
                var path = httpContext.Request.Path.Value ?? "";
                if (path.StartsWith("/_content") || 
                    path.StartsWith("/_framework") ||
                    path.StartsWith("/css") ||
                    path.StartsWith("/js") ||
                    path.StartsWith("/images") ||
                    path.EndsWith(".wasm") ||
                    path.EndsWith(".dll"))
                {
                    return RateLimitPartition.GetNoLimiter("static");
                }
                
                var ipAddress = GetClientIpAddress(httpContext);
                
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.Global.PermitLimit,
                        Window = TimeSpan.FromMinutes(rateLimitOptions.Global.WindowMinutes),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateLimitOptions.Global.QueueLimit
                    });
            });
        });

        return services;
    }

    /// <summary>
    /// Obtiene la IP del cliente considerando proxies/load balancers
    /// </summary>
    private static string GetClientIpAddress(HttpContext httpContext)
    {
        // Intentar obtener IP real detrás de proxy
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For puede tener múltiples IPs, la primera es el cliente
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Aplica Rate Limiting al pipeline de la aplicación
    /// </summary>
    public static IApplicationBuilder UseCimaRateLimiting(this IApplicationBuilder app)
    {
        return app.UseRateLimiter();
    }
}

public class RateLimitOptions
{
    public FixedWindowOptions FixedWindow { get; set; } = new();
    public SlidingWindowOptions SlidingWindow { get; set; } = new();
    public ApiRateLimitOptions Api { get; set; } = new();
    public AuthRateLimitOptions Auth { get; set; } = new();
    public GlobalRateLimitOptions Global { get; set; } = new();
}

public class FixedWindowOptions
{
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 10;
}

public class SlidingWindowOptions
{
    public int PermitLimit { get; set; } = 50;
    public int WindowSeconds { get; set; } = 30;
    public int SegmentsPerWindow { get; set; } = 3;
    public int QueueLimit { get; set; } = 5;
}

public class ApiRateLimitOptions
{
    public int PermitLimit { get; set; } = 200;
    public int WindowSeconds { get; set; } = 60;
    public int SegmentsPerWindow { get; set; } = 4;
    public int QueueLimit { get; set; } = 10;
}

public class AuthRateLimitOptions
{
    public int PermitLimit { get; set; } = 10;
    public int WindowSeconds { get; set; } = 60;
}

public class GlobalRateLimitOptions
{
    public int PermitLimit { get; set; } = 1000;
    public int WindowMinutes { get; set; } = 5;
    public int QueueLimit { get; set; } = 50;
}
