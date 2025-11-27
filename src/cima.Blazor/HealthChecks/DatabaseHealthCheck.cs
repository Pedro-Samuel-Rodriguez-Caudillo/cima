using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace cima.Blazor.HealthChecks
{
    /// <summary>
    /// Health check que verifica migraciones pendientes de la base de datos
    /// Usa connection string directa en lugar de DbContext para evitar requerir UoW
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;

        public DatabaseHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("Default");
                
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return HealthCheckResult.Unhealthy("Connection string 'Default' no esta configurada");
                }

                // Crear un DbContext temporal solo para verificar migraciones
                // No usa UoW porque es standalone
                var optionsBuilder = new DbContextOptionsBuilder<EntityFrameworkCore.cimaDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                using var dbContext = new EntityFrameworkCore.cimaDbContext(optionsBuilder.Options);
                
                // Verificar que no hay migraciones pendientes
                var pendingMigrations = await dbContext.Database
                    .GetPendingMigrationsAsync(cancellationToken);
                
                if (pendingMigrations.Any())
                {
                    return HealthCheckResult.Degraded(
                        $"Hay {pendingMigrations.Count()} migraciones pendientes: {string.Join(", ", pendingMigrations.Take(3))}...");
                }

                // Verificar que hay al menos una migración aplicada
                var appliedMigrations = await dbContext.Database
                    .GetAppliedMigrationsAsync(cancellationToken);
                
                if (!appliedMigrations.Any())
                {
                    return HealthCheckResult.Degraded("No hay migraciones aplicadas a la base de datos");
                }

                return HealthCheckResult.Healthy(
                    $"Base de datos OK. {appliedMigrations.Count()} migraciones aplicadas, 0 pendientes");
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
