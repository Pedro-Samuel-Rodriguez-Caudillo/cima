using Microsoft.Playwright;
using System;
using System.Threading.Tasks;
using Xunit;

namespace cima.Blazor.E2ETests;

/// <summary>
/// Base class for E2E tests with Playwright setup
/// </summary>
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;
    
    protected virtual string BaseUrl => Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:5000";
    protected virtual bool Headless => true;

    public virtual async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Headless
        });
        Context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            ScreenSize = new ScreenSize { Width = 1920, Height = 1080 }
        });
        Page = await Context.NewPageAsync();
    }

    public virtual async Task DisposeAsync()
    {
        if (Page != null) await Page.CloseAsync();
        if (Context != null) await Context.CloseAsync();
        if (Browser != null) await Browser.CloseAsync();
        Playwright?.Dispose();
    }

    protected async Task NavigateToAsync(string path)
    {
        var url = path.StartsWith("http") ? path : $"{BaseUrl}{path}";
        await Page.GotoAsync(url);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    protected async Task<bool> IsElementVisibleAsync(string selector)
    {
        try
        {
            return await Page.Locator(selector).IsVisibleAsync();
        }
        catch
        {
            return false;
        }
    }

    protected async Task WaitForElementAsync(string selector, int timeoutMs = 5000)
    {
        await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            Timeout = timeoutMs
        });
    }
}
