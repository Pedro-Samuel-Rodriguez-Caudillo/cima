using cima.Localization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Asp.Versioning;

namespace cima.Controllers;

/* Inherit your controllers from this class.
 */
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public abstract class cimaController : AbpControllerBase
{
    protected cimaController()
    {
        LocalizationResource = typeof(cimaResource);
    }
}
