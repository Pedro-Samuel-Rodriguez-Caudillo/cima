using System;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using cima.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace cima.Notifications;

/// <summary>
/// Implementacion del proveedor de notificaciones usando Azure Communication Services Email.
/// Se registra como estrategia concreta y es seleccionado por <see cref="SwitchingEmailNotificationService"/>.
/// </summary>
public class AzureEmailNotificationService : IEmailDeliveryProvider, ITransientDependency
{
    public string Name => EmailProviderNames.AzureCommunicationServices;

    private readonly IConfiguration _configuration;
    private readonly ISettingProvider _settingProvider;
    private readonly ILogger<AzureEmailNotificationService> _logger;

    public AzureEmailNotificationService(
        IConfiguration configuration,
        ISettingProvider settingProvider,
        ILogger<AzureEmailNotificationService> logger)
    {
        _configuration = configuration;
        _settingProvider = settingProvider;
        _logger = logger;
    }

    public async Task SendContactRequestNotificationAsync(ContactRequestNotificationDto notification)
    {
        // Obtener email de admin desde settings (prioridad) o usar el del DTO
        var adminEmail = await GetAdminEmailAsync() ?? notification.AdminEmail;

        if (string.IsNullOrEmpty(adminEmail))
        {
            _logger.LogWarning("No hay email de admin configurado. Notificacion no enviada.");
            return;
        }

        var subject = notification.PropertyTitle != null
            ? $"[4cima] Nueva consulta sobre: {notification.PropertyTitle}"
            : "[4cima] Nueva solicitud de contacto";

        var htmlContent = BuildContactRequestNotificationHtml(notification);

        await SendEmailAsync(adminEmail, subject, htmlContent);
    }

    public async Task SendContactRequestConfirmationAsync(ContactRequestConfirmationDto confirmation)
    {
        var subject = "[4cima] Hemos recibido tu mensaje";
        var htmlContent = BuildContactRequestConfirmationHtml(confirmation);

        await SendEmailAsync(confirmation.CustomerEmail, subject, htmlContent);
    }

    public async Task SendArchitectWelcomeEmailAsync(ArchitectWelcomeEmailDto welcome)
    {
        var subject = "[4cima] Bienvenido - Tu cuenta ha sido creada";
        var htmlContent = BuildArchitectWelcomeHtml(welcome);

        await SendEmailAsync(welcome.ArchitectEmail, subject, htmlContent);
    }

    /// <summary>
    /// Obtiene email de admin desde settings o config
    /// </summary>
    private async Task<string?> GetAdminEmailAsync()
    {
        // Primero intentar desde settings de BD
        var settingEmail = await _settingProvider.GetOrNullAsync(SiteSettingNames.AdminNotificationEmail);
        if (!string.IsNullOrEmpty(settingEmail))
        {
            return settingEmail;
        }

        // Fallback a configuracion
        return _configuration["Email:AdminNotification"];
    }

    /// <summary>
    /// Obtiene configuracion de Azure CS desde settings o config
    /// </summary>
    private async Task<(string? connectionString, string? senderAddress)> GetAzureConfigAsync()
    {
        // Intentar desde settings de BD
        var connectionString = await _settingProvider.GetOrNullAsync(SiteSettingNames.AzureEmailConnectionString);
        var senderAddress = await _settingProvider.GetOrNullAsync(SiteSettingNames.AzureEmailSenderAddress);

        // Fallback a configuracion si no hay en settings
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = _configuration["Email:AzureCommunicationServices:ConnectionString"];
        }
        if (string.IsNullOrEmpty(senderAddress))
        {
            senderAddress = _configuration["Email:AzureCommunicationServices:SenderAddress"];
        }

