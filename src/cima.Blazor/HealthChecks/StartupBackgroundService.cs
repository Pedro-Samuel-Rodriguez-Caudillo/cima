using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace cima.Blazor.HealthChecks
{
    /// <summary>
    /// Background service que ejecuta tareas de inicio de la aplicacion
    /// Marca la aplicacion como "ready" cuando todas las tareas completan
    /// </summary>
    public class StartupBackgroundService : BackgroundService
    {
        private readonly StartupHealthCheck _healthCheck;
        private readonly ILogger<StartupBackgroundService> _logger;

        public StartupBackgroundService(
            StartupHealthCheck healthCheck,
            ILogger<StartupBackgroundService> logger)
        {
            _healthCheck = healthCheck;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Iniciando tareas de startup...");

            try
            {
                // Simular verificaciones de inicio (ej: cache warming, config loading, etc)
                // En produccion, aqui irian tareas reales de inicializacion
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                _logger.LogInformation("Tareas de startup completadas exitosamente");
                
                // Marcar la aplicacion como lista
                _healthCheck.IsReady = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante las tareas de startup");
                _healthCheck.IsReady = false;
            }
        }
    }
}
