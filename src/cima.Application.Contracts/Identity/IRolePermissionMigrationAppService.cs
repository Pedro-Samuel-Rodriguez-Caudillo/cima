using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace cima.Identity;

public interface IRolePermissionMigrationAppService : IApplicationService
{
    Task MigrateAsync(RolePermissionMigrationInput input);
}
