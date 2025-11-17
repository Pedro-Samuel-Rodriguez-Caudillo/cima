using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using cima.Localization;

namespace cima.Blazor.Client;

[Dependency(ReplaceServices = true)]
public class cimaBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<cimaResource> _localizer;

    public cimaBrandingProvider(IStringLocalizer<cimaResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
