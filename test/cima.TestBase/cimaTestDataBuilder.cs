using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Identity;
using Microsoft.AspNetCore.Identity;
using Volo.Abp.Guids;

namespace cima;

public class cimaTestDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly ILookupNormalizer _lookupNormalizer;
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;
    private readonly IGuidGenerator _guidGenerator;

    public cimaTestDataSeedContributor(
        ICurrentTenant currentTenant,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        ILookupNormalizer lookupNormalizer,
        IdentityUserManager userManager,
        IdentityRoleManager roleManager,
        IGuidGenerator guidGenerator)
    {
        _currentTenant = currentTenant;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _lookupNormalizer = lookupNormalizer;
        _userManager = userManager;
        _roleManager = roleManager;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            await SeedAdminRoleAndUserAsync();
        }
    }

    private async Task SeedAdminRoleAndUserAsync()
    {
        // Create admin role if it doesn't exist
        var adminRole = await _roleRepository.FindByNormalizedNameAsync(_lookupNormalizer.NormalizeName("admin"));
        if (adminRole == null)
        {
            adminRole = new IdentityRole(
                _guidGenerator.Create(),
                "admin",
                tenantId: null
            )
            {
                IsStatic = true,
                IsPublic = true
            };

            await _roleRepository.InsertAsync(adminRole);
        }

        // Create admin user if it doesn't exist
        var adminUser = await _userRepository.FindByNormalizedUserNameAsync(_lookupNormalizer.NormalizeName("admin"));
        if (adminUser == null)
        {
            adminUser = new IdentityUser(
                _guidGenerator.Create(),
                "admin",
                "admin@abp.io",
                tenantId: null
            )
            {
                Name = "admin"
            };

            await _userManager.CreateAsync(adminUser, "1q2w3E*");
            await _userManager.SetEmailAsync(adminUser, "admin@abp.io");
            await _userManager.AddToRoleAsync(adminUser, "admin");
        }
    }
}
