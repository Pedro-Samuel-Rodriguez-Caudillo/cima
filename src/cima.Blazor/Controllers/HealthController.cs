using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cima.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace cima.Blazor.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [AllowAnonymous] // Permitir acceso sin autenticacion para Docker health checks
    public class HealthController : ControllerBase
    {
        private readonly cimaDbContext _dbContext;

        public HealthController(cimaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Verificar conexion a base de datos
                var canConnect = await _dbContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(503, new
                    {
                        status = "unhealthy",
                        timestamp = DateTime.UtcNow,
                        database = "disconnected",
                        application = "cima.Blazor",
                        message = "Cannot connect to database"
                    });
                }

                // Verificar que al menos una tabla existe (verificacion adicional)
                var listingCount = await _dbContext.Listings.CountAsync();
                
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    database = "connected",
                    application = "cima.Blazor",
                    version = "1.0.0",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    checks = new
                    {
                        database_connection = "ok",
                        listings_table = "ok",
                        total_listings = listingCount
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    database = "error",
                    application = "cima.Blazor",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                status = "alive",
                timestamp = DateTime.UtcNow,
                message = "pong"
            });
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready()
        {
            try
            {
                // Verificacion mas profunda para readiness probe
                var canConnect = await _dbContext.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(503, new { ready = false, reason = "database_unavailable" });
                }

                // Verificar que las migraciones estan aplicadas
                var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    return StatusCode(503, new
                    {
                        ready = false,
                        reason = "pending_migrations",
                        pending = pendingMigrations
                    });
                }

                return Ok(new
                {
                    ready = true,
                    timestamp = DateTime.UtcNow,
                    checks = new
                    {
                        database = "ready",
                        migrations = "applied"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    ready = false,
                    reason = "exception",
                    error = ex.Message
                });
            }
        }
    }
}