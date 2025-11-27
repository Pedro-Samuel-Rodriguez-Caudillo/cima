using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace cima.Controllers
{
    /// <summary>
    /// Endpoint simple de ping para Railway
    /// NO usa health checks del sistema - solo verifica que la app responde
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
            return Ok(new
            {
                status = "alive",
                timestamp = DateTime.UtcNow,
                message = "pong",
                application = "cima.Blazor",
                port = Environment.GetEnvironmentVariable("PORT") ?? "unknown"
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