        return (connectionString, senderAddress);
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var (connectionString, senderAddress) = await GetAzureConfigAsync();

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(senderAddress))
        {
            _logger.LogWarning("Azure Communication Services Email no configurado. Email no enviado a {To}: {Subject}", to, subject);
            return;
        }

        try
        {
            var emailClient = new EmailClient(connectionString);

            var emailMessage = new EmailMessage(
                senderAddress: senderAddress,
                recipientAddress: to,
                content: new EmailContent(subject)
                {
                    Html = htmlBody
                });

            EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage);

            _logger.LogInformation(
                "Email enviado exitosamente via Azure CS a {To}: {Subject}. OperationId: {OperationId}",
                to, subject, emailSendOperation.Id);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Error de Azure Communication Services al enviar email a {To}: {Subject}. Code: {Code}",
                to, subject, ex.ErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email via Azure CS a {To}: {Subject}", to, subject);
        }
    }

    internal static string BuildContactRequestNotificationHtml(ContactRequestNotificationDto notification)
    {
        return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #1a365d 0%, #2d4a7c 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
        <h1 style='margin: 0; font-size: 28px;'>4cima</h1>
        <p style='margin: 10px 0 0 0; opacity: 0.9;'>Nueva solicitud de contacto</p>
    </div>
    
    <div style='padding: 30px; background: #f8fafc; border-radius: 0 0 8px 8px;'>
        <h2 style='color: #1a365d; margin-top: 0; font-size: 20px;'>Datos del interesado</h2>
        
        <table style='width: 100%; border-collapse: collapse; margin-bottom: 20px;'>
            <tr>
                <td style='padding: 12px; font-weight: 600; width: 120px; color: #64748b;'>Nombre:</td>
                <td style='padding: 12px; color: #1e293b;'>{System.Net.WebUtility.HtmlEncode(notification.CustomerName)}</td>
            </tr>
            <tr style='background: white;'>
                <td style='padding: 12px; font-weight: 600; color: #64748b;'>Email:</td>
                <td style='padding: 12px;'><a href='mailto:{notification.CustomerEmail}' style='color: #1a365d;'>{notification.CustomerEmail}</a></td>
            </tr>
            {(string.IsNullOrEmpty(notification.CustomerPhone) ? "" : $@"
            <tr>
                <td style='padding: 12px; font-weight: 600; color: #64748b;'>Telefono:</td>
                <td style='padding: 12px;'><a href='tel:{notification.CustomerPhone}' style='color: #1a365d;'>{notification.CustomerPhone}</a></td>
            </tr>")}
        </table>
        
        {(string.IsNullOrEmpty(notification.PropertyTitle) ? "" : $@"
        <div style='background: white; padding: 15px; border-radius: 6px; margin-bottom: 20px;'>
            <h3 style='color: #1a365d; margin: 0 0 10px 0; font-size: 16px;'>Propiedad de interes</h3>
            <a href='{notification.PropertyUrl}' style='color: #1a365d; font-weight: 500;'>{System.Net.WebUtility.HtmlEncode(notification.PropertyTitle)}</a>
        </div>")}
        
        <div style='background: white; padding: 20px; border-radius: 6px; border-left: 4px solid #1a365d;'>
            <h3 style='color: #1a365d; margin: 0 0 10px 0; font-size: 16px;'>Mensaje</h3>
            <p style='margin: 0; color: #475569; line-height: 1.6; white-space: pre-wrap;'>{System.Net.WebUtility.HtmlEncode(notification.Message)}</p>
        </div>
    </div>
    
    <p style='text-align: center; color: #94a3b8; font-size: 12px; margin-top: 20px;'>
        Este email fue enviado automaticamente desde 4cima
    </p>
</body>
</html>";
    }

    internal static string BuildContactRequestConfirmationHtml(ContactRequestConfirmationDto confirmation)
    {
        return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #1a365d 0%, #2d4a7c 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
        <h1 style='margin: 0; font-size: 28px;'>4cima</h1>
    </div>
    
    <div style='padding: 30px; background: #f8fafc; border-radius: 0 0 8px 8px;'>
        <h2 style='color: #1a365d; margin-top: 0;'>Gracias por contactarnos, {System.Net.WebUtility.HtmlEncode(confirmation.CustomerName)}!</h2>
        
        <p style='color: #475569; line-height: 1.6;'>
            Hemos recibido tu mensaje{(string.IsNullOrEmpty(confirmation.PropertyTitle) ? "" : $" sobre la propiedad <strong>{System.Net.WebUtility.HtmlEncode(confirmation.PropertyTitle)}</strong>")}.
        </p>
        
        <p style='color: #475569; line-height: 1.6;'>
            Nuestro equipo revisara tu solicitud y te contactaremos a la brevedad posible.
        </p>
        
        <div style='background: white; padding: 20px; border-radius: 6px; margin-top: 25px;'>
            <p style='margin: 0; color: #64748b; font-size: 14px;'>
                <strong>Horario de atencion:</strong><br>
                Lunes a Viernes: 9:00 AM - 6:00 PM<br>
                Sabado: 10:00 AM - 2:00 PM
            </p>
        </div>
    </div>
    
    <p style='text-align: center; color: #94a3b8; font-size: 12px; margin-top: 20px;'>
        4cima - Arquitectura - Diseno - Exclusividad
    </p>
</body>
</html>";
    }

    internal static string BuildArchitectWelcomeHtml(ArchitectWelcomeEmailDto welcome)
    {
        return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #1a365d 0%, #2d4a7c 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
        <h1 style='margin: 0; font-size: 28px;'>4cima</h1>
        <p style='margin: 10px 0 0 0; opacity: 0.9;'>Portal de Arquitectos</p>
    </div>
    
    <div style='padding: 30px; background: #f8fafc; border-radius: 0 0 8px 8px;'>
        <h2 style='color: #1a365d; margin-top: 0;'>Bienvenido, {System.Net.WebUtility.HtmlEncode(welcome.ArchitectName)}!</h2>
        
        <p style='color: #475569; line-height: 1.6;'>
            Tu cuenta de arquitecto en 4cima ha sido creada exitosamente.
        </p>
        
        <div style='background: #fef3c7; border: 1px solid #f59e0b; padding: 15px; border-radius: 6px; margin: 20px 0;'>
            <p style='margin: 0; color: #92400e; font-size: 14px;'>
                <strong>Importante:</strong> Por seguridad, deberas cambiar tu contrasena temporal en tu primer inicio de sesion.
            </p>
        </div>
        
        <div style='background: white; padding: 20px; border-radius: 6px;'>
            <h3 style='margin-top: 0; color: #1a365d; font-size: 16px;'>Credenciales de acceso</h3>
            <p style='margin: 8px 0; color: #475569;'><strong>Email:</strong> {welcome.ArchitectEmail}</p>
            <p style='margin: 8px 0; color: #475569;'><strong>Contrasena temporal:</strong> <code style='background: #e2e8f0; padding: 4px 8px; border-radius: 4px; font-family: monospace;'>{welcome.TemporaryPassword}</code></p>
        </div>
        
        <div style='text-align: center; margin-top: 25px;'>
            <a href='{welcome.LoginUrl}' style='display: inline-block; background: linear-gradient(135deg, #1a365d 0%, #2d4a7c 100%); color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600;'>
                Iniciar Sesion
            </a>
        </div>
    </div>
    
    <p style='text-align: center; color: #94a3b8; font-size: 12px; margin-top: 20px;'>
        Si no solicitaste esta cuenta, por favor ignora este mensaje.
    </p>
</body>
</html>";
    }
}
