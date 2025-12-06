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
        
        var result = await base.OnGetAsync();
        
        // Forzar EnableLocalLogin a true ya que este proyecto usa autenticación local
        // Esto soluciona el problema de "Login no disponible" que ocurre cuando
        // ABP no detecta correctamente la configuración de login local
        EnableLocalLogin = true;
        
        return result;
    }

    public override async Task<IActionResult> OnPostAsync(string action)
    {
        try
        {
            // Validar que los campos requeridos estén presentes
            if (string.IsNullOrWhiteSpace(LoginInput?.UserNameOrEmailAddress))
            {
                ModelState.AddModelError(string.Empty, "El correo electrónico es requerido");
                return Page();
            }
            
            if (string.IsNullOrWhiteSpace(LoginInput?.Password))
            {
                ModelState.AddModelError(string.Empty, "La contraseña es requerida");
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
        catch (Volo.Abp.Validation.AbpValidationException ex)
        {
            // Mostrar errores de validación específicos
            foreach (var error in ex.ValidationErrors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage ?? "Error de validación");
            }
            return Page();
        }
        catch (Volo.Abp.UserFriendlyException ex)
        {
            // Errores amigables de ABP
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("locked", StringComparison.OrdinalIgnoreCase))
        {
            // Cuenta bloqueada
            ModelState.AddModelError(string.Empty, "Tu cuenta ha sido bloqueada temporalmente. Intenta más tarde.");
            return Page();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                                                    ex.Message.Contains("credential", StringComparison.OrdinalIgnoreCase))
        {
            // Credenciales incorrectas
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos");
            return Page();
        }
        catch (Exception)
        {
            // Error genérico - no revelar detalles por seguridad
            ModelState.AddModelError(string.Empty, "No se pudo iniciar sesión. Verifica tus credenciales e intenta de nuevo.");
            return Page();
        }
    }
}
