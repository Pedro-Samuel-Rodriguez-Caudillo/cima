using Volo.Abp.TextTemplating;

namespace cima.TextTemplates;

/// <summary>
/// Proveedor de definiciones de templates de texto para ABP.
/// </summary>
public class CimaTextTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition(CimaTextTemplateDefinitions.ContactRequestNotification)
                .WithVirtualFilePath("/TextTemplates/ContactRequestNotification.tpl", isInlineLocalized: true)
        );

        context.Add(
            new TemplateDefinition(CimaTextTemplateDefinitions.ContactRequestConfirmation)
                .WithVirtualFilePath("/TextTemplates/ContactRequestConfirmation.tpl", isInlineLocalized: true)
        );

        context.Add(
            new TemplateDefinition(CimaTextTemplateDefinitions.ListingPublishedNotification)
                .WithVirtualFilePath("/TextTemplates/ListingPublishedNotification.tpl", isInlineLocalized: true)
        );

        context.Add(
            new TemplateDefinition(CimaTextTemplateDefinitions.WelcomeArchitect)
                .WithVirtualFilePath("/TextTemplates/WelcomeArchitect.tpl", isInlineLocalized: true)
        );
    }
}
