using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace cima.Settings;

/// <summary>
/// Servicio para gestionar configuración del sitio (admin only)
/// </summary>
public interface ISiteSettingsAppService : IApplicationService
{
    /// <summary>
    /// Obtiene toda la configuración actual
    /// </summary>
    Task<SiteSettingsDto> GetAsync();
    
    /// <summary>
    /// Obtiene configuración pública (sin datos sensibles como connection strings)
    /// </summary>
    Task<PublicSiteSettingsDto> GetPublicAsync();
    
    /// <summary>
    /// Actualiza configuración de email
    /// </summary>
    Task UpdateEmailSettingsAsync(UpdateEmailSettingsDto input);
    
    /// <summary>
    /// Actualiza configuración de WhatsApp
    /// </summary>
    Task UpdateWhatsAppSettingsAsync(UpdateWhatsAppSettingsDto input);
    
    /// <summary>
    /// Actualiza información del negocio
    /// </summary>
    Task UpdateBusinessInfoAsync(UpdateBusinessInfoDto input);
    
    /// <summary>
    /// Prueba el envío de email con la configuración actual
    /// </summary>
    Task<bool> TestEmailAsync(string testRecipient);
}

/// <summary>
/// Configuración pública del sitio (sin datos sensibles)
/// </summary>
public class PublicSiteSettingsDto
{
    public string BusinessName { get; set; } = "4cima";
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? Address { get; set; }
    
    // WhatsApp
    public bool WhatsAppEnabled { get; set; }
    public string? WhatsAppUrl { get; set; } // URL completa wa.me/...
    
    // Redes sociales
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    
    // Horarios
    public string? BusinessHoursWeekday { get; set; }
    public string? BusinessHoursSaturday { get; set; }
    public string? BusinessHoursSunday { get; set; }
}
