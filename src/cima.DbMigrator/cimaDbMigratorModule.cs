using cima.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace cima.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(cimaEntityFrameworkCoreModule),
    typeof(cimaApplicationContractsModule)
)]
public class cimaDbMigratorModule : AbpModule
{
}
