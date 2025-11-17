using Volo.Abp.Modularity;

namespace cima;

public abstract class cimaApplicationTestBase<TStartupModule> : cimaTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
