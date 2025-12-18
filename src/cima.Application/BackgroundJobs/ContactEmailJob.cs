using System.Threading.Tasks;
using cima.Notifications;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace cima.BackgroundJobs;

/// <summary>
/// Background job para enviar emails de contacto de forma asíncrona.
/// Esto mejora la experiencia del usuario al no bloquear la request HTTP.
/// </summary>
public class ContactEmailJob : AsyncBackgroundJob<ContactEmailJobArgs>, ITransientDependency
{
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<ContactEmailJob> _logger;

    public ContactEmailJob(
        IEmailNotificationService emailService,
        ILogger<ContactEmailJob> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public override async Task ExecuteAsync(ContactEmailJobArgs args)
    {
        _logger.LogInformation(
            "Ejecutando ContactEmailJob: Notificando a {AdminEmail} sobre solicitud de {CustomerName}",
            args.AdminEmail,
            args.CustomerName);

        try
        {
            // Enviar notificación al admin/arquitecto
            if (!string.IsNullOrWhiteSpace(args.AdminEmail))
            {
                await _emailService.SendContactRequestNotificationAsync(new ContactRequestNotificationDto
                {
                    AdminEmail = args.AdminEmail,
                    CustomerName = args.CustomerName,
                    CustomerEmail = args.CustomerEmail,
                    CustomerPhone = args.CustomerPhone,
                    Message = args.Message,
                    PropertyTitle = args.PropertyTitle,
                    PropertyUrl = args.PropertyUrl
                });

                _logger.LogInformation("Notificación enviada a admin {AdminEmail}", args.AdminEmail);
            }

            // Enviar confirmación al cliente
            if (args.SendConfirmationToCustomer && !string.IsNullOrWhiteSpace(args.CustomerEmail))
            {
                await _emailService.SendContactRequestConfirmationAsync(new ContactRequestConfirmationDto
                {
                    CustomerEmail = args.CustomerEmail,
                    CustomerName = args.CustomerName,
                    PropertyTitle = args.PropertyTitle
                });

                _logger.LogInformation("Confirmación enviada a cliente {CustomerEmail}", args.CustomerEmail);
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, 
                "Error en ContactEmailJob al enviar email a {AdminEmail} o {CustomerEmail}",
                args.AdminEmail,
                args.CustomerEmail);
            
            // Re-throw para que ABP pueda reintentar el job
            throw;
        }
    }
}
