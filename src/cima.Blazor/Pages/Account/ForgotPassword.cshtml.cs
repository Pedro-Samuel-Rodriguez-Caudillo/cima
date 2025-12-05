using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Account.Web.Pages.Account;

namespace cima.Blazor.Pages.Account;

public class ForgotPasswordModel : Volo.Abp.Account.Web.Pages.Account.ForgotPasswordModel
{
    [BindProperty]
    public bool EmailSent { get; set; }

    public override async Task<IActionResult> OnPostAsync()
    {
        var result = await base.OnPostAsync();
        
        // Siempre mostrar éxito por seguridad (no revelar si el email existe)
        EmailSent = true;
        return Page();
    }
}
