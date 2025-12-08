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

    public Task Info(string title, string message, Action<UiMessageOptions>? options = null)
        => LogAsync(nameof(Info), title, message, options);

    public Task Success(string title, string message, Action<UiMessageOptions>? options = null)
        => LogAsync(nameof(Success), title, message, options);

    public Task Warn(string title, string message, Action<UiMessageOptions>? options = null)
        => LogAsync(nameof(Warn), title, message, options);

    public Task Error(string title, string message, Action<UiMessageOptions>? options = null)
        => LogAsync(nameof(Error), title, message, options);

    public Task<bool> Confirm(string title, string message, Action<UiMessageOptions>? options = null)
    {
        Log(nameof(Confirm), title, message, options);
        return Task.FromResult(false);
    }

    private Task LogAsync(string level, string title, string message, Action<UiMessageOptions>? options)
    {
        Log(level, title, message, options);
        return Task.CompletedTask;
    }

    private void Log(string level, string title, string message, Action<UiMessageOptions>? options)
    {
        _logger.LogInformation("UI message ({Level}) {Title}: {Message}", level, title, message);

        if (options is not null)
        {
            // Execute the options callback to keep calling code compatible even though we ignore the values.
            options(new UiMessageOptions());
        }
    }
}
