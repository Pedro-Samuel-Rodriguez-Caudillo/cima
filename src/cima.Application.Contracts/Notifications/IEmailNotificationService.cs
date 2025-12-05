using System.Threading.Tasks;

namespace cima.Notifications;

/// <summary>
/// Servicio para envío de notificaciones por email
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>
    /// Envía notificación de nueva solicitud de contacto al administrador
    /// </summary>
    Task SendContactRequestNotificationAsync(ContactRequestNotificationDto notification);
    
    /// <summary>
    /// Envía confirmación al usuario que envió la solicitud
    /// </summary>
    Task SendContactRequestConfirmationAsync(ContactRequestConfirmationDto confirmation);
    
    /// <summary>
    /// Envía notificación de contraseña temporal a nuevo arquitecto
    /// </summary>
    Task SendArchitectWelcomeEmailAsync(ArchitectWelcomeEmailDto welcome);
}

/// <summary>
/// DTO para notificación de solicitud de contacto
/// </summary>
public class ContactRequestNotificationDto
{
    public string AdminEmail { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string? CustomerPhone { get; set; }
    public string Message { get; set; } = "";
    public string? PropertyTitle { get; set; }
    public string? PropertyUrl { get; set; }
}

/// <summary>
/// DTO para confirmación de solicitud de contacto
/// </summary>
public class ContactRequestConfirmationDto
{
    public string CustomerEmail { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string? PropertyTitle { get; set; }
}

/// <summary>
/// DTO para email de bienvenida a arquitecto
/// </summary>
public class ArchitectWelcomeEmailDto
{
    public string ArchitectEmail { get; set; } = "";
    public string ArchitectName { get; set; } = "";
    public string TemporaryPassword { get; set; } = "";
    public string LoginUrl { get; set; } = "";
}
