using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using cima.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace cima.Notifications;

/// <summary>
/// Proveedor basado en la API HTTP de Brevo (antes Sendinblue).
/// </summary>
public class BrevoEmailNotificationService : IEmailDeliveryProvider, ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingProvider _settingProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BrevoEmailNotificationService> _logger;

    public string Name => EmailProviderNames.Brevo;

    public BrevoEmailNotificationService(
        IHttpClientFactory httpClientFactory,
        ISettingProvider settingProvider,
        IConfiguration configuration,
        ILogger<BrevoEmailNotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingProvider = settingProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendContactRequestNotificationAsync(ContactRequestNotificationDto notification)
    {
        var subject = notification.PropertyTitle != null
            ? $"[4cima] Nueva consulta sobre: {notification.PropertyTitle}"
            : "[4cima] Nueva solicitud de contacto";

        var htmlBody = AzureEmailNotificationService.BuildContactRequestNotificationHtml(notification);
        await SendAsync(notification.AdminEmail, notification.CustomerName ?? notification.AdminEmail, subject, htmlBody);
    }

    public async Task SendContactRequestConfirmationAsync(ContactRequestConfirmationDto confirmation)
    {
        var subject = "[4cima] Hemos recibido tu mensaje";
        var htmlBody = AzureEmailNotificationService.BuildContactRequestConfirmationHtml(confirmation);
        await SendAsync(confirmation.CustomerEmail, confirmation.CustomerName ?? confirmation.CustomerEmail, subject, htmlBody);
    }

    public async Task SendArchitectWelcomeEmailAsync(ArchitectWelcomeEmailDto welcome)
    {
        var subject = "[4cima] Bienvenido - Tu cuenta ha sido creada";
        var htmlBody = AzureEmailNotificationService.BuildArchitectWelcomeHtml(welcome);
        await SendAsync(welcome.ArchitectEmail, welcome.ArchitectName ?? welcome.ArchitectEmail, subject, htmlBody);
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlContent)
    {
        var (apiKey, senderEmail, senderName) = await GetConfigAsync();
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(senderEmail))
        {
            _logger.LogWarning("Brevo no configurado. Email no enviado a {To}: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient(nameof(BrevoEmailNotificationService));
            var endpoint = _configuration["Email:Brevo:Endpoint"] ?? "https://api.brevo.com/v3/smtp/email";

            var payload = new
            {
                sender = new { email = senderEmail, name = senderName ?? senderEmail },
                to = new[] { new { email = toEmail, name = toName } },
                subject,
                htmlContent
            };

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", apiKey);

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo devolvió {StatusCode} al enviar a {To}: {Body}", response.StatusCode, toEmail, content);
            }
            else
            {
                _logger.LogInformation("Email enviado via Brevo a {To}: {Subject}", toEmail, subject);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al usar Brevo para enviar email a {To}: {Subject}", toEmail, subject);
        }
    }

    private async Task<(string ApiKey, string SenderEmail, string? SenderName)> GetConfigAsync()
    {
        var apiKey = await _settingProvider.GetOrNullAsync(SiteSettingNames.BrevoApiKey)
            ?? _configuration["Email:Brevo:ApiKey"];
        var senderEmail = await _settingProvider.GetOrNullAsync(SiteSettingNames.BrevoSenderEmail)
            ?? _configuration["Email:Brevo:SenderEmail"];
        var senderName = await _settingProvider.GetOrNullAsync(SiteSettingNames.BrevoSenderName)
            ?? _configuration["Email:Brevo:SenderName"];

        return (apiKey ?? string.Empty, senderEmail ?? string.Empty, senderName);
    }
}
