using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using cima.Architects;

namespace cima.Blazor.Client.Components.Auth;

/// <summary>
/// Componente que verifica si el usuario debe cambiar su contraseña
/// y redirige automáticamente a la página de cambio
/// </summary>
public partial class MustChangePasswordGuard : ComponentBase
{
    [Inject] private IArchitectAppService ArchitectService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    [Inject] private ILogger<MustChangePasswordGuard> Logger { get; set; } = null!;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool _isChecking = true;
    private bool _mustChangePassword = false;

    // Rutas excluidas de la verificación
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

    protected override async Task OnInitializedAsync()
    {
        await CheckMustChangePassword();
    }

    private async Task CheckMustChangePassword()
    {
        try
        {
            // No verificar en rutas excluidas (verificar primero para evitar llamadas innecesarias)
            var currentPath = new Uri(Navigation.Uri).AbsolutePath;
            if (ExcludedPaths.Any(p => currentPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                _isChecking = false;
                return;
            }

            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            // Solo verificar si está autenticado
            if (user.Identity?.IsAuthenticated != true)
            {
                _isChecking = false;
                return;
            }

            // Verificar si debe cambiar contraseña
            try
            {
                _mustChangePassword = await ArchitectService.MustChangePasswordAsync();

                if (_mustChangePassword)
                {
                    Logger.LogInformation("Usuario debe cambiar contraseña, redirigiendo...");
                    Navigation.NavigateTo("/account/change-password", forceLoad: false);
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Token expirado o inválido - dejar que el flujo normal de auth lo maneje
                Logger.LogDebug("Token no autorizado durante verificación de cambio de contraseña");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                // Sin permisos - probablemente usuario normal sin perfil de arquitecto
                Logger.LogDebug("Sin permisos para verificar cambio de contraseña");
            }
        }
        catch (Exception ex)
        {
            // Si falla la verificación por cualquier otra razón, continuar normalmente
            // pero loguear para diagnóstico
            Logger.LogWarning(ex, "Error durante verificación de cambio de contraseña obligatorio");
        }
        finally
        {
            _isChecking = false;
        }
    }
}
