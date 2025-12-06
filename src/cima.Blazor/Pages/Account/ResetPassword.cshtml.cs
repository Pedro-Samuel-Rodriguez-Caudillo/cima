using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace cima.Blazor.Pages.Account;

public class ResetPasswordModel : Volo.Abp.Account.Web.Pages.Account.ResetPasswordModel
{
    [BindProperty(SupportsGet = true)]
    public bool PasswordChanged { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public bool InvalidToken { get; set; }
    
    [BindProperty]
    public string? ErrorMessage { get; set; }

    public override async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "La nueva contraseña es requerida";
                return Page();
            }

            var result = await base.OnPostAsync();
            
            if (result is RedirectResult || result is LocalRedirectResult)
            {
                PasswordChanged = true;
                ErrorMessage = null;
                return Page();
            }
            
            return result;
        }
        catch (Volo.Abp.Validation.AbpValidationException ex)
        {
            // Errores de validación de contraseña
            var firstError = ex.ValidationErrors.FirstOrDefault();
            if (firstError?.ErrorMessage?.Contains("password", StringComparison.OrdinalIgnoreCase) == true)
            {
                ErrorMessage = "La contraseña debe tener al menos 6 caracteres, incluir mayúsculas, minúsculas y números";
            }
            else
            {
                ErrorMessage = firstError?.ErrorMessage ?? "Por favor verifica los datos ingresados";
            }
            return Page();
        }
        catch (Volo.Abp.UserFriendlyException ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("token", StringComparison.OrdinalIgnoreCase))
        {
            InvalidToken = true;
            ErrorMessage = "El enlace para restablecer la contraseña ha expirado o es inválido. Por favor solicita uno nuevo.";
            return Page();
        }
        catch (Exception)
        {
            InvalidToken = true;
            ErrorMessage = "No se pudo restablecer la contraseña. El enlace puede haber expirado.";
            return Page();
        }
    }
}
