using System.Threading.Tasks;
using cima.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Authorization.Permissions;

namespace cima.Data;

/// <summary>
/// Seeder que asigna todos los permisos de CIMA al rol "admin" automáticamente
/// Se ejecuta cuando se inicializa la base de datos
/// </summary>
public class cimaPermissionsDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IPermissionDataSeeder _permissionDataSeeder;

    public cimaPermissionsDataSeeder(IPermissionDataSeeder permissionDataSeeder)
    {
        _permissionDataSeeder = permissionDataSeeder;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Asignar TODOS los permisos de CIMA al rol "admin"
        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            "admin",
            new[]
            {
                // Listings (Propiedades)
                cimaPermissions.Listings.Default,
                cimaPermissions.Listings.Create,
                cimaPermissions.Listings.Edit,
                cimaPermissions.Listings.Delete,
                cimaPermissions.Listings.Publish,
                cimaPermissions.Listings.Archive,
                
                // Architects (Arquitectos)
                cimaPermissions.Architects.Default,
                cimaPermissions.Architects.Create,
                cimaPermissions.Architects.Edit,
                cimaPermissions.Architects.Delete,
                
                // ContactRequests (Solicitudes de contacto)
                cimaPermissions.ContactRequests.Default,
                cimaPermissions.ContactRequests.View,
                cimaPermissions.ContactRequests.Manage,
                cimaPermissions.ContactRequests.Reply,
                cimaPermissions.ContactRequests.Close
            }
        );
    }
}
