using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Volo.Abp.AspNetCore.Components.Messages;

namespace cima.Blazor.Services;

/// <summary>
/// Simple server-side fallback for components that request IUiMessageService during prerender.
/// Logs the requested message and immediately completes without showing client-side UI.
/// </summary>
public class FallbackUiMessageService : IUiMessageService
{
    private readonly ILogger<FallbackUiMessageService> _logger;

    public FallbackUiMessageService(ILogger<FallbackUiMessageService> logger)
    {
        _logger = logger;
    }

    public Task Info(string message, string? title = null, Action<UiMessageOptions>? options = null)
        => LogAsync(nameof(Info), message, title, options);

    public Task Success(string message, string? title = null, Action<UiMessageOptions>? options = null)
        => LogAsync(nameof(Success), message, title, options);

    public Task Warn(string message, string? title = null, Action<UiMessageOptions>? options = null)
        => LogAsync(nameof(Warn), message, title, options);

    public Task Error(string message, string? title = null, Action<UiMessageOptions>? options = null)
        => LogAsync(nameof(Error), message, title, options);

    public Task<bool> Confirm(string message, string? title = null, Action<UiMessageOptions>? options = null)
    {
        Log(nameof(Confirm), message, title, options);
        return Task.FromResult(false);
    }

    private Task LogAsync(string level, string message, string? title, Action<UiMessageOptions>? options)
    {
        Log(level, message, title, options);
        return Task.CompletedTask;
    }

    private void Log(string level, string message, string? title, Action<UiMessageOptions>? options)
    {
        _logger.LogInformation("UI message ({Level}) {Title}: {Message}", level, title ?? "(No Title)", message);

        if (options is not null)
        {
            // Execute the options callback to keep calling code compatible even though we ignore the values.
            options(new UiMessageOptions());
        }
    }
}