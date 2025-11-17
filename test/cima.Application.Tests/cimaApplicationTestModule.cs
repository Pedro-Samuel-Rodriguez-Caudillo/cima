using Volo.Abp.Modularity;

namespace cima;

[DependsOn(
    typeof(cimaApplicationModule),
    typeof(cimaDomainTestModule)
)]
public class cimaApplicationTestModule : AbpModule
{

}
