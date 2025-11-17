using cima.Localization;
using Volo.Abp.AspNetCore.Components;

namespace cima.Blazor.Client;

public abstract class cimaComponentBase : AbpComponentBase
{
    protected cimaComponentBase()
    {
        LocalizationResource = typeof(cimaResource);
    }
}
