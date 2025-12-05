using System;
using System.Net;
using System.Threading.Tasks;
using cima.Notifications;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace cima.Settings;

/// <summary>
/// Servicio para gestionar configuración del sitio
/// </summary>
public class SiteSettingsAppService : ApplicationService, ISiteSettingsAppService
{
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;
    private readonly IEmailNotificationService _emailService;

    public SiteSettingsAppService(
        ISettingProvider settingProvider,
        ISettingManager settingManager,
        IEmailNotificationService emailService)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
        _emailService = emailService;
    }

    /// <summary>
    /// Obtiene toda la configuración (admin only)
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task<SiteSettingsDto> GetAsync()
    {
        return new SiteSettingsDto
        {
            // Email
            AdminNotificationEmail = await _settingProvider.GetOrNullAsync(SiteSettingNames.AdminNotificationEmail),
            AzureEmailConnectionString = await _settingProvider.GetOrNullAsync(SiteSettingNames.AzureEmailConnectionString),
            AzureEmailSenderAddress = await _settingProvider.GetOrNullAsync(SiteSettingNames.AzureEmailSenderAddress),
            
            // WhatsApp
            WhatsAppEnabled = await GetBoolSettingAsync(SiteSettingNames.WhatsAppEnabled),
            WhatsAppNumber = await _settingProvider.GetOrNullAsync(SiteSettingNames.WhatsAppNumber),
            WhatsAppDefaultMessage = await _settingProvider.GetOrNullAsync(SiteSettingNames.WhatsAppDefaultMessage),
            
            // Business Info
            BusinessName = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessName) ?? "4cima",
            ContactPhone = await _settingProvider.GetOrNullAsync(SiteSettingNames.ContactPhone),
            ContactEmail = await _settingProvider.GetOrNullAsync(SiteSettingNames.ContactEmail),
            Address = await _settingProvider.GetOrNullAsync(SiteSettingNames.Address),
            
            // Social Media
            FacebookUrl = await _settingProvider.GetOrNullAsync(SiteSettingNames.FacebookUrl),
            InstagramUrl = await _settingProvider.GetOrNullAsync(SiteSettingNames.InstagramUrl),
            LinkedInUrl = await _settingProvider.GetOrNullAsync(SiteSettingNames.LinkedInUrl),
            
            // Business Hours
            BusinessHoursWeekday = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessHoursWeekday),
            BusinessHoursSaturday = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessHoursSaturday),
            BusinessHoursSunday = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessHoursSunday)
        };
    }

    /// <summary>
    /// Obtiene configuración pública (sin datos sensibles)
    /// </summary>
    [AllowAnonymous]
    public async Task<PublicSiteSettingsDto> GetPublicAsync()
    {
        var whatsAppEnabled = await GetBoolSettingAsync(SiteSettingNames.WhatsAppEnabled);
        var whatsAppNumber = await _settingProvider.GetOrNullAsync(SiteSettingNames.WhatsAppNumber);
        var whatsAppMessage = await _settingProvider.GetOrNullAsync(SiteSettingNames.WhatsAppDefaultMessage);
        
        string? whatsAppUrl = null;
        if (whatsAppEnabled && !string.IsNullOrEmpty(whatsAppNumber))
        {
            var encodedMessage = string.IsNullOrEmpty(whatsAppMessage) 
                ? "" 
                : $"?text={WebUtility.UrlEncode(whatsAppMessage)}";
            whatsAppUrl = $"https://wa.me/{whatsAppNumber}{encodedMessage}";
        }

        return new PublicSiteSettingsDto
        {
            BusinessName = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessName) ?? "4cima",
            ContactPhone = await _settingProvider.GetOrNullAsync(SiteSettingNames.ContactPhone),
            ContactEmail = await _settingProvider.GetOrNullAsync(SiteSettingNames.ContactEmail),
            Address = await _settingProvider.GetOrNullAsync(SiteSettingNames.Address),
            
            WhatsAppEnabled = whatsAppEnabled && !string.IsNullOrEmpty(whatsAppNumber),
            WhatsAppUrl = whatsAppUrl,
            
            FacebookUrl = await _settingProvider.GetOrNullAsync(SiteSettingNames.FacebookUrl),
            InstagramUrl = await _settingProvider.GetOrNullAsync(SiteSettingNames.InstagramUrl),
            LinkedInUrl = await _settingProvider.GetOrNullAsync(SiteSettingNames.LinkedInUrl),
            
            BusinessHoursWeekday = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessHoursWeekday),
            BusinessHoursSaturday = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessHoursSaturday),
            BusinessHoursSunday = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessHoursSunday)
        };
    }

    /// <summary>
    /// Actualiza configuración de email
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task UpdateEmailSettingsAsync(UpdateEmailSettingsDto input)
    {
        await _settingManager.SetGlobalAsync(SiteSettingNames.AdminNotificationEmail, input.AdminNotificationEmail);
        
        if (!string.IsNullOrEmpty(input.AzureEmailConnectionString))
        {
            await _settingManager.SetGlobalAsync(SiteSettingNames.AzureEmailConnectionString, input.AzureEmailConnectionString);
        }
        
        if (!string.IsNullOrEmpty(input.AzureEmailSenderAddress))
        {
            await _settingManager.SetGlobalAsync(SiteSettingNames.AzureEmailSenderAddress, input.AzureEmailSenderAddress);
        }
    }

    /// <summary>
    /// Actualiza configuración de WhatsApp
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task UpdateWhatsAppSettingsAsync(UpdateWhatsAppSettingsDto input)
    {
        await _settingManager.SetGlobalAsync(SiteSettingNames.WhatsAppEnabled, input.Enabled.ToString().ToLower());
        await _settingManager.SetGlobalAsync(SiteSettingNames.WhatsAppNumber, input.PhoneNumber ?? "");
        await _settingManager.SetGlobalAsync(SiteSettingNames.WhatsAppDefaultMessage, input.DefaultMessage ?? "");
    }

    /// <summary>
    /// Actualiza información del negocio
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task UpdateBusinessInfoAsync(UpdateBusinessInfoDto input)
    {
        await _settingManager.SetGlobalAsync(SiteSettingNames.BusinessName, input.BusinessName);
        await _settingManager.SetGlobalAsync(SiteSettingNames.ContactPhone, input.ContactPhone ?? "");
        await _settingManager.SetGlobalAsync(SiteSettingNames.ContactEmail, input.ContactEmail ?? "");
        await _settingManager.SetGlobalAsync(SiteSettingNames.Address, input.Address ?? "");
        
        await _settingManager.SetGlobalAsync(SiteSettingNames.FacebookUrl, input.FacebookUrl ?? "");
        await _settingManager.SetGlobalAsync(SiteSettingNames.InstagramUrl, input.InstagramUrl ?? "");
        await _settingManager.SetGlobalAsync(SiteSettingNames.LinkedInUrl, input.LinkedInUrl ?? "");
        
        await _settingManager.SetGlobalAsync(SiteSettingNames.BusinessHoursWeekday, input.BusinessHoursWeekday ?? "");
        await _settingManager.SetGlobalAsync(SiteSettingNames.BusinessHoursSaturday, input.BusinessHoursSaturday ?? "");
        await _settingManager.SetGlobalAsync(SiteSettingNames.BusinessHoursSunday, input.BusinessHoursSunday ?? "");
    }

    /// <summary>
    /// Prueba el envío de email
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task<bool> TestEmailAsync(string testRecipient)
    {
        try
        {
            await _emailService.SendContactRequestConfirmationAsync(new ContactRequestConfirmationDto
            {
                CustomerEmail = testRecipient,
                CustomerName = "Test",
                PropertyTitle = "Email de prueba - Configuración 4cima"
            });
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al enviar email de prueba a {Recipient}", testRecipient);
            return false;
        }
    }

    private async Task<bool> GetBoolSettingAsync(string name)
    {
        var value = await _settingProvider.GetOrNullAsync(name);
        return bool.TryParse(value, out var result) && result;
    }
}
