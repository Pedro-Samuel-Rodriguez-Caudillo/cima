using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace cima.Identity;

[Authorize(IdentityPermissions.Roles.ManagePermissions)]
public class RolePermissionMigrationAppService : ApplicationService, IRolePermissionMigrationAppService
{
    private readonly IPermissionManager _permissionManager;
    private readonly IRepository<PermissionGrant, Guid> _permissionGrantRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentTenant _currentTenant;

    public RolePermissionMigrationAppService(
        IPermissionManager permissionManager,
        IRepository<PermissionGrant, Guid> permissionGrantRepository,
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant)
    {
        _permissionManager = permissionManager;
        _permissionGrantRepository = permissionGrantRepository;
        _tenantRepository = tenantRepository;
        _currentTenant = currentTenant;
    }

    public async Task MigrateAsync(RolePermissionMigrationInput input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.OldName) || string.IsNullOrWhiteSpace(input.NewName))
        {
            return;
        }

        var oldName = input.OldName.Trim();
        var newName = input.NewName.Trim();
        if (string.Equals(oldName, newName, StringComparison.Ordinal))
        {
            return;
        }

        await MigrateForTenantAsync(null, oldName, newName);
        var tenants = await _tenantRepository.GetListAsync();
        foreach (var tenant in tenants)
        {
            await MigrateForTenantAsync(tenant.Id, oldName, newName);
        }
    }

    private async Task MigrateForTenantAsync(Guid? tenantId, string oldName, string newName)
    {
        using (_currentTenant.Change(tenantId))
        {
            var grants = await _permissionGrantRepository.GetListAsync(grant =>
                grant.ProviderName == RolePermissionValueProvider.ProviderName &&
                grant.ProviderKey == oldName);

            if (grants.Count == 0)
            {
                return;
            }

            var existing = await _permissionGrantRepository.GetListAsync(grant =>
                grant.ProviderName == RolePermissionValueProvider.ProviderName &&
                grant.ProviderKey == newName);
            var existingNames = existing
                .Select(grant => grant.Name)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var grant in grants)
            {
                if (!existingNames.Contains(grant.Name))
                {
                    await _permissionManager.SetAsync(
                        grant.Name,
                        RolePermissionValueProvider.ProviderName,
                        newName,
                        true);
                }

                await _permissionGrantRepository.DeleteAsync(grant, autoSave: true);
            }
        }
    }
}
