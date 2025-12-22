using System;
using System.Linq;
using System.Threading.Tasks;
using cima.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Guids;
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
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IGuidGenerator _guidGenerator;

    public cimaPermissionsDataSeeder(
        IPermissionDataSeeder permissionDataSeeder,
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        IGuidGenerator guidGenerator)
    {
        _permissionDataSeeder = permissionDataSeeder;
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await EnsureRoleExistsAsync("admin");
        await EnsureRoleExistsAsync("architect");

        // Asignar TODOS los permisos de CIMA al rol "admin"
        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            "admin",
            new[]
            {
                // Identity (Roles/Users)
                IdentityPermissions.Roles.Default,
                IdentityPermissions.Roles.Create,
                IdentityPermissions.Roles.Update,
                IdentityPermissions.Roles.Delete,
                IdentityPermissions.Roles.ManagePermissions,
                IdentityPermissions.Users.Default,
                IdentityPermissions.Users.Create,
                IdentityPermissions.Users.Update,
                IdentityPermissions.Users.Delete,
                IdentityPermissions.Users.ManageRoles,


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

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        var normalizedName = roleName.ToUpperInvariant();
        var existingRole = await _roleRepository.FindByNormalizedNameAsync(normalizedName);
        if (existingRole != null)
        {
            return;
        }

        var role = new IdentityRole(_guidGenerator.Create(), roleName);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(error => error.Description));
            throw new AbpException($"No se pudo crear el rol '{roleName}': {errors}");
        }
    }

}
