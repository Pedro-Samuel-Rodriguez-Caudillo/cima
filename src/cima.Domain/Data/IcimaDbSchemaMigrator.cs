using System.Threading.Tasks;

namespace cima.Data;

public interface IcimaDbSchemaMigrator
{
    Task MigrateAsync();
}
