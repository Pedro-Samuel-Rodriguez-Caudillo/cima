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
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<cimaDomainSharedModule>(name);
        }
    }
}
