using Volo.Abp.AspNetCore.Mvc.UI.Bundling;

namespace cima.Blazor;

public class cimaScriptBundleContributor : BundleContributor
{
    public override void ConfigureBundle(BundleConfigurationContext context)
    {
        // Script personalizado de navbar
        context.Files.Add("/js/navbar-scroll.js");
    }
}