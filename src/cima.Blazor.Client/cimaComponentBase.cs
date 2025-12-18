using cima.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Volo.Abp.Localization;
using Volo.Abp.Users;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using MudBlazor;
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

    [Inject]
    protected ISnackbar? Snackbar { get; set; }

    private CurrentUserWrapper? _currentUser;
    protected ICurrentUser CurrentUser => _currentUser ??= new CurrentUserWrapper(AuthenticationStateProvider);

    private bool _disposed;

    protected cimaComponentBase()
    {
    }

    /// <summary>
    /// Inicializa el componente y el CurrentUser de manera asíncrona
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Inicializar el CurrentUser para prevenir errores de runtime en WASM
        if (_currentUser == null)
        {
            _currentUser = new CurrentUserWrapper(AuthenticationStateProvider);
            await _currentUser.InitializeAsync();
        }
        
        await base.OnInitializedAsync();
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
            
            // Try to display error to user via Snackbar if available
            if (Snackbar != null)
            {
                var errorMessage = GetUserFriendlyErrorMessage(exception);
                Snackbar.Add(errorMessage, MudBlazor.Severity.Error);
            }
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
    }

    /// <summary>
    /// Extracts a user-friendly error message, including BusinessException codes
    /// </summary>
    protected virtual string GetUserFriendlyErrorMessage(Exception exception)
    {
        // Check if it's an ABP RemoteServiceErrorInfo (when exception comes from API)
        if (exception is Volo.Abp.Http.Client.AbpRemoteProcedureCallException abpRpcException)
        {
            var errorInfo = abpRpcException.Error;
            if (errorInfo != null)
            {
                // Include error code if present (e.g., "Listing:NoImages")
                if (!string.IsNullOrEmpty(errorInfo.Code))
                {
                    return L[$"{errorInfo.Code}"] ?? $"[{errorInfo.Code}] {errorInfo.Message}";
                }
                return errorInfo.Message ?? exception.Message;
            }
        }
        
        // Check for direct BusinessException (rarely happens on client)
        if (exception is Volo.Abp.BusinessException businessException)
        {
            if (!string.IsNullOrEmpty(businessException.Code))
            {
                return L[$"{businessException.Code}"] ?? $"[{businessException.Code}] {businessException.Message}";
            }
        }

        return exception.Message;
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
/// IMPORTANTE: No puede usar operaciones bloqueantes como .GetAwaiter().GetResult() 
/// porque el runtime de WebAssembly es single-threaded
/// </summary>
public class CurrentUserWrapper : ICurrentUser
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private ClaimsPrincipal? _principal;
    private bool _initialized;

    public CurrentUserWrapper(AuthenticationStateProvider authenticationStateProvider)
    {
        _authenticationStateProvider = authenticationStateProvider;
    }

    /// <summary>
    /// Inicializa el principal de manera asíncrona
    /// DEBE ser llamado antes de usar cualquier propiedad
    /// </summary>
    public async Task InitializeAsync()
    {
        if (!_initialized)
        {
            var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
            _principal = state.User;
            _initialized = true;
        }
    }

    private ClaimsPrincipal GetPrincipal()
    {
        if (_principal == null)
        {
            // Retornar un ClaimsPrincipal vacío si no se ha inicializado
            // Esto previene el error en runtime de WASM
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
        return _principal;
    }

    public bool IsAuthenticated => GetPrincipal().Identity?.IsAuthenticated ?? false;

    public Guid? Id
    {
        get
        {
            var idClaim = GetPrincipal().FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : null;
        }
    }

    public string? UserName => GetPrincipal().FindFirst(ClaimTypes.Name)?.Value;

    public string? Name => GetPrincipal().FindFirst("name")?.Value;

    public string? SurName => GetPrincipal().FindFirst("family_name")?.Value;

    public string? PhoneNumber => GetPrincipal().FindFirst(ClaimTypes.MobilePhone)?.Value;

    public bool PhoneNumberVerified => bool.TryParse(
        GetPrincipal().FindFirst("phone_number_verified")?.Value, 
        out var verified) && verified;

    public string? Email => GetPrincipal().FindFirst(ClaimTypes.Email)?.Value;

    public bool EmailVerified => bool.TryParse(
        GetPrincipal().FindFirst("email_verified")?.Value, 
        out var verified) && verified;

    public Guid? TenantId
    {
        get
        {
            var tenantIdClaim = GetPrincipal().FindFirst("tenant_id")?.Value;
            return Guid.TryParse(tenantIdClaim, out var id) ? id : null;
        }
    }

    public string[] Roles => GetPrincipal()
        .FindAll(ClaimTypes.Role)
        .Select(c => c.Value)
        .ToArray();

    public Claim? FindClaim(string claimType) => GetPrincipal().FindFirst(claimType);

    public Claim[] FindClaims(string claimType) => GetPrincipal()
        .FindAll(claimType)
        .ToArray();

    public Claim[] GetAllClaims() => GetPrincipal().Claims.ToArray();

    public bool IsInRole(string roleName) => GetPrincipal().IsInRole(roleName);
}
