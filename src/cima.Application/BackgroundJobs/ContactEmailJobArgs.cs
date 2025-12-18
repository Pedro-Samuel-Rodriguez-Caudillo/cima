using System;

namespace cima.BackgroundJobs;

/// <summary>
/// Argumentos para el job de envío de email de contacto
/// </summary>
[Serializable]
public class ContactEmailJobArgs
{
    /// <summary>
    /// Email del administrador o arquitecto a notificar
    /// </summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del cliente que envió la solicitud
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Email del cliente
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono del cliente (opcional)
    /// </summary>
    public string? CustomerPhone { get; set; }

    /// <summary>
    /// Mensaje de la solicitud
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Título de la propiedad (o "Consulta General" si es general)
    /// </summary>
    public string PropertyTitle { get; set; } = string.Empty;

    /// <summary>
    /// URL de la propiedad
    /// </summary>
    public string PropertyUrl { get; set; } = string.Empty;

    /// <summary>
    /// Indica si también se debe enviar confirmación al cliente
    /// </summary>
    public bool SendConfirmationToCustomer { get; set; } = true;
}
