using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using cima.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace cima.Notifications;

/// <summary>
/// Implementación del servicio de notificaciones por email usando SMTP
/// </summary>
public class SmtpEmailNotificationService : IEmailNotificationService, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailNotificationService> _logger;

    public SmtpEmailNotificationService(
        IConfiguration configuration,
        ILogger<SmtpEmailNotificationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendContactRequestNotificationAsync(ContactRequestNotificationDto notification)
    {
        var subject = notification.PropertyTitle != null
            ? $"[4cima] Nueva consulta sobre: {notification.PropertyTitle}"
            : "[4cima] Nueva solicitud de contacto";

        var body = $@"
<html>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background-color: #1a365d; color: white; padding: 20px; text-align: center;'>
        <h1 style='margin: 0;'>4cima</h1>
        <p style='margin: 5px 0 0 0;'>Nueva solicitud de contacto</p>
    </div>
    
    <div style='padding: 20px; background-color: #f8f9fa;'>
        <h2 style='color: #1a365d; margin-top: 0;'>Datos del interesado</h2>
        
        <table style='width: 100%; border-collapse: collapse;'>
            <tr>
                <td style='padding: 8px 0; font-weight: bold; width: 120px;'>Nombre:</td>
                <td style='padding: 8px 0;'>{WebUtility.HtmlEncode(notification.CustomerName)}</td>
            </tr>
            <tr>
                <td style='padding: 8px 0; font-weight: bold;'>Email:</td>
                <td style='padding: 8px 0;'><a href='mailto:{notification.CustomerEmail}'>{notification.CustomerEmail}</a></td>
            </tr>
            {(string.IsNullOrEmpty(notification.CustomerPhone) ? "" : $@"
            <tr>
                <td style='padding: 8px 0; font-weight: bold;'>Teléfono:</td>
                <td style='padding: 8px 0;'><a href='tel:{notification.CustomerPhone}'>{notification.CustomerPhone}</a></td>
            </tr>")}
        </table>
        
        {(string.IsNullOrEmpty(notification.PropertyTitle) ? "" : $@"
        <h3 style='color: #1a365d; margin-top: 20px;'>Propiedad de interés</h3>
        <p><a href='{notification.PropertyUrl}' style='color: #1a365d;'>{WebUtility.HtmlEncode(notification.PropertyTitle)}</a></p>")}
        
        <h3 style='color: #1a365d; margin-top: 20px;'>Mensaje</h3>
        <div style='background-color: white; padding: 15px; border-radius: 5px; border-left: 4px solid #1a365d;'>
            <p style='margin: 0; white-space: pre-wrap;'>{WebUtility.HtmlEncode(notification.Message)}</p>
        </div>
    </div>
    
    <div style='padding: 15px; text-align: center; color: #666; font-size: 12px;'>
        <p>Este email fue enviado automáticamente desde el sitio web de 4cima.</p>
    </div>
</body>
</html>";

        await SendEmailAsync(notification.AdminEmail, subject, body);
    }

    public async Task SendContactRequestConfirmationAsync(ContactRequestConfirmationDto confirmation)
    {
        var subject = "[4cima] Hemos recibido tu mensaje";

        var body = $@"
<html>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background-color: #1a365d; color: white; padding: 20px; text-align: center;'>
        <h1 style='margin: 0;'>4cima</h1>
    </div>
    
    <div style='padding: 20px;'>
        <h2 style='color: #1a365d;'>¡Gracias por contactarnos, {WebUtility.HtmlEncode(confirmation.CustomerName)}!</h2>
        
        <p>Hemos recibido tu mensaje{(string.IsNullOrEmpty(confirmation.PropertyTitle) ? "" : $" sobre la propiedad <strong>{WebUtility.HtmlEncode(confirmation.PropertyTitle)}</strong>")}.</p>
        
        <p>Nuestro equipo revisará tu solicitud y te contactaremos a la brevedad posible.</p>
        
        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin-top: 20px;'>
            <p style='margin: 0; color: #666;'>
                <strong>Horario de atención:</strong><br>
                Lunes a Viernes: 9:00 AM - 6:00 PM<br>
                Sábado: 10:00 AM - 2:00 PM
            </p>
        </div>
    </div>
    
    <div style='padding: 15px; text-align: center; color: #666; font-size: 12px; border-top: 1px solid #eee;'>
        <p>4cima - Arquitectura · Diseño · Exclusividad</p>
    </div>
</body>
</html>";

        await SendEmailAsync(confirmation.CustomerEmail, subject, body);
    }

    public async Task SendArchitectWelcomeEmailAsync(ArchitectWelcomeEmailDto welcome)
    {
        var subject = "[4cima] Bienvenido - Tu cuenta ha sido creada";

        var body = $@"
<html>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background-color: #1a365d; color: white; padding: 20px; text-align: center;'>
        <h1 style='margin: 0;'>4cima</h1>
        <p style='margin: 5px 0 0 0;'>Portal de Arquitectos</p>
    </div>
    
    <div style='padding: 20px;'>
        <h2 style='color: #1a365d;'>¡Bienvenido, {WebUtility.HtmlEncode(welcome.ArchitectName)}!</h2>
        
        <p>Tu cuenta de arquitecto en 4cima ha sido creada exitosamente.</p>
        
        <div style='background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 5px; margin: 20px 0;'>
            <p style='margin: 0; color: #856404;'>
                <strong>?? Importante:</strong> Por seguridad, deberás cambiar tu contraseña temporal en tu primer inicio de sesión.
            </p>
        </div>
        
        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px;'>
            <h3 style='margin-top: 0; color: #1a365d;'>Credenciales de acceso</h3>
            <p><strong>Email:</strong> {welcome.ArchitectEmail}</p>
            <p><strong>Contraseña temporal:</strong> <code style='background: #e9ecef; padding: 2px 6px; border-radius: 3px;'>{welcome.TemporaryPassword}</code></p>
        </div>
        
        <div style='text-align: center; margin-top: 25px;'>
            <a href='{welcome.LoginUrl}' style='display: inline-block; background-color: #1a365d; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                Iniciar Sesión
            </a>
        </div>
    </div>
    
    <div style='padding: 15px; text-align: center; color: #666; font-size: 12px; border-top: 1px solid #eee;'>
        <p>Si no solicitaste esta cuenta, por favor ignora este mensaje.</p>
    </div>
</body>
</html>";

        await SendEmailAsync(welcome.ArchitectEmail, subject, body);
    }

    private async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var smtpSettings = _configuration.GetSection("Email:Smtp");
        
        var host = smtpSettings["Host"];
        var port = int.Parse(smtpSettings["Port"] ?? "587");
        var userName = smtpSettings["UserName"];
        var password = smtpSettings["Password"];
        var fromAddress = smtpSettings["FromAddress"] ?? userName;
        var fromName = smtpSettings["FromName"] ?? "4cima";
        var enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(userName) || string.IsNullOrWhiteSpace(fromAddress))
        {
            _logger.LogWarning("Email SMTP no configurado. Email no enviado a {To}: {Subject}", to, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = enableSsl
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            
            _logger.LogInformation("Email enviado exitosamente a {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email a {To}: {Subject}", to, subject);
            // No lanzar excepción para no interrumpir el flujo principal
        }
    }
}
