using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace cima.Blazor.Infrastructure.Observability;

/// <summary>
/// Configuración de OpenTelemetry para distributed tracing y métricas
/// Nota: Requiere paquetes OpenTelemetry.* para funcionalidad completa
/// Por ahora solo registra las métricas de negocio
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
        var otelOptions = configuration
            .GetSection("OpenTelemetry")
            .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

        if (!otelOptions.Enabled)
        {
            return services;
        }

        // Registrar métricas de negocio (siempre disponibles)
        services.AddSingleton<CimaMetrics>();

        // TODO: Agregar OpenTelemetry completo cuando se instalen los paquetes
        // services.AddOpenTelemetry()...

        return services;
    }

    public static IApplicationBuilder UseCimaOpenTelemetry(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var otelOptions = configuration
            .GetSection("OpenTelemetry")
            .Get<OpenTelemetryOptions>() ?? new OpenTelemetryOptions();

        // TODO: Habilitar Prometheus endpoint cuando se instale el paquete
        // if (otelOptions.EnablePrometheus)
        // {
        //     app.UseOpenTelemetryPrometheusScrapingEndpoint();
        // }

        return app;
    }
}

public class OpenTelemetryOptions
{
    public bool Enabled { get; set; } = true;
    public string? OtlpEndpoint { get; set; }
    public string? OtlpHeaders { get; set; }
    public bool UseConsoleExporter { get; set; } = false;
    public bool EnablePrometheus { get; set; } = true;
    public string? DeploymentRegion { get; set; }
}

public static class CimaActivitySource
{
    public const string Name = "cima.Application";
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

    public static Activity? StartArchitectActivity(string operation, Guid? architectId = null)
    {
        var activity = Instance.StartActivity($"Architect.{operation}");
        if (activity != null && architectId.HasValue)
        {
            activity.SetTag("architect.id", architectId.Value.ToString());
        }
        return activity;
    }

    public static Activity? StartCacheActivity(string operation, string key)
    {
        var activity = Instance.StartActivity($"Cache.{operation}");
        activity?.SetTag("cache.key", key);
        return activity;
    }
}

/// <summary>
/// Métricas personalizadas de negocio para CIMA
/// </summary>
public class CimaMetrics
{
    public const string MeterName = "cima.Business";
    
    private readonly Counter<long> _listingsCreated;
    private readonly Counter<long> _listingsPublished;
    private readonly Counter<long> _contactRequestsReceived;
    private readonly Counter<long> _searchesPerformed;
    private readonly Histogram<double> _searchDuration;
    private readonly ObservableGauge<int> _activeListings;
    private readonly ObservableGauge<int> _activeArchitects;

    private int _activeListingsCount;
    private int _activeArchitectsCount;

    public CimaMetrics()
    {
        var meter = new Meter(MeterName, OpenTelemetryConfiguration.ServiceVersion);

        _listingsCreated = meter.CreateCounter<long>(
            "cima.listings.created",
            description: "Número total de listings creados");

        _listingsPublished = meter.CreateCounter<long>(
            "cima.listings.published",
            description: "Número total de listings publicados");

        _contactRequestsReceived = meter.CreateCounter<long>(
            "cima.contact_requests.received",
            description: "Número total de solicitudes de contacto recibidas");

        _searchesPerformed = meter.CreateCounter<long>(
            "cima.searches.performed",
            description: "Número total de búsquedas realizadas");

        _searchDuration = meter.CreateHistogram<double>(
            "cima.searches.duration",
            unit: "ms",
            description: "Duración de las búsquedas en milisegundos");

        _activeListings = meter.CreateObservableGauge(
            "cima.listings.active",
            () => _activeListingsCount,
            description: "Número de listings activos actualmente");

        _activeArchitects = meter.CreateObservableGauge(
            "cima.architects.active",
            () => _activeArchitectsCount,
            description: "Número de arquitectos activos actualmente");
    }

    public void RecordListingCreated(string category = "unknown")
    {
        _listingsCreated.Add(1, new KeyValuePair<string, object?>("category", category));
    }

    public void RecordListingPublished(string transactionType = "unknown")
    {
        _listingsPublished.Add(1, new KeyValuePair<string, object?>("transaction_type", transactionType));
    }

    public void RecordContactRequest(string source = "website")
    {
        _contactRequestsReceived.Add(1, new KeyValuePair<string, object?>("source", source));
    }

    public void RecordSearch(double durationMs, string? searchType = null, int resultsCount = 0)
    {
        _searchesPerformed.Add(1, 
            new KeyValuePair<string, object?>("type", searchType ?? "general"),
            new KeyValuePair<string, object?>("has_results", resultsCount > 0));
        
        _searchDuration.Record(durationMs,
            new KeyValuePair<string, object?>("type", searchType ?? "general"));
    }

    public void SetActiveListingsCount(int count) => _activeListingsCount = count;
    public void SetActiveArchitectsCount(int count) => _activeArchitectsCount = count;
}
