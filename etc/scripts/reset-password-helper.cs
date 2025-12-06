using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Identity;

namespace cima.DbMigrator;

public static class PasswordResetHelper
{
    public static async Task ResetPasswordAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<IdentityUserManager>();
        var userRepository = serviceProvider.GetRequiredService<IIdentityUserRepository>();
        
        // Reset admin@cima.com password
        var user = await userRepository.FindByNormalizedUserNameAsync("ADMIN@CIMA.COM");
        if (user != null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, "1q2w3E*");
            Console.WriteLine($"Reset password for admin@cima.com: {(result.Succeeded ? "SUCCESS" : string.Join(", ", result.Errors))}");
        }
        
        // Also reset admin user
        var adminUser = await userRepository.FindByNormalizedUserNameAsync("ADMIN");
        if (adminUser != null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var result = await userManager.ResetPasswordAsync(adminUser, token, "1q2w3E*");
            Console.WriteLine($"Reset password for admin: {(result.Succeeded ? "SUCCESS" : string.Join(", ", result.Errors))}");
        }
    }
}
