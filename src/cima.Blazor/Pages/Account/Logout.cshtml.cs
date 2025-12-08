using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Identity;

namespace cima.Blazor.Pages.Account;

public class LogoutModel : Volo.Abp.Account.Web.Pages.Account.LogoutModel
{
    private readonly SignInManager<Volo.Abp.Identity.IdentityUser> _signInManager;

    [BindProperty(SupportsGet = true)]
    public bool LoggedOut { get; set; }

    public string? UserName { get; set; }
    public string? UserEmail { get; set; }

    public LogoutModel(SignInManager<Volo.Abp.Identity.IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public override Task<IActionResult> OnGetAsync()
    {
        // Obtener información del usuario actual antes de mostrar confirmación
        if (User.Identity?.IsAuthenticated == true)
        {
            UserName = User.Identity.Name;
            UserEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        }

        return Task.FromResult<IActionResult>(Page());
    }

    public override async Task<IActionResult> OnPostAsync()
    {
        await _signInManager.SignOutAsync();
        
        LoggedOut = true;
        return Page();
    }
}
