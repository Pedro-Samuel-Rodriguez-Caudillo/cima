using cima.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace cima.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class cimaController : AbpControllerBase
{
    protected cimaController()
    {
        LocalizationResource = typeof(cimaResource);
    }
}
