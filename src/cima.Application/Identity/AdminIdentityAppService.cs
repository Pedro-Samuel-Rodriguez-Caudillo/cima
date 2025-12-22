using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Identity;

namespace cima.Identity;

public class AdminIdentityAppService : cimaAppService, IAdminIdentityAppService
{
    private const string MustChangePasswordClaim = "MustChangePassword";

    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;

    public AdminIdentityAppService(
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    [Authorize(IdentityPermissions.Users.Update)]
    public async Task<string> ResetPasswordAsync(Guid id, ResetIdentityUserPasswordDto input, CancellationToken cancellationToken = default)
    {
        if (!CurrentUser.IsInRole("admin"))
        {
            throw new UserFriendlyException("Solo administradores pueden restablecer contraseñas");
        }

        var user = await _userRepository.FindAsync(id, cancellationToken: cancellationToken);
        if (user == null)
        {
            throw new UserFriendlyException("Usuario no encontrado");
        }

        var newPassword = string.IsNullOrWhiteSpace(input.NewTemporaryPassword)
            ? GenerateTemporaryPassword()
            : input.NewTemporaryPassword!.Trim();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new UserFriendlyException($"Error al restablecer contraseña: {errors}");
        }

        var existingClaims = await _userManager.GetClaimsAsync(user);
        var mustChangeClaim = existingClaims.FirstOrDefault(c => c.Type == MustChangePasswordClaim);
        if (mustChangeClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, mustChangeClaim);
        }

        await _userManager.AddClaimAsync(user, new Claim(MustChangePasswordClaim, "true"));
        return newPassword;
    }

    private static string GenerateTemporaryPassword()
    {
        var random = new Random();
        var digits = random.Next(1000, 9999);
        return $"Cima{digits}!";
    }
}
