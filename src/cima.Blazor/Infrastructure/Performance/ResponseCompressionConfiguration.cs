using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

namespace cima.Blazor.Infrastructure.Performance;

/// <summary>
/// Configuración de Response Compression (Gzip/Brotli)
/// </summary>
public static class ResponseCompressionConfiguration
{
    public static IServiceCollection AddCimaResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            
            // MIME types a comprimir
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json",
                "application/javascript",
                "text/css",
                "text/html",
                "text/plain",
                "text/xml",
                "application/xml",
                "application/wasm",
                "image/svg+xml",
                "application/font-woff",
                "application/font-woff2"
            });
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        return services;
    }

    public static IApplicationBuilder UseCimaResponseCompression(this IApplicationBuilder app)
    {
        return app.UseResponseCompression();
    }
}
