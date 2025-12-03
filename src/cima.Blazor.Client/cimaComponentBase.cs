using cima.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Volo.Abp.Localization;
using Volo.Abp.Users;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace cima.Blazor.Client;

/// <summary>
/// Clase base para componentes Blazor WASM del cliente
/// No hereda de AbpComponentBase para evitar dependencias no disponibles en WASM
/// </summary>
public abstract class cimaComponentBase : ComponentBase, IDisposable
{
    [Inject]
    protected IStringLocalizer<cimaResource> L { get; set; } = default!;

    [Inject]
    protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    private ICurrentUser? _currentUser;
    protected ICurrentUser CurrentUser => _currentUser ??= new CurrentUserWrapper(AuthenticationStateProvider);

    private bool _disposed;

    protected cimaComponentBase()
    {
    }

    /// <summary>
    /// Maneja errores de forma graceful en Blazor WASM
    /// </summary>
    protected virtual async Task HandleErrorAsync(Exception exception)
    {
        // Don't attempt JS calls if component is disposed or if it's a cancellation
        if (_disposed)
        {
            return;
        }

        // Ignore ObjectDisposedException and OperationCanceledException - these happen during navigation
        if (exception is ObjectDisposedException || exception is OperationCanceledException)
        {
            return;
        }

        try
        {
            // En WASM, simplemente logueamos a la consola del navegador
            await JSRuntime.InvokeVoidAsync("console.error", $"Error: {exception.Message}", exception.ToString());
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
        catch (InvalidOperationException)
        {
            // Prerendering or disposed, ignore
        }
        catch (TaskCanceledException)
        {
            // Navigation occurred, ignore
        }
        
        // Opcionalmente mostrar un mensaje al usuario
        // Puedes integrar con MudBlazor Snackbar aquí
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}

/// <summary>
/// Wrapper simple para ICurrentUser en Blazor WASM
/// </summary>
public class CurrentUserWrapper : ICurrentUser
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private ClaimsPrincipal? _principal;

    public CurrentUserWrapper(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    private async Task<ClaimsPrincipal> GetPrincipalAsync()
    {
        if (_principal == null)
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            _principal = state.User;
        }
        return _principal;
    }

    public bool IsAuthenticated => GetPrincipalAsync().GetAwaiter().GetResult().Identity?.IsAuthenticated ?? false;

    public Guid? Id
    {
        get
        {
            var idClaim = GetPrincipalAsync().GetAwaiter().GetResult().FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public string? UserName => GetPrincipalAsync().GetAwaiter().GetResult().FindFirst(ClaimTypes.Name)?.Value;

    public string? Name => GetPrincipalAsync().GetAwaiter().GetResult().FindFirst("name")?.Value;

    public string? SurName => GetPrincipalAsync().GetAwaiter().GetResult().FindFirst("family_name")?.Value;

    public string? PhoneNumber => GetPrincipalAsync().GetAwaiter().GetResult().FindFirst(ClaimTypes.MobilePhone)?.Value;

    public bool PhoneNumberVerified => bool.TryParse(
        GetPrincipalAsync().GetAwaiter().GetResult().FindFirst("phone_number_verified")?.Value, 
        out var verified) && verified;

    public string? Email => GetPrincipalAsync().GetAwaiter().GetResult().FindFirst(ClaimTypes.Email)?.Value;

    public bool EmailVerified => bool.TryParse(
        GetPrincipalAsync().GetAwaiter().GetResult().FindFirst("email_verified")?.Value, 
        out var verified) && verified;

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = GetPrincipalAsync().GetAwaiter().GetResult().FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantIdClaim, out var id) ? id : null;
        }
    }

    public string[] Roles => GetPrincipalAsync().GetAwaiter().GetResult()
        .FindAll(ClaimTypes.Role)
        .Select(c => c.Value)
        .ToArray();

    public Claim? FindClaim(string claimType) => GetPrincipalAsync().GetAwaiter().GetResult().FindFirst(claimType);

    public Claim[] FindClaims(string claimType) => GetPrincipalAsync().GetAwaiter().GetResult()
        .FindAll(claimType)
        .ToArray();

    public Claim[] GetAllClaims() => GetPrincipalAsync().GetAwaiter().GetResult().Claims.ToArray();

    public bool IsInRole(string roleName) => GetPrincipalAsync().GetAwaiter().GetResult().IsInRole(roleName);
}
