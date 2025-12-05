using System;
using System.ComponentModel.DataAnnotations;

namespace cima.Settings;

/// <summary>
/// DTO para configuración del sitio (admin)
/// </summary>
public class SiteSettingsDto
{
    // === Configuración de Contacto ===
    
    /// <summary>
    /// Email principal para recibir notificaciones de contacto
    /// </summary>
    [EmailAddress]
    public string? AdminNotificationEmail { get; set; }
    
    /// <summary>
    /// Número de WhatsApp para contacto directo (formato internacional sin +)
    /// Ejemplo: 5215512345678
    /// </summary>
    [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Formato inválido. Use solo números, 10-15 dígitos")]
    public string? WhatsAppNumber { get; set; }
    
    /// <summary>
    /// Mensaje predeterminado para WhatsApp
    /// </summary>
    [MaxLength(500)]
    public string? WhatsAppDefaultMessage { get; set; }
    
    /// <summary>
    /// Habilitar botón de WhatsApp en el sitio
    /// </summary>
    public bool WhatsAppEnabled { get; set; }
    
    // === Configuración de Email (Azure Communication Services) ===
    
    /// <summary>
    /// Connection string de Azure Communication Services
    /// </summary>
    public string? AzureEmailConnectionString { get; set; }
    
    /// <summary>
    /// Dirección del remitente de Azure CS
    /// </summary>
    [EmailAddress]
    public string? AzureEmailSenderAddress { get; set; }
    
    /// <summary>
    /// Indica si el email está configurado correctamente
    /// </summary>
    public bool IsEmailConfigured => 
        !string.IsNullOrEmpty(AzureEmailConnectionString) && 
        !string.IsNullOrEmpty(AzureEmailSenderAddress);
    
    /// <summary>
    /// Indica si WhatsApp está configurado correctamente
    /// </summary>
    public bool IsWhatsAppConfigured => 
        WhatsAppEnabled && 
        !string.IsNullOrEmpty(WhatsAppNumber);
    
    // === Información del Negocio ===
    
    /// <summary>
    /// Nombre del negocio
    /// </summary>
    [MaxLength(100)]
    public string BusinessName { get; set; } = "4cima";
    
    /// <summary>
    /// Teléfono de contacto (visible en el sitio)
    /// </summary>
    [MaxLength(20)]
    public string? ContactPhone { get; set; }
    
    /// <summary>
    /// Email de contacto público (visible en el sitio)
    /// </summary>
    [EmailAddress]
    public string? ContactEmail { get; set; }
    
    /// <summary>
    /// Dirección física
    /// </summary>
    [MaxLength(300)]
    public string? Address { get; set; }
    
    // === Redes Sociales ===
    
    [MaxLength(200)]
    public string? FacebookUrl { get; set; }
    
    [MaxLength(200)]
    public string? InstagramUrl { get; set; }
    
    [MaxLength(200)]
    public string? LinkedInUrl { get; set; }
    
    // === Horarios ===
    
    [MaxLength(100)]
    public string? BusinessHoursWeekday { get; set; } = "Lunes a Viernes: 9:00 AM - 6:00 PM";
    
    [MaxLength(100)]
    public string? BusinessHoursSaturday { get; set; } = "Sábado: 10:00 AM - 2:00 PM";
    
    [MaxLength(100)]
    public string? BusinessHoursSunday { get; set; } = "Domingo: Cerrado";
}

/// <summary>
/// DTO para actualizar solo la configuración de email
/// </summary>
public class UpdateEmailSettingsDto
{
    [Required]
    [EmailAddress]
    public string AdminNotificationEmail { get; set; } = "";
    
    public string? AzureEmailConnectionString { get; set; }
    
    [EmailAddress]
    public string? AzureEmailSenderAddress { get; set; }
}

/// <summary>
/// DTO para actualizar solo la configuración de WhatsApp
/// </summary>
public class UpdateWhatsAppSettingsDto
{
    public bool Enabled { get; set; }
    
    [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Formato inválido. Use solo números, 10-15 dígitos")]
    public string? PhoneNumber { get; set; }
    
    [MaxLength(500)]
    public string? DefaultMessage { get; set; } = "Hola, me interesa obtener más información sobre sus propiedades.";
}

/// <summary>
/// DTO para actualizar información del negocio
/// </summary>
public class UpdateBusinessInfoDto
{
    [MaxLength(100)]
    public string BusinessName { get; set; } = "4cima";
    
    [MaxLength(20)]
    public string? ContactPhone { get; set; }
    
    [EmailAddress]
    public string? ContactEmail { get; set; }
    
    [MaxLength(300)]
    public string? Address { get; set; }
    
    [MaxLength(200)]
    public string? FacebookUrl { get; set; }
    
    [MaxLength(200)]
    public string? InstagramUrl { get; set; }
    
    [MaxLength(200)]
    public string? LinkedInUrl { get; set; }
    
    [MaxLength(100)]
    public string? BusinessHoursWeekday { get; set; }
    
    [MaxLength(100)]
    public string? BusinessHoursSaturday { get; set; }
    
    [MaxLength(100)]
    public string? BusinessHoursSunday { get; set; }
}
