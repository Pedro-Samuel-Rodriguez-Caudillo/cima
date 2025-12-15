using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace cima.Notifications;

/// <summary>
/// Servicio que selecciona dinámicamente el proveedor de correo según settings/configuración.
/// Implementa <see cref="IEmailNotificationService"/> para el resto del sistema.
/// </summary>
public class SwitchingEmailNotificationService : IEmailNotificationService, ITransientDependency
{
    private readonly IReadOnlyDictionary<string, IEmailDeliveryProvider> _providers;
    private readonly ISettingProvider _settingProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SwitchingEmailNotificationService> _logger;

    public SwitchingEmailNotificationService(
        IEnumerable<IEmailDeliveryProvider> providers,
        ISettingProvider settingProvider,
        IConfiguration configuration,
        ILogger<SwitchingEmailNotificationService> logger)
    {
        _providers = providers.ToDictionary(
            p => p.Name.ToLowerInvariant(),
            p => p);
        _settingProvider = settingProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendContactRequestNotificationAsync(ContactRequestNotificationDto notification)
    {
        var provider = await ResolveProviderAsync();
        await provider.SendContactRequestNotificationAsync(notification);
    }

    public async Task SendContactRequestConfirmationAsync(ContactRequestConfirmationDto confirmation)
    {
        var provider = await ResolveProviderAsync();
        await provider.SendContactRequestConfirmationAsync(confirmation);
    }

    public async Task SendArchitectWelcomeEmailAsync(ArchitectWelcomeEmailDto welcome)
    {
        var provider = await ResolveProviderAsync();
        await provider.SendArchitectWelcomeEmailAsync(welcome);
    }

    private async Task<IEmailDeliveryProvider> ResolveProviderAsync()
    {
        var selected = (await _settingProvider.GetOrNullAsync(SiteSettingNames.EmailProvider))
            ?? _configuration["Email:Provider"]
            ?? EmailProviderNames.AzureCommunicationServices;

        var normalized = selected.ToLowerInvariant();
        if (_providers.TryGetValue(normalized, out var provider))
        {
            return provider;
        }

        _logger.LogWarning(
            "Proveedor de email '{Provider}' no configurado. Se utilizará Azure como valor por defecto.",
            selected);

        return _providers.TryGetValue(EmailProviderNames.AzureCommunicationServices, out var fallback)
            ? fallback
            : _providers.Values.First();
    }
}
