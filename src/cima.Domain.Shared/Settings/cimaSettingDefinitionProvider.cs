using System;
using Volo.Abp.Localization;
using Volo.Abp.Settings;

namespace cima.Settings
{
    /// <summary>
    /// Define los settings del sistema CIMA que son configurables
    /// </summary>
    public class cimaSettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            // Grupo de configuraciones de contacto
            context.Add(
                new SettingDefinition(
                    cimaSettings.Contact.AdminEmail,
                    "contacto@cima.com", // Valor por defecto
                    displayName: L("Cima:Contact:AdminEmail"),
                    description: L("Cima:Contact:AdminEmail:Description"),
                    isVisibleToClients: false, // Solo visible en backend
                    isEncrypted: false
                ),
                new SettingDefinition(
                    cimaSettings.Contact.AdminPhone,
                    "+52 55 1234 5678", // Valor por defecto
                    displayName: L("Cima:Contact:AdminPhone"),
                    description: L("Cima:Contact:AdminPhone:Description"),
                    isVisibleToClients: false,
                    isEncrypted: false
                )
            );

            // Grupo de configuraciones de negocio
            context.Add(
                new SettingDefinition(
                    cimaBusinessSettings.MaxImagesPerListing,
                    "12",
                    displayName: L("Cima:Business:MaxImagesPerListing"),
                    description: L("Cima:Business:MaxImagesPerListing:Description"),
                    isVisibleToClients: false
                ),
                new SettingDefinition(
                    cimaBusinessSettings.DraftCleanupDays,
                    "30",
                    displayName: L("Cima:Business:DraftCleanupDays"),
                    description: L("Cima:Business:DraftCleanupDays:Description"),
                    isVisibleToClients: false
                ),
                new SettingDefinition(
                    cimaBusinessSettings.AdminNotificationEmail,
                    "",
                    displayName: L("Cima:Business:AdminNotificationEmail"),
                    description: L("Cima:Business:AdminNotificationEmail:Description"),
                    isVisibleToClients: false
                ),
                new SettingDefinition(
                    cimaBusinessSettings.MaxFeaturedListings,
                    "6",
                    displayName: L("Cima:Business:MaxFeaturedListings"),
                    description: L("Cima:Business:MaxFeaturedListings:Description"),
                    isVisibleToClients: false
                ),
                new SettingDefinition(
                    cimaBusinessSettings.ListingExpirationDays,
                    "0",
                    displayName: L("Cima:Business:ListingExpirationDays"),
                    description: L("Cima:Business:ListingExpirationDays:Description"),
                    isVisibleToClients: false
                )
            );

            // Grupo de configuraciones legales (markdown)
            context.Add(
                new SettingDefinition(
                    cimaLegalSettings.PrivacyContent,
                    "# Política de Privacidad\n\nContenido pendiente de configurar.",
                    displayName: L("Cima:Legal:PrivacyContent"),
                    isVisibleToClients: true // Visible para páginas públicas
                ),
                new SettingDefinition(
                    cimaLegalSettings.TermsContent,
                    "# Términos y Condiciones\n\nContenido pendiente de configurar.",
                    displayName: L("Cima:Legal:TermsContent"),
                    isVisibleToClients: true
                ),
                new SettingDefinition(
                    cimaLegalSettings.PrivacyLastUpdated,
                    DateTime.UtcNow.ToString("o"),
                    isVisibleToClients: true
                ),
                new SettingDefinition(
                    cimaLegalSettings.TermsLastUpdated,
                    DateTime.UtcNow.ToString("o"),
                    isVisibleToClients: true
                )
            );
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<cimaDomainSharedModule>(name);
        }
    }
}
