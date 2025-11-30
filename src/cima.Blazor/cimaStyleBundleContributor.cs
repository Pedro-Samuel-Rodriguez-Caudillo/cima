using Volo.Abp.AspNetCore.Mvc.UI.Bundling;

namespace cima.Blazor;

public class cimaStyleBundleContributor : BundleContributor
{
    public override void ConfigureBundle(BundleConfigurationContext context)
    {
        // El CSS de Tailwind se carga directamente desde /css/app.min.css en App.razor
        // No es necesario agregarlo al bundle aquí
    }
}
