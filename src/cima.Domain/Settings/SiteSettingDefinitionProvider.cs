using Volo.Abp.Settings;

namespace cima.Settings;

/// <summary>
/// Definicion de settings del sitio para ABP.
/// </summary>
public class SiteSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                SiteSettingNames.EmailProvider,
                defaultValue: null,
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.AdminNotificationEmail,
                defaultValue: string.Empty,
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.AzureEmailConnectionString,
                defaultValue: string.Empty,
                isVisibleToClients: false,
                isEncrypted: true
            ),
            new SettingDefinition(
                SiteSettingNames.AzureEmailSenderAddress,
                defaultValue: string.Empty,
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.SmtpHost,
                defaultValue: string.Empty,
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.SmtpPort,
                defaultValue: "587",
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.SmtpUserName,
                defaultValue: string.Empty,
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.SmtpPassword,
                defaultValue: string.Empty,
                isVisibleToClients: false,
                isEncrypted: true
            ),
            new SettingDefinition(
                SiteSettingNames.SmtpFromAddress,
                defaultValue: string.Empty,
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.SmtpFromName,
                defaultValue: "4cima",
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.SmtpEnableSsl,
                defaultValue: "true",
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.BrevoApiKey,
                defaultValue: string.Empty,
                isVisibleToClients: false,
                isEncrypted: true
            ),
            new SettingDefinition(
                SiteSettingNames.BrevoSenderEmail,
                defaultValue: string.Empty,
                isVisibleToClients: false,
                isEncrypted: false
            ),
            new SettingDefinition(
                SiteSettingNames.BrevoSenderName,
                defaultValue: "4cima",
                isVisibleToClients: false,
                isEncrypted: false
            )
        );

        context.Add(
            new SettingDefinition(
                SiteSettingNames.WhatsAppEnabled,
                defaultValue: "false",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.WhatsAppNumber,
                defaultValue: string.Empty,
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.WhatsAppDefaultMessage,
                defaultValue: "Hola, me interesa obtener mas informacion sobre sus propiedades.",
                isVisibleToClients: true
            )
        );

        context.Add(
            new SettingDefinition(
                SiteSettingNames.BusinessName,
                defaultValue: "4cima",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.ContactPhone,
                defaultValue: string.Empty,
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.ContactEmail,
                defaultValue: string.Empty,
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.Address,
                defaultValue: string.Empty,
                isVisibleToClients: true
            )
        );

        context.Add(
            new SettingDefinition(
                SiteSettingNames.FacebookUrl,
                defaultValue: string.Empty,
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.InstagramUrl,
                defaultValue: string.Empty,
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.LinkedInUrl,
                defaultValue: string.Empty,
                isVisibleToClients: true
            )
        );

        context.Add(
            new SettingDefinition(
                SiteSettingNames.BusinessHoursWeekday,
                defaultValue: "Lunes a Viernes: 9:00 AM - 6:00 PM",
                isVisibleToClients: true
            ),
            new SettingDefinition(
                SiteSettingNames.BusinessHoursSaturday,
                defaultValue: "Sabado: 10:00 AM - 2:00 PM",
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
