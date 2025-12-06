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
/// y redirige automáticamente a la página de cambio.
/// Implementado de forma no intrusiva: se ejecuta solo en el primer render
/// y evita redirecciones en rutas "admin" o rutas excluidas para no bloquear
/// flujos administrativos.
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

    // Evita múltiples redirecciones simultáneas
    private bool _navigationInitiated = false;
    // Evita volver a comprobar repetidamente si ya se comprobó con éxito
    private bool _checkedOnce = false;

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

            // No verificar en rutas excluidas (evita llamadas innecesarias)
            if (ExcludedPaths.Any(p => currentPath.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                _isChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Evitar bloquear paneles administrativos: permitir /admin/* directamente
            if (currentPath.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
            {
                _isChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Si ya comprobamos y no hay cambio obligatorio, no volver a comprobar
            if (_checkedOnce)
            {
                _isChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            // Solo verificar si está autenticado
            if (user.Identity?.IsAuthenticated != true)
            {
                _isChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Si el usuario es administrador, no forzar el cambio aquí (permite operaciones administrativas)
            if (user.IsInRole("admin"))
            {
                _isChecking = false;
                await InvokeAsync(StateHasChanged);
                return;
            }

            // Verificar si debe cambiar contraseña
            try
            {
                _mustChangePassword = await ArchitectService.MustChangePasswordAsync();

                _checkedOnce = true;

                if (_mustChangePassword)
                {
                    var target = "/account/change-password";

                    // Evitar redireccionar si ya estamos en la ruta objetivo o si ya iniciamos navegación
                    if (!_navigationInitiated && !new Uri(Navigation.Uri).AbsolutePath.StartsWith(target, StringComparison.OrdinalIgnoreCase))
                    {
                        _navigationInitiated = true;
                        Logger.LogInformation("Usuario debe cambiar contraseña, redirigiendo...");

                        // Reemplaza la entrada de historial para evitar loops al pulsar atrás
                        Navigation.NavigateTo(target, replace: true);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.LogDebug("Verificación de MustChangePassword cancelada");
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
            await InvokeAsync(StateHasChanged);
        }
    }

    // Render fragment: mostrar children siempre, el guard solo redirige cuando aplica
    public override Task SetParametersAsync(ParameterView parameters)
    {
        parameters.SetParameterProperties(this);
        return base.SetParametersAsync(ParameterView.Empty);
    }
}
