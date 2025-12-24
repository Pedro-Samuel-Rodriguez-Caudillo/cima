using System;
using System.Collections.Generic;
using System.Timers;

namespace cima.Blazor.Client.Services.Notifications;

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public class ToastMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Message { get; set; } = string.Empty;
    public ToastLevel Level { get; set; } = ToastLevel.Info;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class CimaToastService : IDisposable
{
    public event Action? OnChange;
    public List<ToastMessage> Messages { get; private set; } = new();
    
    private readonly Timer _countdown;

    public CimaToastService()
    {
        _countdown = new Timer(5000);
        _countdown.Elapsed += RemoveExpiredToasts;
        _countdown.AutoReset = true;
    }

    public void ShowInfo(string message) => ShowToast(message, ToastLevel.Info);
    public void ShowSuccess(string message) => ShowToast(message, ToastLevel.Success);
    public void ShowWarning(string message) => ShowToast(message, ToastLevel.Warning);
    public void ShowError(string message) => ShowToast(message, ToastLevel.Error);

    public void ShowToast(string message, ToastLevel level)
    {
        Messages.Add(new ToastMessage
        {
            Message = message,
            Level = level
        });
        
        if (!_countdown.Enabled)
        {
            _countdown.Start();
        }
        
        NotifyStateChanged();
    }

    public void RemoveToast(Guid id)
    {
        var toast = Messages.Find(x => x.Id == id);
        if (toast != null)
        {
            Messages.Remove(toast);
            NotifyStateChanged();
        }
    }

    private void RemoveExpiredToasts(object? sender, ElapsedEventArgs e)
    {
        if (Messages.Count == 0)
        {
            _countdown.Stop();
            return;
        }

        // Simplification: remove oldest toast on tick
        // In a real app, we would check CreatedAt + Duration
        if (Messages.Count > 0)
        {
            Messages.RemoveAt(0);
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();

    public void Dispose()
    {
        _countdown?.Dispose();
    }
}
