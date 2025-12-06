using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Account.Web.Pages.Account;

namespace cima.Blazor.Pages.Account;

public class ForgotPasswordModel : Volo.Abp.Account.Web.Pages.Account.ForgotPasswordModel
{
    [BindProperty]
    public bool EmailSent { get; set; }
    
    [BindProperty]
    public string? ErrorMessage { get; set; }

    public override async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Validar email
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Por favor ingresa tu correo electrónico";
                return Page();
            }

            var result = await base.OnPostAsync();
            
            // Siempre mostrar éxito por seguridad (no revelar si el email existe)
            EmailSent = true;
            ErrorMessage = null;
            return Page();
        }
        catch (Volo.Abp.Validation.AbpValidationException ex)
        {
            // Error de validación (formato de email inválido, etc.)
            ErrorMessage = ex.ValidationErrors.FirstOrDefault()?.ErrorMessage 
                ?? "Por favor verifica el formato del correo electrónico";
            return Page();
        }
        catch (Volo.Abp.UserFriendlyException ex)
        {
            // Error amigable de ABP
            ErrorMessage = ex.Message;
            return Page();
        }
        catch (Exception)
        {
            // Por seguridad, mostrar éxito incluso si hay error interno
            // (no revelar si el email existe o no)
            EmailSent = true;
            ErrorMessage = null;
            return Page();
        }
    }
}
