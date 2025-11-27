using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using cima.EntityFrameworkCore;

namespace cima.Blazor.HealthChecks
{
    /// <summary>
    /// Health check que verifica la conexion a PostgreSQL
    /// y que las migraciones esten aplicadas
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly cimaDbContext _dbContext;

        public DatabaseHealthCheck(cimaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Verificar conexion a la base de datos
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
                
                if (!canConnect)
                {
                    return HealthCheckResult.Unhealthy(
                        "No se puede conectar a la base de datos PostgreSQL");
                }

                // 2. Verificar que no hay migraciones pendientes
                var pendingMigrations = await _dbContext.Database
                    .GetPendingMigrationsAsync(cancellationToken);
                
                if (pendingMigrations.Any())
                {
                    return HealthCheckResult.Degraded(
                        $"Hay {pendingMigrations.Count()} migraciones pendientes: {string.Join(", ", pendingMigrations)}");
                }

                // 3. Verificar que podemos ejecutar queries (opcional)
                var listingCount = await _dbContext.Listings
                    .CountAsync(cancellationToken);

                return HealthCheckResult.Healthy(
                    $"Base de datos conectada. {listingCount} listings en la base de datos");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Error al verificar la base de datos",
                    ex);
            }
        }
    }
}
