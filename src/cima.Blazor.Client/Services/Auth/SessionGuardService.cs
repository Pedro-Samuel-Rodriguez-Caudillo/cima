using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using cima.Localization;
using Microsoft.Extensions.Localization;

using cima.Blazor.Client.Components.Common;

namespace cima.Blazor.Client.Services.Auth;

/// <summary>
/// Intercepts navigation or API calls to check for session/auth issues
/// and shows a modal dialog instead of just redirecting.
/// </summary>
public class SessionGuardService(
    IDialogService dialogService,
    NavigationManager navigationManager,
    AuthenticationStateProvider authenticationStateProvider,
    IStringLocalizer<cimaResource> l)
{
    private readonly IDialogService _dialogService = dialogService;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;
    private readonly IStringLocalizer<cimaResource> _l = l;

    public async Task<bool> ValidateSessionAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();

        if (authState.User.Identity?.IsAuthenticated != true)
        {
            await ShowSessionExpiredDialogAsync();
            return false;
        }

        return true;
    }

    private async Task ShowSessionExpiredDialogAsync()
    {
        var parameters = new DialogParameters
        {
            { "ContentText", _l["Auth:SessionExpiredContent"] },
            { "ButtonText", _l["Auth:Login"] },
            { "Color", Color.Primary }
        };

        DialogOptions options = new() { CloseButton = false, BackdropClick = false };

        var dialog = await _dialogService.ShowAsync<SessionExpiredDialog>(_l["Auth:SessionExpiredTitle"], parameters, options);
        var result = await dialog.Result;

        if (result?.Canceled == false)
        {
            _navigationManager.NavigateTo("authentication/login", forceLoad: true);
        }
    }
}
