using Microsoft.Extensions.Localization;
using cima.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace cima.Blazor;

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
