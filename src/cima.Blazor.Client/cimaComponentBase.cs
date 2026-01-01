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
using Volo.Abp.Http.Client;
using Volo.Abp;
using System.Collections;
using System.Collections.Generic;

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
    protected ISnackbar Snackbar { get; set; } = default!;

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
        if (exception is AggregateException aggregateException)
        {
            var innerMessages = aggregateException.Flatten().InnerExceptions
                .Select(GetUserFriendlyErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToList();

            if (innerMessages.Count > 0)
            {
                return string.Join(Environment.NewLine, innerMessages);
            }
        }

        if (exception is AbpRemoteCallException remoteCallException)
        {
            var errorInfo = remoteCallException.Error;
            var validationMessages = ExtractRemoteValidationMessages(errorInfo);
            if (validationMessages.Count > 0)
            {
                return string.Join(Environment.NewLine, validationMessages);
            }

            var remoteCode = GetRemoteErrorCode(errorInfo);
            if (!string.IsNullOrWhiteSpace(remoteCode))
            {
                var localized = L[remoteCode!];
                if (localized.ResourceNotFound == false)
                {
                    return localized.Value;
                }
            }

            var remoteMessage = errorInfo?.Message;
            var remoteDetails = errorInfo?.Details;

            if (!string.IsNullOrWhiteSpace(remoteMessage))
            {
                return remoteMessage;
            }

            if (!string.IsNullOrWhiteSpace(remoteDetails))
            {
                return remoteDetails;
            }
        }

        if (exception is BusinessException businessException)
        {
            if (!string.IsNullOrWhiteSpace(businessException.Code))
            {
                var localizedMessage = L[businessException.Code];
                if (localizedMessage.ResourceNotFound == false)
                {
                    return localizedMessage.Value;
                }

                return $"[{businessException.Code}] {businessException.Message}";
            }
        }

        // Check if message contains error code pattern like "[Code] Message"
        if (exception.Message.StartsWith("[") && exception.Message.Contains("]"))
        {
            return exception.Message;
        }

        if (string.IsNullOrWhiteSpace(exception.Message))
        {
            return "Ocurrio un error.";
        }

        return exception.Message;
    }

    private static List<string> ExtractRemoteValidationMessages(object? errorInfo)
    {
        var messages = new List<string>();
        if (errorInfo == null)
        {
            return messages;
        }

        var validationErrors = GetPropertyValue<IEnumerable?>(errorInfo, "ValidationErrors");
        if (validationErrors != null)
        {
            foreach (var error in validationErrors)
            {
                if (error == null)
                {
                    continue;
                }

                var message = GetPropertyValue<string?>(error, "Message")
                    ?? GetPropertyValue<string?>(error, "ErrorMessage");

                if (!string.IsNullOrWhiteSpace(message))
                {
                    messages.Add(message);
                }
            }
        }

        var data = GetPropertyValue<IDictionary?>(errorInfo, "Data");
        if (data != null)
        {
            foreach (var entry in data.Values)
            {
                if (entry is IEnumerable entryList and not string)
                {
                    foreach (var item in entryList)
                    {
                        var message = item?.ToString();
                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            messages.Add(message);
                        }
                    }
                }
            }
        }

        return messages.Distinct().ToList();
    }

    private static string? GetRemoteErrorCode(object? errorInfo)
    {
        if (errorInfo == null)
        {
            return null;
        }

        return GetPropertyValue<string?>(errorInfo, "Code");
    }

    private static T? GetPropertyValue<T>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName);
        if (property == null)
        {
            return default;
        }

        var value = property.GetValue(instance);
        if (value is T typedValue)
        {
            return typedValue;
        }

        return default;
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
