using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace cima.Blazor.Pages.Account;

public class ResetPasswordModel : Volo.Abp.Account.Web.Pages.Account.ResetPasswordModel
{
    [BindProperty(SupportsGet = true)]
    public bool PasswordChanged { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public bool InvalidToken { get; set; }

    public override async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var result = await base.OnPostAsync();
            
            if (result is RedirectResult || result is LocalRedirectResult)
            {
                PasswordChanged = true;
                return Page();
            }
            
            return result;
        }
        catch
        {
            InvalidToken = true;
            return Page();
        }
    }
}
