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
}

public class LoginRedirectService : ILoginRedirectService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly NavigationManager _navigation;

    // Rutas por defecto según rol
    private const string AdminDashboard = "/admin/dashboard";
    private const string ArchitectDashboard = "/architect/dashboard";
    private const string PublicHome = "/";
    private const string ChangePasswordRoute = "/account/change-password";

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

        // Verificar si debe cambiar contraseña
        var mustChangePassword = user.FindFirst("MustChangePassword")?.Value;
        if (mustChangePassword == "true")
            return ChangePasswordRoute;

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
        // Si hay returnUrl válido, usarlo (excepto si debe cambiar contraseña)
        var mustChangePassword = user.FindFirst("MustChangePassword")?.Value == "true";
        
        if (mustChangePassword)
        {
            // Preservar returnUrl para después del cambio de contraseña
            return string.IsNullOrEmpty(returnUrl) 
                ? ChangePasswordRoute 
                : $"{ChangePasswordRoute}?returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        if (!string.IsNullOrEmpty(returnUrl) && IsValidReturnUrl(returnUrl))
        {
            return returnUrl;
        }

        return GetDefaultRouteForUser(user);
    }

    public async Task<string> GetRedirectUrlAsync(string? returnUrl = null)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return GetRedirectUrl(authState.User, returnUrl);
    }

    private bool IsValidReturnUrl(string returnUrl)
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
