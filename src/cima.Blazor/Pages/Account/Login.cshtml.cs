using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.Account.Web.Pages.Account;
using Volo.Abp.Identity;
using Volo.Abp.Validation;

namespace cima.Blazor.Pages.Account;

// Disable automatic validation - we'll handle it manually
[DisableValidation]
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
        if (CurrentUser.IsAuthenticated)
        {
            return LocalRedirect("/account/post-login");
        }
        
        // Let the base class initialize LoginInput properly
        await base.OnGetAsync();
        
        // Ensure LoginInput is initialized after base call
        LoginInput ??= new LoginInputModel();
        
        return Page();
    }

    public override async Task<IActionResult> OnPostAsync(string action)
    {
        _logger.LogInformation("=== LOGIN ATTEMPT START ===");
        _logger.LogInformation("Action parameter: {Action}", action ?? "(null)");
        _logger.LogInformation("LoginInput is null: {IsNull}", LoginInput == null);
        
        if (LoginInput != null)
        {
            _logger.LogInformation("User: {User}", LoginInput.UserNameOrEmailAddress ?? "(null)");
            _logger.LogInformation("Password provided: {HasPassword}", !string.IsNullOrEmpty(LoginInput.Password));
            _logger.LogInformation("RememberMe: {RememberMe}", LoginInput.RememberMe);
        }
        
        // Manual binding if LoginInput is null or empty
        if (LoginInput == null)
        {
            _logger.LogWarning("LoginInput is null, creating new instance");
            LoginInput = new LoginInputModel();
        }
        
        // Always try to bind from form data to ensure we have the values
        if (string.IsNullOrWhiteSpace(LoginInput.UserNameOrEmailAddress))
        {
            LoginInput.UserNameOrEmailAddress = Request.Form["LoginInput.UserNameOrEmailAddress"].ToString();
            _logger.LogInformation("Manually bound UserNameOrEmailAddress: {User}", LoginInput.UserNameOrEmailAddress);
        }
        
        if (string.IsNullOrWhiteSpace(LoginInput.Password))
        {
            LoginInput.Password = Request.Form["LoginInput.Password"].ToString();
            _logger.LogInformation("Manually bound Password length: {Length}", LoginInput.Password?.Length ?? 0);
        }
        
        // Handle RememberMe checkbox
        var rememberMeValue = Request.Form["LoginInput.RememberMe"].ToString();
        LoginInput.RememberMe = rememberMeValue == "true" || rememberMeValue == "on";
        
        // Validate manually
        if (string.IsNullOrWhiteSpace(LoginInput.UserNameOrEmailAddress))
        {
            _logger.LogWarning("Username/Email is empty after binding");
            ModelState.AddModelError(string.Empty, "El usuario o correo es requerido");
            return Page();
        }
        
        if (string.IsNullOrWhiteSpace(LoginInput.Password))
        {
            _logger.LogWarning("Password is empty after binding");
            ModelState.AddModelError(string.Empty, "La contraseña es requerida");
            return Page();
        }
        
        _logger.LogInformation("Validation passed, attempting login with user: {User}", LoginInput.UserNameOrEmailAddress);
        
        // Clear ModelState before calling base to avoid validation errors from the base class
        ModelState.Clear();
        
        try
        {
            // Pass the action parameter (or default to "Login")
            var result = await base.OnPostAsync(action ?? "Login");
            
            _logger.LogInformation("Login result type: {Type}", result?.GetType().Name);
            
            if (result is RedirectResult rr)
            {
                _logger.LogInformation("Redirect to: {Url}", rr.Url);
                return LocalRedirect("/account/post-login");
            }
            if (result is LocalRedirectResult lr)
            {
                _logger.LogInformation("LocalRedirect to: {Url}", lr.Url);
                return LocalRedirect("/account/post-login");
            }
            if (result is RedirectToPageResult rp)
            {
                _logger.LogInformation("RedirectToPage: {Page}", rp.PageName);
                return LocalRedirect("/account/post-login");
            }
            
            _logger.LogWarning("Login returned Page() - checking ModelState");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("ModelState error: {Error}", error.ErrorMessage);
            }
            
            return result ?? Page();
        }
        catch (AbpValidationException vex)
        {
            _logger.LogError(vex, "ABP Validation exception during login");
            foreach (var error in vex.ValidationErrors)
            {
                var members = string.Join(",", error.MemberNames);
                _logger.LogError("Validation error - Member: {Member}, Message: {Message}", members, error.ErrorMessage);
            }
            ModelState.AddModelError(string.Empty, "Error de validación. Verifique sus credenciales.");
            return Page();
        }
        catch (UserFriendlyException ufe)
        {
            _logger.LogError(ufe, "User friendly exception: {Message}", ufe.Message);
            ModelState.AddModelError(string.Empty, ufe.Message);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login exception: {Message}", ex.Message);
            ModelState.AddModelError(string.Empty, "Error al iniciar sesión");
            return Page();
        }
    }
}
