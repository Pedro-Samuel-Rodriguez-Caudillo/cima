using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace cima.Notifications;

/// <summary>
/// Configuración del servicio de notificaciones por email.
/// Soporta múltiples proveedores: AzureCommunicationServices, SMTP
/// </summary>
public static class EmailNotificationServiceExtensions
{
    /// <summary>
    /// Registra el servicio de notificaciones por email según configuración.
    /// Usa Email:Provider para determinar qué implementación usar.
    /// </summary>
    public static IServiceCollection AddEmailNotificationService(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Registrar servicio de plantillas
        services.AddTransient<IEmailTemplateService, EmailTemplateService>();
        
        var provider = configuration["Email:Provider"]?.ToLowerInvariant() ?? "smtp";

        switch (provider)
        {
            case "azurecommunicationservices":
            case "azure":
            case "acs":
                services.AddTransient<IEmailNotificationService, AzureEmailNotificationService>();
                break;
            
            case "smtp":
            default:
                services.AddTransient<IEmailNotificationService, SmtpEmailNotificationService>();
                break;
        }

        return services;
    }
}
