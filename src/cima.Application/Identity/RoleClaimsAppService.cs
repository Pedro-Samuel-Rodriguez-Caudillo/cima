using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace cima.Identity;

[Authorize(IdentityPermissions.Roles.ManagePermissions)]
public class RoleClaimsAppService : ApplicationService, IRoleClaimsAppService
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;

    public RoleClaimsAppService(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
    }

    public async Task<ListResultDto<RoleClaimDto>> GetListAsync(Guid roleId)
    {
        var role = await _roleRepository.GetAsync(roleId);
        var claims = await _roleManager.GetClaimsAsync(role);
        var dtos = claims
            .Select(claim => new RoleClaimDto
            {
                Type = claim.Type ?? string.Empty,
                Value = claim.Value ?? string.Empty
            })
            .ToList();

        return new ListResultDto<RoleClaimDto>(dtos);
    }

    public async Task UpdateAsync(Guid roleId, UpdateRoleClaimsInput input)
    {
        var role = await _roleRepository.GetAsync(roleId);
        var existingClaims = await _roleManager.GetClaimsAsync(role);
        var desiredClaims = NormalizeClaims(input.Claims);

        var existingKeys = new HashSet<string>(
            existingClaims.Select(BuildClaimKey),
            StringComparer.OrdinalIgnoreCase);
        var desiredKeys = new HashSet<string>(
            desiredClaims.Select(BuildClaimKey),
            StringComparer.OrdinalIgnoreCase);

        foreach (var claim in existingClaims)
        {
            if (!desiredKeys.Contains(BuildClaimKey(claim)))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }
        }

        foreach (var claim in desiredClaims)
        {
            if (!existingKeys.Contains(BuildClaimKey(claim)))
            {
                await _roleManager.AddClaimAsync(role, claim);
            }
        }

        await _roleRepository.UpdateAsync(role, autoSave: true);
    }

    private static List<Claim> NormalizeClaims(IEnumerable<RoleClaimDto> claims)
    {
        return claims
            .Where(claim => !string.IsNullOrWhiteSpace(claim.Type))
            .Where(claim => !string.IsNullOrWhiteSpace(claim.Value))
            .Select(claim => new Claim(claim.Type.Trim(), claim.Value.Trim()))
            .ToList();
    }

    private static string BuildClaimKey(Claim claim)
    {
        return $"{claim.Type}::{claim.Value}";
    }
}
