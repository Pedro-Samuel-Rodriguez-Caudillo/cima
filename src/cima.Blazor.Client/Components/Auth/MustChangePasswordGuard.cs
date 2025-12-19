using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using cima.Architects;
using cima.Blazor.Client.Services;

namespace cima.Blazor.Client.Components.Auth;

/// <summary>
/// Component that checks if the user must change their password
/// and automatically redirects to the change password page.
/// Uses a cached service to avoid redundant API calls on each navigation.
/// </summary>
public partial class MustChangePasswordGuard : ComponentBase
{
    [Inject] private IArchitectAppService ArchitectService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    [Inject] private ILogger<MustChangePasswordGuard> Logger { get; set; } = null!;
    [Inject] private MustChangePasswordStateService StateService { get; set; } = null!;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool _isChecking = true;
    private bool _mustChangePassword = false;
    private bool _navigationInitiated = false;

    // Paths excluded from the check
    private static readonly string[] ExcludedPaths =
    {
        "/account/change-password",
        "/account/login",
        "/account/logout",
        "/account/register",
        "/authentication",
        "/signin-oidc",
        "/signout-callback-oidc"
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CheckMustChangePassword();
        }
    }

    private async Task CheckMustChangePassword()
    {
        try
        {
            var currentPath = new Uri(Navigation.Uri).AbsolutePath;

            // Skip excluded paths
            if (ExcludedPaths.Any(p => currentPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                _isChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Skip admin paths
            if (currentPath.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
            {
                _isChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Use cached result if available - AVOIDS REDUNDANT API CALLS
            if (StateService.HasChecked)
            {
                _mustChangePassword = StateService.MustChangePassword;
                if (_mustChangePassword)
                {
                    RedirectToChangePassword();
                }
                _isChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            // Only check if authenticated
            if (user.Identity?.IsAuthenticated != true)
            {
                _isChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Admins don't need to change password here
            if (user.IsInRole("admin"))
            {
                StateService.SetResult(false);
                _isChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Make the API call only once per session
            try
            {
                _mustChangePassword = await ArchitectService.MustChangePasswordAsync();
                StateService.SetResult(_mustChangePassword);

                if (_mustChangePassword)
                {
                    RedirectToChangePassword();
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogDebug("MustChangePassword verification cancelled");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                Logger.LogDebug("Unauthorized token during password change verification");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                // User probably doesn't have an architect profile
                StateService.SetResult(false);
                Logger.LogDebug("No permissions for password change verification");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error during mandatory password change verification");
        }
        finally
        {
            _isChecking = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void RedirectToChangePassword()
    {
        var target = "/account/change-password";
        var currentPath = new Uri(Navigation.Uri).AbsolutePath;

        if (!_navigationInitiated && !currentPath.StartsWith(target, StringComparison.OrdinalIgnoreCase))
        {
            _navigationInitiated = true;
            Logger.LogInformation("User must change password, redirecting...");
            Navigation.NavigateTo(target, replace: true);
        }
    }

    public override Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);
        return base.SetParametersAsync(ParameterView.Empty);
    }
}
