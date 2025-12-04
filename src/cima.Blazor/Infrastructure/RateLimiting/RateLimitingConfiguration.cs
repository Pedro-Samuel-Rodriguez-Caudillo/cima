using System;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                context.HttpContext.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "TooManyRequests",
                    message = "Has excedido el límite de peticiones. Por favor, espera antes de intentar de nuevo.",
                    retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? retryAfter.TotalSeconds
                        : 60
                };
                
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry))
                {
                    context.HttpContext.Response.Headers.RetryAfter = retry.TotalSeconds.ToString();
                }
                
                await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            };

            // Fixed Window - General API
            options.AddFixedWindowLimiter(FixedPolicy, opt =>
            {
                opt.PermitLimit = rateLimitOptions.FixedWindow.PermitLimit;
                opt.Window = TimeSpan.FromSeconds(rateLimitOptions.FixedWindow.WindowSeconds);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = rateLimitOptions.FixedWindow.QueueLimit;
            });

            // Sliding Window - Endpoints sensibles
            options.AddSlidingWindowLimiter(SlidingPolicy, opt =>
            {
                opt.PermitLimit = rateLimitOptions.SlidingWindow.PermitLimit;
                opt.Window = TimeSpan.FromSeconds(rateLimitOptions.SlidingWindow.WindowSeconds);
                opt.SegmentsPerWindow = rateLimitOptions.SlidingWindow.SegmentsPerWindow;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = rateLimitOptions.SlidingWindow.QueueLimit;
            });

            // API Policy - Basada en IP
            options.AddPolicy(ApiPolicy, httpContext =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
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

            // Auth Policy - Más restrictiva
            options.AddPolicy(AuthPolicy, httpContext =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ipAddress,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.Auth.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitOptions.Auth.WindowSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
            });

            // Global Limiter
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                
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
