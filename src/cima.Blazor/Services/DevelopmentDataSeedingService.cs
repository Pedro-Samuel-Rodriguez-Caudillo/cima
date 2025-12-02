using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;

namespace cima.Blazor.Services;

/// <summary>
/// Servicio en background que ejecuta el seeding de datos de desarrollo
/// Solo se ejecuta en entorno Development
/// </summary>
public class DevelopmentDataSeedingService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DevelopmentDataSeedingService> _logger;

    public DevelopmentDataSeedingService(
        IServiceProvider serviceProvider,
        ILogger<DevelopmentDataSeedingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Esperar un poco para asegurar que la aplicación esté completamente inicializada
        await Task.Delay(2000, cancellationToken);

        _logger.LogInformation("=== DESARROLLO: Iniciando seeding de datos de prueba ===");

        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();

                _logger.LogInformation("Ejecutando DataSeeder (incluye DevelopmentDataSeeder)...");
                await dataSeeder.SeedAsync(new DataSeedContext());
                _logger.LogInformation("DataSeeder completado exitosamente");
            }

            _logger.LogInformation("=== SEEDING DE DESARROLLO COMPLETADO ===");
        }
        catch (Exception ex)
        {
            // No romper la app si falla el seeding de desarrollo
            _logger.LogWarning(ex, "Advertencia: Error durante el seeding de desarrollo (no crítico)");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
