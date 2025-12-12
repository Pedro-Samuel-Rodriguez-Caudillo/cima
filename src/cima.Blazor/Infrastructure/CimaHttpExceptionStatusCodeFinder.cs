using System;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.DependencyInjection;

namespace cima.Blazor.Infrastructure;

/// <summary>
/// Implementacion personalizada para mapear codigos de error de negocio a HTTP status codes especificos.
/// Esto permite que el frontend distinga entre diferentes tipos de errores basandose en el status HTTP.
/// </summary>
public class CimaHttpExceptionStatusCodeFinder : DefaultHttpExceptionStatusCodeFinder
{
    public CimaHttpExceptionStatusCodeFinder(IOptions<AbpExceptionHttpStatusCodeOptions> options)
        : base(options)
    {
    }

    public override HttpStatusCode GetStatusCode(HttpContext httpContext, Exception exception)
    {
        // Mapear errores de concurrencia a 409 Conflict
        // Esto indica al cliente que el recurso fue modificado por otra operacion concurrente
        if (exception is BusinessException businessException)
        {
            return businessException.Code switch
            {
                "Listing:ConcurrencyConflict" => HttpStatusCode.Conflict, // 409
                _ => base.GetStatusCode(httpContext, exception)
            };
        }

        return base.GetStatusCode(httpContext, exception);
    }
}
