using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Account.Web;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Identity;

namespace cima.Blazor.Pages.Account;

public class LoginModel : Volo.Abp.Account.Web.Pages.Account.LoginModel
{
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        IAuthenticationSchemeProvider schemeProvider,
        IOptions<AbpAccountOptions> accountOptions,
        IOptions<IdentityOptions> identityOptions,
        IdentityDynamicClaimsPrincipalContributorCache identityDynamicClaimsPrincipalContributorCache,
        IWebHostEnvironment webHostEnvironment,
        ILogger<LoginModel> logger)
        : base(schemeProvider, accountOptions, identityOptions, identityDynamicClaimsPrincipalContributorCache, webHostEnvironment)
    {
        _logger = logger;
    }

    public override async Task<IActionResult> OnGetAsync()
    {
        _logger.LogDebug("Login OnGetAsync iniciado");
        
        // Si ya está autenticado, redirigir
        if (CurrentUser.IsAuthenticated)
        {
            _logger.LogDebug("Usuario ya autenticado, redirigiendo a post-login");
            return LocalRedirect("/account/post-login");
        }
        
        // Inicializar LoginInput para evitar null
        LoginInput = new LoginInputModel();
        
        // IMPORTANTE: Forzar EnableLocalLogin a true ANTES de llamar a base
        // Este proyecto usa autenticación local exclusivamente
        EnableLocalLogin = true;
        
        try
        {
            var result = await base.OnGetAsync();
            
            // Forzar EnableLocalLogin nuevamente después de base.OnGetAsync()
            // porque la clase base puede haberlo modificado
            EnableLocalLogin = true;
            
            _logger.LogDebug("Login OnGetAsync completado. EnableLocalLogin={EnableLocalLogin}", EnableLocalLogin);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en OnGetAsync de Login");
            // En caso de error, asegurar que la página se muestre correctamente
            EnableLocalLogin = true;
            return Page();
        }
    }

    public override async Task<IActionResult> OnPostAsync(string action)
    {
        _logger.LogDebug("Login OnPostAsync iniciado para usuario: {User}", LoginInput?.UserNameOrEmailAddress);
        
        // Asegurar que EnableLocalLogin esté habilitado para el POST también
        EnableLocalLogin = true;
        
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
            
            _logger.LogDebug("Login OnPostAsync resultado: {ResultType}", result?.GetType().Name);
            
            // Si el login fue exitoso, redirigir a post-login para manejo por rol
            if (result is RedirectResult || result is LocalRedirectResult || result is RedirectToPageResult)
            {
                // Preservar returnUrl si existe
                var returnUrl = ReturnUrl;
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    _logger.LogDebug("Login exitoso, redirigiendo a post-login con returnUrl: {ReturnUrl}", returnUrl);
                    return LocalRedirect($"/account/post-login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                }
                _logger.LogDebug("Login exitoso, redirigiendo a post-login");
                return LocalRedirect("/account/post-login");
            }
            
            return result;
        }
        catch (Volo.Abp.Validation.AbpValidationException ex)
        {
            _logger.LogWarning(ex, "Error de validación en login");
            // Mostrar errores de validación específicos
            foreach (var error in ex.ValidationErrors)
            {
                ModelState.AddModelError(string.Empty, error.ErrorMessage ?? "Error de validación");
            }
            return Page();
        }
        catch (Volo.Abp.UserFriendlyException ex)
        {
            _logger.LogWarning(ex, "Error amigable en login: {Message}", ex.Message);
            // Errores amigables de ABP
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("locked", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Cuenta bloqueada");
            // Cuenta bloqueada
            ModelState.AddModelError(string.Empty, "Tu cuenta ha sido bloqueada temporalmente. Intenta más tarde.");
            return Page();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                                                    ex.Message.Contains("credential", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Credenciales incorrectas");
            // Credenciales incorrectas
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado en login");
            // Error genérico - no revelar detalles por seguridad
            ModelState.AddModelError(string.Empty, "No se pudo iniciar sesión. Verifica tus credenciales e intenta de nuevo.");
            return Page();
        }
    }
}
