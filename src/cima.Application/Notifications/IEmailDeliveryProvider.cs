using System.Threading.Tasks;

namespace cima.Notifications;

/// <summary>
/// Estrategia concreta para enviar correos mediante un proveedor específico.
/// </summary>
public interface IEmailDeliveryProvider
{
    /// <summary>
    /// Identificador del proveedor (usar constantes de <see cref="EmailProviderNames"/>).
    /// </summary>
    string Name { get; }

    Task SendContactRequestNotificationAsync(ContactRequestNotificationDto notification);
    Task SendContactRequestConfirmationAsync(ContactRequestConfirmationDto confirmation);
    Task SendArchitectWelcomeEmailAsync(ArchitectWelcomeEmailDto welcome);
}
