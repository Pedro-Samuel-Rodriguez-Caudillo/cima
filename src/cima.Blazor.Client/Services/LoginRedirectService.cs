using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace cima.Blazor.Client.Services;

/// <summary>
/// Servicio para manejar redirecciones post-login basadas en rol
/// </summary>
public interface ILoginRedirectService
{
    string GetDefaultRouteForUser(ClaimsPrincipal user);
    string GetRedirectUrl(ClaimsPrincipal user, string? returnUrl = null);
    Task<string> GetRedirectUrlAsync(string? returnUrl = null);
    bool IsValidReturnUrl(string returnUrl);
}

public class LoginRedirectService : ILoginRedirectService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly NavigationManager _navigation;

    // Rutas por defecto según rol
    public const string AdminDashboard = "/admin/dashboard";
    public const string ArchitectDashboard = "/architect/dashboard";
    public const string PublicHome = "/";
    public const string ChangePasswordRoute = "/account/change-password";

    public LoginRedirectService(
        AuthenticationStateProvider authStateProvider,
        NavigationManager navigation)
    {
        _authStateProvider = authStateProvider;
        _navigation = navigation;
    }

    public string GetDefaultRouteForUser(ClaimsPrincipal user)
    {
        if (!user.Identity?.IsAuthenticated ?? true)
            return PublicHome;

        // Admin tiene prioridad
        if (user.IsInRole("admin"))
            return AdminDashboard;

        // Architect
        if (user.IsInRole("architect"))
            return ArchitectDashboard;

        // Usuario público autenticado
        return PublicHome;
    }

    public string GetRedirectUrl(ClaimsPrincipal user, string? returnUrl = null)
    {
        // Si hay returnUrl válido, usarlo
        if (!string.IsNullOrEmpty(returnUrl) && IsValidReturnUrl(returnUrl))
        {
            // No redirigir a rutas de auth
            if (!IsAuthRoute(returnUrl))
            {
                return returnUrl;
            }
        }

        return GetDefaultRouteForUser(user);
    }

    public async Task<string> GetRedirectUrlAsync(string? returnUrl = null)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return GetRedirectUrl(authState.User, returnUrl);
    }

    public bool IsValidReturnUrl(string returnUrl)
    {
        // Solo permitir URLs relativas o del mismo dominio
        if (string.IsNullOrWhiteSpace(returnUrl))
            return false;

        // URLs relativas son válidas
        if (returnUrl.StartsWith("/") && !returnUrl.StartsWith("//"))
            return true;

        // URLs del mismo origen
        try
        {
            var baseUri = new Uri(_navigation.BaseUri);
            var returnUri = new Uri(returnUrl, UriKind.RelativeOrAbsolute);
            
            if (!returnUri.IsAbsoluteUri)
                return true;

            return returnUri.Host == baseUri.Host;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAuthRoute(string url)
    {
        var lowerUrl = url.ToLowerInvariant();
        return lowerUrl.Contains("/account/login") ||
               lowerUrl.Contains("/account/logout") ||
               lowerUrl.Contains("/account/post-login") ||
               lowerUrl.Contains("/authentication") ||
               lowerUrl.Contains("/signin-oidc") ||
               lowerUrl.Contains("/signout-callback-oidc");
    }
}

/// <summary>
/// Extensiones para registro del servicio
/// </summary>
public static class LoginRedirectServiceExtensions
{
    public static IServiceCollection AddCimaLoginRedirect(this IServiceCollection services)
    {
        services.AddScoped<ILoginRedirectService, LoginRedirectService>();
        return services;
    }
}
