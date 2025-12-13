using System;
using MudBlazor;

namespace cima.Blazor.Client.Services;

public interface ICimaThemeService
{
    MudTheme CurrentTheme { get; }
    MudTheme LightTheme { get; }
    MudTheme DarkTheme { get; }
    bool IsDarkMode { get; }
    event Action<MudTheme>? ThemeChanged;

    void UseLightTheme();
    void UseDarkTheme();
    void Toggle();
}

public class CimaThemeService : ICimaThemeService
{
    private MudTheme _currentTheme;

    public MudTheme LightTheme { get; }
    public MudTheme DarkTheme { get; }
    public MudTheme CurrentTheme => _currentTheme;
    public bool IsDarkMode { get; private set; }

    public event Action<MudTheme>? ThemeChanged;

    public CimaThemeService()
    {
        LightTheme = BuildLightTheme();
        DarkTheme = BuildDarkTheme();
        _currentTheme = LightTheme;
    }

    public void UseLightTheme()
    {
        if (!IsDarkMode)
        {
            return;
        }

        IsDarkMode = false;
        _currentTheme = LightTheme;
        NotifyChanged();
    }

    public void UseDarkTheme()
    {
        if (IsDarkMode)
        {
            return;
        }

        IsDarkMode = true;
        _currentTheme = DarkTheme;
        NotifyChanged();
    }

    public void Toggle()
    {
        if (IsDarkMode)
        {
            UseLightTheme();
            return;
        }

        UseDarkTheme();
    }

    private void NotifyChanged() => ThemeChanged?.Invoke(_currentTheme);

    private static MudTheme BuildLightTheme() =>
        new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#1e3a8a",
                Secondary = "#3f51c5",
                Info = "#0ea5e9",
                Success = "#22c55e",
                Warning = "#f59e0b",
                Error = "#dc2626",
                Background = "#f8f9fa",
                Surface = "#ffffff",
                AppbarBackground = "#1e3a8a",
                DrawerBackground = "#15255a",
                TextPrimary = "#0f172a",
                TextSecondary = "#475569",
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "10px",
            },
        };

    private static MudTheme BuildDarkTheme() =>
        new()
        {
            PaletteDark = new PaletteDark
            {
                Primary = "#60a5fa",
                Secondary = "#c7d2fe",
                Info = "#38bdf8",
                Success = "#34d399",
                Warning = "#f59e0b",
                Error = "#f87171",
                Background = "#0f172a",
                Surface = "#111827",
                AppbarBackground = "#0b1224",
                DrawerBackground = "#0b1224",
                TextPrimary = "#e2e8f0",
                TextSecondary = "#94a3b8",
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "10px",
            },
        };
}

