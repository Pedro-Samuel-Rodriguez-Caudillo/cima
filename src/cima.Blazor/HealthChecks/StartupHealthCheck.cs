using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace cima.Blazor.HealthChecks
{
    /// <summary>
    /// Health check que verifica si la aplicacion completo su proceso de inicio
    /// Usado para readiness probe - la app no debe recibir trafico hasta que este lista
    /// </summary>
    public class StartupHealthCheck : IHealthCheck
    {
        private volatile bool _isReady = false;

        public bool IsReady
        {
            get => _isReady;
            set => _isReady = value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (_isReady)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("La aplicacion ha completado el inicio y esta lista para recibir trafico"));
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy("La aplicacion aun esta iniciando"));
        }
    }
}
