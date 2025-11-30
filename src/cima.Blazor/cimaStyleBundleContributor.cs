using Volo.Abp.AspNetCore.Mvc.UI.Bundling;

namespace cima.Blazor;

public class cimaStyleBundleContributor : BundleContributor
{
    public override void ConfigureBundle(BundleConfigurationContext context)
    {
        // Agregar el CSS compilado de Tailwind desde el proyecto Client
        context.Files.Add("/_content/cima.Blazor.Client/css/app.min.css");
    }
}
