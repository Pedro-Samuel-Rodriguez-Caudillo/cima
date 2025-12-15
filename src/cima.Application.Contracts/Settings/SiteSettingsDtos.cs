using System;
using System.ComponentModel.DataAnnotations;

namespace cima.Settings;

public static class EmailProviderNames
{
    public const string AzureCommunicationServices = "AzureCommunicationServices";
    public const string Smtp = "Smtp";
    public const string Brevo = "Brevo";
}

public class SiteSettingsDto
{
    // Email Settings
    public string? AdminNotificationEmail { get; set; }
    public string? EmailProvider { get; set; }
    
    public string? AzureEmailConnectionString { get; set; }
    public string? AzureEmailSenderAddress { get; set; }
    
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUserName { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpFromAddress { get; set; }
    public string? SmtpFromName { get; set; }
    public bool SmtpEnableSsl { get; set; }
    
    public string? BrevoApiKey { get; set; }
    public string? BrevoSenderEmail { get; set; }
    public string? BrevoSenderName { get; set; }
    
    public bool IsEmailConfigured { get; set; }

    // WhatsApp Settings
    public bool WhatsAppEnabled { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? WhatsAppDefaultMessage { get; set; }
    public bool IsWhatsAppConfigured { get; set; }

    // Business Info
    public string? BusinessName { get; set; } = "4cima";
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Address { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? BusinessHoursWeekday { get; set; }
    public string? BusinessHoursSaturday { get; set; }
    public string? BusinessHoursSunday { get; set; }
}

public class UpdateEmailSettingsDto
{
    [Required(ErrorMessage = "El email de notificaciones es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string AdminNotificationEmail { get; set; } = string.Empty;
    
    [Required]
    public string EmailProvider { get; set; } = EmailProviderNames.AzureCommunicationServices;
    
    public string? AzureEmailConnectionString { get; set; }
    
    [EmailAddress]
    public string? AzureEmailSenderAddress { get; set; }
    
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUserName { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpFromAddress { get; set; }
    public string? SmtpFromName { get; set; }
    public bool SmtpEnableSsl { get; set; }
    
    public string? BrevoApiKey { get; set; }
    public string? BrevoSenderEmail { get; set; }
    public string? BrevoSenderName { get; set; }
}

public class UpdateWhatsAppSettingsDto
{
    public bool Enabled { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    [MaxLength(500)]
    public string? DefaultMessage { get; set; }
}

public class UpdateBusinessInfoDto
{
    [Required(ErrorMessage = "El nombre del negocio es requerido")]
    [MaxLength(100)]
    public string BusinessName { get; set; } = "4cima";
    
    [EmailAddress]
    [MaxLength(100)]
    public string? ContactEmail { get; set; }
    
    [MaxLength(20)]
    public string? ContactPhone { get; set; }
    
    [MaxLength(200)]
    public string? Address { get; set; }
    
    [MaxLength(200)]
    [Url]
    public string? FacebookUrl { get; set; }
    
    [MaxLength(200)]
    [Url]
    public string? InstagramUrl { get; set; }
    
    [MaxLength(200)]
    [Url]
    public string? LinkedInUrl { get; set; }
    
    [MaxLength(100)]
    public string? BusinessHoursWeekday { get; set; }
    
    [MaxLength(100)]
    public string? BusinessHoursSaturday { get; set; }
    
    [MaxLength(100)]
    public string? BusinessHoursSunday { get; set; }
}
