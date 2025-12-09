using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace cima.Blazor.Infrastructure.Security;

/// <summary>
/// CONFIGURACIÓN de polticas de seguridad para Identity
/// </summary>
public static class IdentitySecurityConfiguration
{
    /// <summary>
    /// Configura las polticas de contraseña, lockout y seguridad
    /// </summary>
    public static IServiceCollection ConfigureCimaIdentityOptions(this IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
        {
            // ========================================
            // POLTICAS DE CONtraseñA
            // ========================================
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false; // No requerir caracteres especiales
            options.Password.RequiredUniqueChars = 1;

            // ========================================
            // POLTICAS DE LOCKOUT (Bloqueo por intentos fallidos)
            // ========================================
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // ========================================
            // POLTICAS DE USUARIO
            // ========================================
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

            // ========================================
            // POLTICAS DE SIGN-IN
            // ========================================
            options.SignIn.RequireConfirmedEmail = false; // Para arquitectos, el admin crea las cuentas
            options.SignIn.RequireConfirmedPhoneNumber = false;
            options.SignIn.RequireConfirmedAccount = false;
        });

        return services;
    }

    /// <summary>
    /// Configura opciones de cookies de autenticación
    /// </summary>
    public static IServiceCollection ConfigureCimaAuthCookies(this IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            // ========================================
            // CONFIGURACIÓN DE COOKIES
            // ========================================
            options.Cookie.Name = "cima.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;

            // ========================================
            // TIEMPOS DE EXPIRAción
            // ========================================
            options.ExpireTimeSpan = TimeSpan.FromDays(14); // Remember me duration
            options.SlidingExpiration = true; // Renovar cookie en cada request

            // ========================================
            // RUTAS DE autenticación
            // ========================================
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/account/access-denied";
            options.ReturnUrlParameter = "returnUrl";
        });

        return services;
    }

    /// <summary>
    /// Configura el validador de seguridad de stamps
    /// </summary>
    public static IServiceCollection ConfigureCimaSecurityStamp(this IServiceCollection services)
    {
        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            // Validar el security stamp cada 30 minutos
            // Si el usuario cambia su contraseña en otro dispositivo, se invalida la sesión
            options.ValidationInterval = TimeSpan.FromMinutes(30);
        });

        return services;
    }
}
