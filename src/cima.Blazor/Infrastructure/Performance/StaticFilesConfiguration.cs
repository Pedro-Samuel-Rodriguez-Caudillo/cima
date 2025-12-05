using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;

namespace cima.Blazor.Infrastructure.Performance;

/// <summary>
/// Configuración optimizada para archivos estáticos
/// </summary>
public static class StaticFilesConfiguration
{
    /// <summary>
    /// Configura static files con cache headers optimizados
    /// </summary>
    public static IApplicationBuilder UseCimaStaticFiles(this IApplicationBuilder app, string webRootPath)
    {
        var provider = new FileExtensionContentTypeProvider();
        
        // Agregar tipos MIME adicionales
        provider.Mappings[".wasm"] = "application/wasm";
        provider.Mappings[".blat"] = "application/octet-stream";
        provider.Mappings[".dat"] = "application/octet-stream";
        provider.Mappings[".dll"] = "application/octet-stream";
        provider.Mappings[".pdb"] = "application/octet-stream";
        provider.Mappings[".woff"] = "font/woff";
        provider.Mappings[".woff2"] = "font/woff2";

        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = provider,
            OnPrepareResponse = ctx =>
            {
                var headers = ctx.Context.Response.Headers;
                var path = ctx.File.Name.ToLowerInvariant();
                var requestPath = ctx.Context.Request.Path.Value?.ToLowerInvariant() ?? "";

                // Framework Blazor - cache largo (versioned)
                if (requestPath.Contains("/_framework/"))
                {
                    // 1 año para archivos versionados
                    headers[HeaderNames.CacheControl] = "public, max-age=31536000, immutable";
                    return;
                }

                // Archivos hash/versionados - cache muy largo
                if (IsVersionedFile(path))
                {
                    headers[HeaderNames.CacheControl] = "public, max-age=31536000, immutable";
                    return;
                }

                // Fuentes - cache largo
                if (path.EndsWith(".woff") || path.EndsWith(".woff2") || path.EndsWith(".ttf"))
                {
                    headers[HeaderNames.CacheControl] = "public, max-age=31536000";
                    return;
                }

                // Imágenes - cache medio
                if (path.EndsWith(".png") || path.EndsWith(".jpg") || path.EndsWith(".jpeg") || 
                    path.EndsWith(".gif") || path.EndsWith(".webp") || path.EndsWith(".svg"))
                {
                    headers[HeaderNames.CacheControl] = "public, max-age=86400"; // 1 día
                    return;
                }

                // CSS/JS - cache corto con revalidación
                if (path.EndsWith(".css") || path.EndsWith(".js"))
                {
                    headers[HeaderNames.CacheControl] = "public, max-age=3600, must-revalidate"; // 1 hora
                    return;
                }

                // HTML - no cache (siempre verificar)
                if (path.EndsWith(".html"))
                {
                    headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
                    headers[HeaderNames.Pragma] = "no-cache";
                    headers[HeaderNames.Expires] = "0";
                    return;
                }

                // Default - cache corto
                headers[HeaderNames.CacheControl] = "public, max-age=600"; // 10 minutos
            }
        });

        return app;
    }

    private static bool IsVersionedFile(string fileName)
    {
        // Detecta archivos con hash en el nombre (ej: app.abc123.js)
        var parts = fileName.Split('.');
        if (parts.Length >= 3)
        {
            var potentialHash = parts[^2]; // Penúltimo segmento
            return potentialHash.Length >= 8 && potentialHash.All(c => char.IsLetterOrDigit(c));
        }
        return false;
    }
}
