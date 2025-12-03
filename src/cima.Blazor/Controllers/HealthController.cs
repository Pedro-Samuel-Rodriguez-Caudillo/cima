using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace cima.Blazor.Controllers;

/// <summary>
/// Endpoint simple de health check para Railway y otros servicios
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Endpoint basico de ping - usado por Railway
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
            port = Environment.GetEnvironmentVariable("PORT") ?? "not-set",
            aspnetcore_urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "not-set",
            railway_environment = Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"),
            actual_listening_address = $"{Request.Scheme}://{Request.Host}"
        });
    }
    
    /// <summary>
    /// Endpoint raiz de health
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "cima"
        });
    }
}
