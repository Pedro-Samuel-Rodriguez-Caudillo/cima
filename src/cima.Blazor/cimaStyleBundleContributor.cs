using Volo.Abp.AspNetCore.Mvc.UI.Bundling;

namespace cima.Blazor;

public class cimaStyleBundleContributor : BundleContributor
{
    public override void ConfigureBundle(BundleConfigurationContext context)
    {
        context.Files.Add(new BundleFile("/_content/cima.Blazor.Client/css/app.min.css", true));
    }
}
