using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace cima.Notifications;

/// <summary>
/// Configuracion del servicio de notificaciones por email.
/// Soporta multiples proveedores y delega la seleccion a settings/configuracion.
/// </summary>
public static class EmailNotificationServiceExtensions
{
    /// <summary>
    /// Registra el servicio de notificaciones por email.
    /// </summary>
    public static IServiceCollection AddEmailNotificationService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        _ = configuration; // Mantener simetria con otros modulos

        services.AddTransient<IEmailTemplateService, EmailTemplateService>();
        services.AddHttpClient<BrevoEmailNotificationService>();
        services.AddTransient<IEmailDeliveryProvider, AzureEmailNotificationService>();
        services.AddTransient<IEmailDeliveryProvider, SmtpEmailNotificationService>();
        services.AddTransient<IEmailDeliveryProvider, BrevoEmailNotificationService>();

        services.AddTransient<IEmailNotificationService, SwitchingEmailNotificationService>();
        return services;
    }
}
