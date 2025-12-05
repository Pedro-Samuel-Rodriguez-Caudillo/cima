using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Volo.Abp.Account.Web;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Identity;

namespace cima.Blazor.Pages.Account;

public class LoginModel : Volo.Abp.Account.Web.Pages.Account.LoginModel
{
    public LoginModel(
        IAuthenticationSchemeProvider schemeProvider,
        IOptions<AbpAccountOptions> accountOptions,
        IOptions<IdentityOptions> identityOptions,
        IdentityDynamicClaimsPrincipalContributorCache identityDynamicClaimsPrincipalContributorCache,
        IWebHostEnvironment webHostEnvironment)
        : base(schemeProvider, accountOptions, identityOptions, identityDynamicClaimsPrincipalContributorCache, webHostEnvironment)
    {
    }

    public override async Task<IActionResult> OnGetAsync()
    {
        // Si ya está autenticado, redirigir
        if (CurrentUser.IsAuthenticated)
        {
            return LocalRedirect("/account/post-login");
        }
        
        // Inicializar LoginInput para evitar null
        LoginInput = new LoginInputModel();
        
        return await base.OnGetAsync();
    }

    public override async Task<IActionResult> OnPostAsync(string action)
    {
        try
        {
            // Validar que los campos requeridos estén presentes
            if (string.IsNullOrWhiteSpace(LoginInput?.UserNameOrEmailAddress) || 
                string.IsNullOrWhiteSpace(LoginInput?.Password))
            {
                ModelState.AddModelError(string.Empty, "Correo y contraseña son requeridos");
                return Page();
            }

            var result = await base.OnPostAsync(action);
            
            // Si el login fue exitoso, redirigir a post-login para manejo por rol
            if (result is RedirectResult || result is LocalRedirectResult || result is RedirectToPageResult)
            {
                // Preservar returnUrl si existe
                var returnUrl = ReturnUrl;
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect($"/account/post-login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                }
                return LocalRedirect("/account/post-login");
            }
            
            return result;
        }
        catch (Volo.Abp.Validation.AbpValidationException)
        {
            // Si hay error de validación, mostrar mensaje genérico
            ModelState.AddModelError(string.Empty, "Credenciales incorrectas");
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Error al iniciar sesión. Intenta de nuevo.");
            return Page();
        }
    }
}
