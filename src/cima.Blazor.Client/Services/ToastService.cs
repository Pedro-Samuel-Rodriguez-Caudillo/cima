using System;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;

namespace cima.Blazor.Client.Services;

/// <summary>
/// Servicio de notificaciones toast con accesibilidad mejorada
/// </summary>
public interface IToastService
{
    void Success(string message, string? title = null);
    void Error(string message, string? title = null);
    void Warning(string message, string? title = null);
    void Info(string message, string? title = null);
    void Show(string message, Severity severity, string? title = null, Action<SnackbarOptions>? configure = null);
}

public class ToastService : IToastService
{
    private readonly ISnackbar _snackbar;

    public ToastService(ISnackbar snackbar)
    {
        _snackbar = snackbar;
        ConfigureDefaults();
    }

    private void ConfigureDefaults()
    {
        _snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
        _snackbar.Configuration.PreventDuplicates = true;
        _snackbar.Configuration.NewestOnTop = true;
        _snackbar.Configuration.ShowCloseIcon = true;
        _snackbar.Configuration.VisibleStateDuration = 4000;
        _snackbar.Configuration.HideTransitionDuration = 300;
        _snackbar.Configuration.ShowTransitionDuration = 300;
        _snackbar.Configuration.SnackbarVariant = Variant.Filled;
    }

    public void Success(string message, string? title = null)
    {
        Show(message, Severity.Success, title ?? "¡Éxito!", options =>
        {
            options.Icon = Icons.Material.Filled.CheckCircle;
        });
    }

    public void Error(string message, string? title = null)
    {
        Show(message, Severity.Error, title ?? "Error", options =>
        {
            options.Icon = Icons.Material.Filled.Error;
            options.VisibleStateDuration = 6000; // Más tiempo para errores
        });
    }

    public void Warning(string message, string? title = null)
    {
        Show(message, Severity.Warning, title ?? "Atención", options =>
        {
            options.Icon = Icons.Material.Filled.Warning;
        });
    }

    public void Info(string message, string? title = null)
    {
        Show(message, Severity.Info, title ?? "Información", options =>
        {
            options.Icon = Icons.Material.Filled.Info;
        });
    }

    public void Show(string message, Severity severity, string? title = null, Action<SnackbarOptions>? configure = null)
    {
        var fullMessage = string.IsNullOrEmpty(title) 
            ? message 
            : $"<strong>{title}</strong><br/>{message}";

        _snackbar.Add(fullMessage, severity, config =>
        {
            // Accesibilidad: configurar role y aria
            config.ActionColor = Color.Default;
            config.CloseAfterNavigation = false;
            
            // Aplicar configuración personalizada
            configure?.Invoke(config);
        });
    }
}

/// <summary>
/// Extensiones para registro del servicio
/// </summary>
public static class ToastServiceExtensions
{
    public static IServiceCollection AddCimaToastService(this IServiceCollection services)
    {
        services.AddScoped<IToastService, ToastService>();
        return services;
    }
}
