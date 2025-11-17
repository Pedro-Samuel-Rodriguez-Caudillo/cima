using Volo.Abp.Modularity;

namespace cima;

[DependsOn(
    typeof(cimaDomainModule),
    typeof(cimaTestBaseModule)
)]
public class cimaDomainTestModule : AbpModule
{

}
