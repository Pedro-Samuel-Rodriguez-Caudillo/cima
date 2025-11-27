using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Volo.Abp.AspNetCore.Mvc;

namespace cima.Controllers
{
    /// <summary>
    /// Endpoint simple de ping para Railway healthcheck
    /// NO usa health checks del sistema - solo verifica que la app responde
    /// DEBE estar completamente aislado de middleware ABP
    /// </summary>
    [ApiController]
    [Route("api/health")]
    [AllowAnonymous]
    public class HealthController : ControllerBase  // Usar ControllerBase en lugar de cimaController
    {
        /// <summary>
        /// Endpoint basico de ping - usado por Railway
        /// Retorna 200 OK si la aplicacion esta corriendo
        /// Railway usa hostname: healthcheck.railway.app
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            var port = Environment.GetEnvironmentVariable("PORT");
            var aspnetcoreUrls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            var allEnvVars = Environment.GetEnvironmentVariables();
            
            return Ok(new
            {
                status = "alive",
                timestamp = DateTime.UtcNow,
                message = "pong",
                application = "cima.Blazor",
                port = port ?? "not-set",
                aspnetcore_urls = aspnetcoreUrls ?? "not-set",
                // Debugging: mostrar todas las variables que empiezan con RAILWAY o PORT
                railway_environment = Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"),
                railway_service_name = Environment.GetEnvironmentVariable("RAILWAY_SERVICE_NAME"),
                railway_public_domain = Environment.GetEnvironmentVariable("RAILWAY_PUBLIC_DOMAIN"),
                dotnet_running_in_container = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                // Mostrar en qué puerto está escuchando realmente la app
                actual_listening_address = $"{Request.Scheme}://{Request.Host}"
            });
        }
        
        /// <summary>
        /// Endpoint alternativo para Railway - exactamente igual
        /// </summary>
        [HttpGet]
        [HttpGet("")]
        public IActionResult Index()
        {
            return Ping();
        }
    }
}
