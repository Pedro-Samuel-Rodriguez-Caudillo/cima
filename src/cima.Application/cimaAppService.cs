using cima.Localization;
using Volo.Abp.Application.Services;

namespace cima;

/* Inherit your application services from this class.
 */
public abstract class cimaAppService : ApplicationService
{
    protected cimaAppService()
    {
        LocalizationResource = typeof(cimaResource);
    }
}
