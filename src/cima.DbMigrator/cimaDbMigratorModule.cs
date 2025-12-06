using cima.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace cima.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(cimaEntityFrameworkCoreModule),
    typeof(cimaApplicationModule)  // Cambiado de ApplicationContractsModule para incluir DevelopmentDataSeeder
)]
public class cimaDbMigratorModule : AbpModule
{
}
