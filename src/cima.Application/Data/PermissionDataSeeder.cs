using System;
using System.Threading.Tasks;
using cima.Permissions;
using Microsoft.Extensions.Logging;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.MultiTenancy;

namespace cima.Application.Data;

/// <summary>
/// Seeder de permisos para asignar automáticamente permisos a roles
/// </summary>
public class PermissionDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IPermissionManager _permissionManager;
    private readonly ILogger<PermissionDataSeeder> _logger;
    private readonly ICurrentTenant _currentTenant;

    public PermissionDataSeeder(
        IPermissionManager permissionManager,
        ILogger<PermissionDataSeeder> logger,
        ICurrentTenant currentTenant)
    {
        _permissionManager = permissionManager;
        _logger = logger;
        _currentTenant = currentTenant;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        _logger.LogInformation("========================================");
        _logger.LogInformation("[PERMISSION SEEDER] Iniciando asignación de permisos...");
        _logger.LogInformation($"[PERMISSION SEEDER] Entorno: {environment}");
        _logger.LogInformation("========================================");

        try
        {
            // Asignar permisos al rol admin
            await SeedAdminPermissionsAsync();

            _logger.LogInformation("========================================");
            _logger.LogInformation("[PERMISSION SEEDER] Permisos asignados exitosamente");
            _logger.LogInformation("========================================");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PERMISSION SEEDER] Error asignando permisos");
            throw;
        }
    }

    private async Task SeedAdminPermissionsAsync()
    {
        _logger.LogInformation("[PERMISSION SEEDER] Asignando permisos al rol 'admin'...");

        // Usar PermissionValueProvider para el rol
        const string roleProviderName = "R"; // "R" es el provider para Roles en ABP
        const string adminRoleName = "admin";

        // Asignar todos los permisos de Listings
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Listings.Default);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Listings.Create);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Listings.Edit);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Listings.Delete);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Listings.Publish);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Listings.Archive);

        // Asignar todos los permisos de Architects
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Architects.Default);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Architects.Create);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Architects.Edit);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Architects.Delete);

        // Asignar todos los permisos de ContactRequests
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.ContactRequests.Default);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.ContactRequests.View);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.ContactRequests.Reply);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.ContactRequests.Close);

        // Asignar permisos de Dashboard
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Dashboard.Default);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Dashboard.ViewStats);

        // Asignar permisos de Settings
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Settings.Default);
        await GrantPermissionAsync(roleProviderName, adminRoleName, cimaPermissions.Settings.Manage);

        // Identity (Roles/Users)
        await GrantPermissionAsync(roleProviderName, adminRoleName, IdentityPermissions.Roles.Default);
        await GrantPermissionAsync(roleProviderName, adminRoleName, IdentityPermissions.Roles.Create);
        await GrantPermissionAsync(roleProviderName, adminRoleName, IdentityPermissions.Roles.Update);
        await GrantPermissionAsync(roleProviderName, adminRoleName, IdentityPermissions.Roles.Delete);
        await GrantPermissionAsync(roleProviderName, adminRoleName, IdentityPermissions.Roles.ManagePermissions);
        await GrantPermissionAsync(roleProviderName, adminRoleName, IdentityPermissions.Users.Default);
        await GrantPermissionAsync(roleProviderName, adminRoleName, IdentityPermissions.Users.Create);
        await GrantPermissionAsync(roleProviderName, adminRoleName, IdentityPermissions.Users.Update);
        await GrantPermissionAsync(roleProviderName, adminRoleName, IdentityPermissions.Users.Delete);
        await GrantPermissionAsync(roleProviderName, adminRoleName, IdentityPermissions.Users.ManageRoles);

        _logger.LogInformation("[PERMISSION SEEDER] Permisos del rol 'admin' asignados correctamente");
    }

    private async Task GrantPermissionAsync(string providerName, string providerKey, string permissionName)
    {
        try
        {
            // Verificar si el permiso ya está otorgado
            var existingPermission = await _permissionManager.GetAsync(permissionName, providerName, providerKey);
            
            if (existingPermission?.IsGranted == true)
            {
                _logger.LogDebug($"[PERMISSION SEEDER] Permiso '{permissionName}' ya otorgado a '{providerKey}'");
                return;
            }

            // Otorgar el permiso
            await _permissionManager.SetAsync(permissionName, providerName, providerKey, true);
            _logger.LogInformation($"[PERMISSION SEEDER] ? Permiso '{permissionName}' otorgado a '{providerKey}'");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"[PERMISSION SEEDER] Error otorgando permiso '{permissionName}' a '{providerKey}': {ex.Message}");
        }
    }
}


