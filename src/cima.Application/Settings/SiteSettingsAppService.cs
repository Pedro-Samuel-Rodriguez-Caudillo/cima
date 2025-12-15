using System;
using System.Net;
using System.Threading.Tasks;
using cima.Notifications;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;

namespace cima.Settings;

/// <summary>
/// Servicio para gestionar configuracion del sitio.
/// </summary>
public class SiteSettingsAppService : ApplicationService, ISiteSettingsAppService
{
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;
    private readonly IEmailNotificationService _emailService;
    private readonly IConfiguration _configuration;

    public SiteSettingsAppService(
        ISettingProvider settingProvider,
        ISettingManager settingManager,
        IEmailNotificationService emailService,
        IConfiguration configuration)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
        _emailService = emailService;
        _configuration = configuration;
    }

    /// <summary>
    /// Obtiene toda la configuracion (admin only).
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task<SiteSettingsDto> GetAsync()
    {
        var emailProvider = await GetEmailProviderAsync();

        var azureConnection = await GetSettingOrConfigAsync(
            SiteSettingNames.AzureEmailConnectionString,
            "Email:AzureCommunicationServices:ConnectionString");
        var azureSender = await GetSettingOrConfigAsync(
            SiteSettingNames.AzureEmailSenderAddress,
            "Email:AzureCommunicationServices:SenderAddress");

        var smtpHost = await GetSettingOrConfigAsync(SiteSettingNames.SmtpHost, "Email:Smtp:Host");
        var smtpPort = await GetSettingOrConfigAsync(SiteSettingNames.SmtpPort, "Email:Smtp:Port");
        var smtpUser = await GetSettingOrConfigAsync(SiteSettingNames.SmtpUserName, "Email:Smtp:UserName");
        var smtpPassword = await GetSettingOrConfigAsync(SiteSettingNames.SmtpPassword, "Email:Smtp:Password");
        var smtpFrom = await GetSettingOrConfigAsync(SiteSettingNames.SmtpFromAddress, "Email:Smtp:FromAddress");
        var smtpFromName = await GetSettingOrConfigAsync(SiteSettingNames.SmtpFromName, "Email:Smtp:FromName");
        var smtpEnableSsl = await GetSettingOrConfigAsync(SiteSettingNames.SmtpEnableSsl, "Email:Smtp:EnableSsl");

        var brevoApiKey = await GetSettingOrConfigAsync(SiteSettingNames.BrevoApiKey, "Email:Brevo:ApiKey");
        var brevoSenderEmail = await GetSettingOrConfigAsync(SiteSettingNames.BrevoSenderEmail, "Email:Brevo:SenderEmail");
        var brevoSenderName = await GetSettingOrConfigAsync(SiteSettingNames.BrevoSenderName, "Email:Brevo:SenderName");

        var whatsAppEnabled = await GetBoolSettingAsync(SiteSettingNames.WhatsAppEnabled);
        var whatsAppNumber = await _settingProvider.GetOrNullAsync(SiteSettingNames.WhatsAppNumber);

        return new SiteSettingsDto
        {
            AdminNotificationEmail = await _settingProvider.GetOrNullAsync(SiteSettingNames.AdminNotificationEmail),
            EmailProvider = emailProvider,

            AzureEmailConnectionString = azureConnection,
            AzureEmailSenderAddress = azureSender,

            SmtpHost = smtpHost,
            SmtpPort = int.TryParse(smtpPort, out var parsedPort) ? parsedPort : null,
            SmtpUserName = smtpUser,
            SmtpPassword = smtpPassword,
            SmtpFromAddress = smtpFrom,
            SmtpFromName = smtpFromName,
            SmtpEnableSsl = bool.TryParse(smtpEnableSsl, out var parsedSsl) && parsedSsl,

            BrevoApiKey = brevoApiKey,
            BrevoSenderEmail = brevoSenderEmail,
            BrevoSenderName = brevoSenderName,

            IsEmailConfigured = IsEmailConfigured(
                emailProvider,
                azureConnection,
                azureSender,
                smtpHost,
                smtpFrom,
                brevoApiKey,
                brevoSenderEmail),

            WhatsAppEnabled = whatsAppEnabled,
            WhatsAppNumber = whatsAppNumber,
            WhatsAppDefaultMessage = await _settingProvider.GetOrNullAsync(SiteSettingNames.WhatsAppDefaultMessage),
            IsWhatsAppConfigured = whatsAppEnabled && !string.IsNullOrEmpty(whatsAppNumber),

            BusinessName = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessName) ?? "4cima",
            ContactPhone = await _settingProvider.GetOrNullAsync(SiteSettingNames.ContactPhone),
            ContactEmail = await _settingProvider.GetOrNullAsync(SiteSettingNames.ContactEmail),
            Address = await _settingProvider.GetOrNullAsync(SiteSettingNames.Address),

            FacebookUrl = await _settingProvider.GetOrNullAsync(SiteSettingNames.FacebookUrl),
            InstagramUrl = await _settingProvider.GetOrNullAsync(SiteSettingNames.InstagramUrl),
            LinkedInUrl = await _settingProvider.GetOrNullAsync(SiteSettingNames.LinkedInUrl),

            BusinessHoursWeekday = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessHoursWeekday),
            BusinessHoursSaturday = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessHoursSaturday),
            BusinessHoursSunday = await _settingProvider.GetOrNullAsync(SiteSettingNames.BusinessHoursSunday)
        };
    }

    /// <summary>
    /// Obtiene configuracion publica (sin datos sensibles).
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
                ? string.Empty
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
    /// Actualiza configuracion de email.
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task UpdateEmailSettingsAsync(UpdateEmailSettingsDto input)
    {
        await _settingManager.SetGlobalAsync(SiteSettingNames.AdminNotificationEmail, input.AdminNotificationEmail);
        await _settingManager.SetGlobalAsync(SiteSettingNames.EmailProvider, input.EmailProvider);

        if (!string.IsNullOrEmpty(input.AzureEmailConnectionString))
        {
            await _settingManager.SetGlobalAsync(SiteSettingNames.AzureEmailConnectionString, input.AzureEmailConnectionString);
        }

        if (!string.IsNullOrEmpty(input.AzureEmailSenderAddress))
        {
            await _settingManager.SetGlobalAsync(SiteSettingNames.AzureEmailSenderAddress, input.AzureEmailSenderAddress);
        }

        await _settingManager.SetGlobalAsync(SiteSettingNames.SmtpHost, input.SmtpHost ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.SmtpPort, input.SmtpPort?.ToString() ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.SmtpUserName, input.SmtpUserName ?? string.Empty);
        if (!string.IsNullOrEmpty(input.SmtpPassword))
        {
            await _settingManager.SetGlobalAsync(SiteSettingNames.SmtpPassword, input.SmtpPassword);
        }
        await _settingManager.SetGlobalAsync(SiteSettingNames.SmtpFromAddress, input.SmtpFromAddress ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.SmtpFromName, input.SmtpFromName ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.SmtpEnableSsl, input.SmtpEnableSsl.ToString());

        if (!string.IsNullOrEmpty(input.BrevoApiKey))
        {
            await _settingManager.SetGlobalAsync(SiteSettingNames.BrevoApiKey, input.BrevoApiKey);
        }
        await _settingManager.SetGlobalAsync(SiteSettingNames.BrevoSenderEmail, input.BrevoSenderEmail ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.BrevoSenderName, input.BrevoSenderName ?? string.Empty);
    }

    /// <summary>
    /// Actualiza configuracion de WhatsApp.
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task UpdateWhatsAppSettingsAsync(UpdateWhatsAppSettingsDto input)
    {
        await _settingManager.SetGlobalAsync(SiteSettingNames.WhatsAppEnabled, input.Enabled.ToString().ToLower());
        await _settingManager.SetGlobalAsync(SiteSettingNames.WhatsAppNumber, input.PhoneNumber ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.WhatsAppDefaultMessage, input.DefaultMessage ?? string.Empty);
    }

    /// <summary>
    /// Actualiza informacion del negocio.
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task UpdateBusinessInfoAsync(UpdateBusinessInfoDto input)
    {
        await _settingManager.SetGlobalAsync(SiteSettingNames.BusinessName, input.BusinessName);
        await _settingManager.SetGlobalAsync(SiteSettingNames.ContactPhone, input.ContactPhone ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.ContactEmail, input.ContactEmail ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.Address, input.Address ?? string.Empty);

        await _settingManager.SetGlobalAsync(SiteSettingNames.FacebookUrl, input.FacebookUrl ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.InstagramUrl, input.InstagramUrl ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.LinkedInUrl, input.LinkedInUrl ?? string.Empty);

        await _settingManager.SetGlobalAsync(SiteSettingNames.BusinessHoursWeekday, input.BusinessHoursWeekday ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.BusinessHoursSaturday, input.BusinessHoursSaturday ?? string.Empty);
        await _settingManager.SetGlobalAsync(SiteSettingNames.BusinessHoursSunday, input.BusinessHoursSunday ?? string.Empty);
    }

    /// <summary>
    /// Prueba el envio de email.
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
                PropertyTitle = "Email de prueba - Configuracion 4cima"
            });
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al enviar email de prueba a {Recipient}", testRecipient);
            return false;
        }
    }

    private async Task<string> GetEmailProviderAsync()
    {
        return await _settingProvider.GetOrNullAsync(SiteSettingNames.EmailProvider)
            ?? _configuration["Email:Provider"]
            ?? EmailProviderNames.AzureCommunicationServices;
    }

    private async Task<string?> GetSettingOrConfigAsync(string settingName, string configurationKey)
    {
        var settingValue = await _settingProvider.GetOrNullAsync(settingName);
        if (!string.IsNullOrWhiteSpace(settingValue))
        {
            return settingValue;
        }

        return _configuration[configurationKey];
    }

    private static bool IsEmailConfigured(
        string provider,
        string? azureConnection,
        string? azureSender,
        string? smtpHost,
        string? smtpFrom,
        string? brevoApiKey,
        string? brevoSenderEmail)
    {
        return provider.ToLowerInvariant() switch
        {
            EmailProviderNames.AzureCommunicationServices =>
                !string.IsNullOrWhiteSpace(azureConnection) && !string.IsNullOrWhiteSpace(azureSender),
            EmailProviderNames.Smtp =>
                !string.IsNullOrWhiteSpace(smtpHost) && !string.IsNullOrWhiteSpace(smtpFrom),
            EmailProviderNames.Brevo =>
                !string.IsNullOrWhiteSpace(brevoApiKey) && !string.IsNullOrWhiteSpace(brevoSenderEmail),
            _ => false
        };
    }

    private async Task<bool> GetBoolSettingAsync(string name)
    {
        var value = await _settingProvider.GetOrNullAsync(name);
        return bool.TryParse(value, out var result) && result;
    }
}
