using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace cima.Data;

/* This is used if database provider does't define
 * IcimaDbSchemaMigrator implementation.
 */
public class NullcimaDbSchemaMigrator : IcimaDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
