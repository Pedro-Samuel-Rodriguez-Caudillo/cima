using cima.Localization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace cima.Controllers;

/* Inherit your controllers from this class.
 */
[IgnoreAntiforgeryToken]
public abstract class cimaController : AbpControllerBase
{
    protected cimaController()
    {
        LocalizationResource = typeof(cimaResource);
    }
}
