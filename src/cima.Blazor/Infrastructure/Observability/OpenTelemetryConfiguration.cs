using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace cima.Blazor.Infrastructure.Observability;

/// <summary>
/// Configuracion de OpenTelemetry para distributed tracing y metricas
/// </summary>
public static class OpenTelemetryConfiguration
{
    public const string ServiceName = "cima";
    public const string ServiceVersion = "1.0.0";

    public static IServiceCollection AddCimaOpenTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var options = configuration
            .GetSection("OpenTelemetry")
            .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

        if (!options.Enabled)
        {
            return services;
        }

        // Registrar servicios de metricas de negocio
        services.AddSingleton<CimaMetrics>();

        // Configurar OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: ServiceName,
                    serviceVersion: ServiceVersion,
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        // Filtrar health checks y assets estaticos
                        opts.Filter = httpContext =>
                        {
                            var path = httpContext.Request.Path.Value ?? "";
                            return !path.StartsWith("/health") &&
                                   !path.StartsWith("/_framework") &&
                                   !path.StartsWith("/_blazor") &&
                                   !path.EndsWith(".js") &&
                                   !path.EndsWith(".css") &&
                                   !path.EndsWith(".wasm");
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddSource(CimaActivitySource.Name);

                // Exportar a consola en desarrollo
                if (environment.IsDevelopment())
                {
                    tracing.AddConsoleExporter();
                }

                // Exportar a OTLP si esta configurado
                if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                {
                    tracing.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(options.OtlpEndpoint);
                    });
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddMeter(CimaMetrics.MeterName);

                // Exportar a consola en desarrollo
                if (environment.IsDevelopment())
                {
                    metrics.AddConsoleExporter();
                }

                // Exportar a OTLP si esta configurado
                if (!string.IsNullOrEmpty(options.OtlpEndpoint))
                {
                    metrics.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(options.OtlpEndpoint);
                    });
                }
            });

        return services;
    }

    public static IApplicationBuilder UseCimaOpenTelemetry(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        // Middleware adicional si es necesario
        // Por ahora, OpenTelemetry se auto-configura via DI
        return app;
    }
}

public class OpenTelemetryOptions
{
    public bool Enabled { get; set; } = false;
    public string? OtlpEndpoint { get; set; }
}

/// <summary>
/// ActivitySource para tracing distribuido de operaciones de CIMA
/// </summary>
public static class CimaActivitySource
{
    public const string Name = "cima.Blazor";
    
    public static readonly ActivitySource Instance = new(Name, OpenTelemetryConfiguration.ServiceVersion);

    public static Activity? StartListingActivity(string operation, Guid? listingId = null)
    {
        var activity = Instance.StartActivity($"Listing.{operation}");
        if (activity != null && listingId.HasValue)
        {
            activity.SetTag("listing.id", listingId.Value.ToString());
        }
        return activity;
    }

    public static Activity? StartContactRequestActivity(string operation, Guid? requestId = null)
    {
        var activity = Instance.StartActivity($"ContactRequest.{operation}");
        if (activity != null && requestId.HasValue)
        {
            activity.SetTag("contact_request.id", requestId.Value.ToString());
        }
        return activity;
    }
}

/// <summary>
/// Metricas de negocio personalizadas para CIMA
/// </summary>
public class CimaMetrics
{
    public const string MeterName = "cima.Business";

    private readonly Counter<long> _listingsCreated;
    private readonly Counter<long> _contactRequestsReceived;
    private readonly Counter<long> _propertyViews;
    private int _activeListingsCount;
    private int _activeArchitectsCount;

    public CimaMetrics()
    {
        var meter = new Meter(MeterName, OpenTelemetryConfiguration.ServiceVersion);

        _listingsCreated = meter.CreateCounter<long>(
            "cima.listings.created",
            description: "Total de propiedades creadas");

        _contactRequestsReceived = meter.CreateCounter<long>(
            "cima.contact_requests.received",
            description: "Total de solicitudes de contacto recibidas");

        _propertyViews = meter.CreateCounter<long>(
            "cima.property.views",
            description: "Total de visualizaciones de propiedades");

        meter.CreateObservableGauge(
            "cima.listings.active",
            () => _activeListingsCount,
            description: "Numero de propiedades activas (publicadas)");

        meter.CreateObservableGauge(
            "cima.architects.active",
            () => _activeArchitectsCount,
            description: "Numero de arquitectos activos");
    }

    public void IncrementListingsCreated() => _listingsCreated.Add(1);
    
    public void IncrementContactRequests() => _contactRequestsReceived.Add(1);
    
    public void IncrementPropertyViews(Guid listingId)
    {
        _propertyViews.Add(1, new KeyValuePair<string, object?>("listing.id", listingId.ToString()));
    }

    public void SetActiveListingsCount(int count) => _activeListingsCount = count;
    
    public void SetActiveArchitectsCount(int count) => _activeArchitectsCount = count;
}
