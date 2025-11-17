using Volo.Abp.Modularity;

namespace cima;

/* Inherit from this class for your domain layer tests. */
public abstract class cimaDomainTestBase<TStartupModule> : cimaTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
