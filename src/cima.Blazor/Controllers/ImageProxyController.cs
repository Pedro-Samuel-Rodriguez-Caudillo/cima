using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace cima.Blazor.Controllers;

/// <summary>
/// Proxy para servir im�genes desde Azure Blob Storage a trav�s de HTTPS.
/// Soluciona el problema de Mixed Content cuando Azurite usa HTTP.
/// </summary>
[Route("api/image-proxy")]
[ApiController]
public class ImageProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ImageProxyController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    /// <summary>
    /// Proxy GET para im�genes desde Azurite/Azure Blob Storage.
    /// Ejemplo: /api/image-proxy?url=http://127.0.0.1:10000/devstoreaccount1/listings/image.png
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return BadRequest("URL parameter is required");
        }

        // Validar que la URL sea del dominio permitido (Azurite o Azure)
        var publicBaseUrl = _configuration["ImageStorage:Azure:PublicBaseUrl"];
        if (!string.IsNullOrEmpty(publicBaseUrl) && !url.StartsWith(publicBaseUrl, StringComparison.OrdinalIgnoreCase))
        {
            // También permitir URLs de Azure Blob Storage en producción
            if (!url.Contains(".blob.core.windows.net"))
            {
                return BadRequest("Invalid image URL");
            }
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var imageBytes = await response.Content.ReadAsByteArrayAsync();

            return File(imageBytes, contentType);
        }
        catch (Exception)
        {
            return StatusCode(500, "Error fetching image");
        }
    }
}

