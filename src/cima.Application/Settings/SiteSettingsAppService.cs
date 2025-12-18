using System;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Communication.Email;
using cima.Notifications;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp;
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
        // Debug config resolution
        var configProvider = _configuration["Email:Provider"];
        Logger.LogInformation("GetAsync - Config Email:Provider: '{ConfigProvider}'", configProvider);
        
        var emailProvider = await GetEmailProviderAsync();
        Logger.LogInformation("GetAsync - Resolved EmailProvider: '{ResolvedProvider}'", emailProvider);

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
    /// <summary>
    /// Prueba el envio de email usando los valores proporcionados (sin guardar).
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task<bool> TestEmailAsync(TestEmailSettingsDto input)
    {
        var subject = "Prueba de configuración - 4cima";
        var body = $"<p>Este es un correo de prueba para verificar la configuración de {input.EmailProvider}.</p><p>Si estás viendo esto, la configuración es correcta.</p>";

        try
        {
            if (string.Equals(input.EmailProvider, EmailProviderNames.AzureCommunicationServices, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(input.AzureEmailConnectionString) || string.IsNullOrWhiteSpace(input.AzureEmailSenderAddress))
                {
                    throw new UserFriendlyException("Faltan datos de configuración de Azure");
                }

                var client = new EmailClient(input.AzureEmailConnectionString);
                var content = new EmailContent(subject) { Html = body };
                var message = new EmailMessage(input.AzureEmailSenderAddress, input.TargetEmail, content);
                
                await client.SendAsync(Azure.WaitUntil.Completed, message);
                return true;
            }
            else if (string.Equals(input.EmailProvider, EmailProviderNames.Smtp, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(input.SmtpHost) || input.SmtpPort == null || string.IsNullOrWhiteSpace(input.SmtpUserName))
                {
                    throw new UserFriendlyException("Faltan datos de configuración SMTP");
                }

                using var client = new SmtpClient(input.SmtpHost, input.SmtpPort.Value)
                {
                    Credentials = new NetworkCredential(input.SmtpUserName, input.SmtpPassword),
                    EnableSsl = input.SmtpEnableSsl
                };

                var from = new MailAddress(input.SmtpFromAddress ?? input.SmtpUserName, input.SmtpFromName ?? "4cima Test");
                var message = new MailMessage(from, new MailAddress(input.TargetEmail))
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
                return true;
            }
            else if (string.Equals(input.EmailProvider, EmailProviderNames.Brevo, StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(input.BrevoApiKey) || string.IsNullOrWhiteSpace(input.BrevoSenderEmail))
                {
                    throw new UserFriendlyException("Faltan datos de configuración de Brevo");
                }

                using var client = new HttpClient();
                // Usar endpoint por defecto si no está en config (hardcoded for test safety as fallback)
                var endpoint = "https://api.brevo.com/v3/smtp/email"; 
                
                var payload = new
                {
                    sender = new { email = input.BrevoSenderEmail, name = input.BrevoSenderName ?? "4cima Test" },
                    to = new[] { new { email = input.TargetEmail, name = "Test User" } },
                    subject,
                    htmlContent = body
                };

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload, new JsonSerializerOptions(JsonSerializerDefaults.Web)), Encoding.UTF8, "application/json")
                };
                request.Headers.Add("api-key", input.BrevoApiKey);

                var response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Logger.LogError("Error probando Brevo: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    return false;
                }
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al probar configuración de email para {Provider}", input.EmailProvider);
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

    /// <summary>
    /// Obtiene contenido legal (Privacy y Terms) - público para renderizar páginas
    /// </summary>
    [AllowAnonymous]
    public async Task<LegalContentDto> GetLegalContentAsync()
    {
        return new LegalContentDto
        {
            PrivacyContent = await _settingProvider.GetOrNullAsync(cimaLegalSettings.PrivacyContent) ?? "",
            TermsContent = await _settingProvider.GetOrNullAsync(cimaLegalSettings.TermsContent) ?? ""
        };
    }

    /// <summary>
    /// Actualiza contenido legal (admin only)
    /// </summary>
    [Authorize(cimaPermissions.Settings.Manage)]
    public async Task UpdateLegalContentAsync(UpdateLegalContentDto input)
    {
        await _settingManager.SetGlobalAsync(cimaLegalSettings.PrivacyContent, input.PrivacyContent);
        await _settingManager.SetGlobalAsync(cimaLegalSettings.TermsContent, input.TermsContent);
        await _settingManager.SetGlobalAsync(cimaLegalSettings.PrivacyLastUpdated, DateTime.UtcNow.ToString("o"));
        await _settingManager.SetGlobalAsync(cimaLegalSettings.TermsLastUpdated, DateTime.UtcNow.ToString("o"));
    }
}
