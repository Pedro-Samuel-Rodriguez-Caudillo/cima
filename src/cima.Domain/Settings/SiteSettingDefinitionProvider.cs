using Volo.Abp.Settings;

namespace cima.Settings;

/// <summary>
/// Definición de settings del sitio para ABP
/// </summary>
public class SiteSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        // Email Settings
        context.Add(
            new SettingDefinition(
                SiteSettingNames.AdminNotificationEmail,
                defaultValue: "",
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.AzureEmailConnectionString,
                defaultValue: "",
                isVisibleToClients: false,
                isEncrypted: true // Encriptar datos sensibles
            ),
            new SettingDefinition(
                SiteSettingNames.AzureEmailSenderAddress,
                defaultValue: "",
                isVisibleToClients: false,
                isEncrypted: false
            )
        );

        // WhatsApp Settings
        context.Add(
            new SettingDefinition(
                SiteSettingNames.WhatsAppEnabled,
                defaultValue: "false",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.WhatsAppNumber,
                defaultValue: "",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.WhatsAppDefaultMessage,
                defaultValue: "Hola, me interesa obtener más información sobre sus propiedades.",
                isVisibleToClients: true
            )
        );

        // Business Info (visible to clients)
        context.Add(
            new SettingDefinition(
                SiteSettingNames.BusinessName,
                defaultValue: "4cima",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.ContactPhone,
                defaultValue: "",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.ContactEmail,
                defaultValue: "",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.Address,
                defaultValue: "",
                isVisibleToClients: true
            )
        );

        // Social Media (visible to clients)
        context.Add(
            new SettingDefinition(
                SiteSettingNames.FacebookUrl,
                defaultValue: "",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.InstagramUrl,
                defaultValue: "",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.LinkedInUrl,
                defaultValue: "",
                isVisibleToClients: true
            )
        );

        // Business Hours (visible to clients)
        context.Add(
            new SettingDefinition(
                SiteSettingNames.BusinessHoursWeekday,
                defaultValue: "Lunes a Viernes: 9:00 AM - 6:00 PM",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.BusinessHoursSaturday,
                defaultValue: "Sábado: 10:00 AM - 2:00 PM",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.BusinessHoursSunday,
                defaultValue: "Domingo: Cerrado",
                isVisibleToClients: true
            )
        );
    }
}
