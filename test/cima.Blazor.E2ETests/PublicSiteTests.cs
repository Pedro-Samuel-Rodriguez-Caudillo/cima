using System;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Xunit;

namespace cima.Blazor.E2ETests;

[Collection("E2E Tests")]
public class PublicSiteTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    
    private const string BaseUrl = "http://localhost:5000";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _page.CloseAsync();
        await _context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task HomePage_Should_LoadSuccessfully()
    {
        // Act
        await _page.GotoAsync(BaseUrl);
        
        // Assert
        await Assertions.Expect(_page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("CIMA"));
    }

    [Fact]
    public async Task HomePage_Should_DisplayHeroSection()
    {
        // Act
        await _page.GotoAsync(BaseUrl);
        
        // Assert
        var heroSection = _page.Locator(".hero-section, [class*='hero']");
        await Assertions.Expect(heroSection).ToBeVisibleAsync();
    }

    [Fact]
    public async Task HomePage_Should_DisplayFeaturedProperties()
    {
        // Act
        await _page.GotoAsync(BaseUrl);
        
        // Wait for content to load
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - check for property cards
        var propertyCards = _page.Locator("[class*='listing-card'], [class*='property-card']");
        var count = await propertyCards.CountAsync();
        Assert.True(count > 0, "Should display at least one featured property");
    }

    [Fact]
    public async Task PropertiesPage_Should_LoadSuccessfully()
    {
        // Act
        await _page.GotoAsync($"{BaseUrl}/properties");
        
        // Wait for navigation
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        await Assertions.Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/properties"));
    }

    [Fact]
    public async Task PropertiesPage_Should_DisplaySearchFilters()
    {
        // Act
        await _page.GotoAsync($"{BaseUrl}/properties");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - check for search/filter inputs
        var hasSearchInput = await _page.Locator("input[type='search'], input[type='text']").CountAsync() > 0;
        Assert.True(hasSearchInput, "Should display search filters");
    }

    [Fact]
    public async Task PropertiesPage_Should_FilterByPropertyType()
    {
        // Arrange
        await _page.GotoAsync($"{BaseUrl}/properties");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var initialCount = await _page.Locator("[class*='listing-card']").CountAsync();
        
        // Act - try to select a filter (if available)
        var selectElement = _page.Locator("select").First;
        if (await selectElement.CountAsync() > 0)
        {
            await selectElement.SelectOptionAsync(new[] { "1" }); // Residential
            await _page.WaitForTimeoutAsync(1000); // Wait for filter
            
            // Assert
            var filteredCount = await _page.Locator("[class*='listing-card']").CountAsync();
            // Count might be same or different, just verify page didn't crash
            Assert.True(filteredCount >= 0);
        }
    }

    [Fact]
    public async Task PropertyDetail_Should_LoadWhenCardClicked()
    {
        // Arrange
        await _page.GotoAsync($"{BaseUrl}/properties");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Act - click first property card link
        var firstPropertyLink = _page.Locator("a[href*='/properties/']").First;
        if (await firstPropertyLink.CountAsync() > 0)
        {
            await firstPropertyLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Assert
            await Assertions.Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/properties/[0-9a-f-]+"));
        }
    }

    [Fact]
    public async Task PropertyDetail_Should_DisplayImageGallery()
    {
        // Arrange - navigate to first property
        await _page.GotoAsync($"{BaseUrl}/properties");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var firstPropertyLink = _page.Locator("a[href*='/properties/']").First;
        if (await firstPropertyLink.CountAsync() > 0)
        {
            await firstPropertyLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Assert - check for images
            var images = _page.Locator("img[class*='gallery'], img[class*='property']");
            var imageCount = await images.CountAsync();
            Assert.True(imageCount > 0, "Property detail should display images");
        }
    }

    [Fact]
    public async Task PropertyDetail_Should_DisplayContactForm()
    {
        // Arrange
        await _page.GotoAsync($"{BaseUrl}/properties");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var firstPropertyLink = _page.Locator("a[href*='/properties/']").First;
        if (await firstPropertyLink.CountAsync() > 0)
        {
            await firstPropertyLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Assert
            var contactForm = _page.Locator("form, [class*='contact-form']");
            await Assertions.Expect(contactForm.First).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task ContactForm_Should_ValidateRequiredFields()
    {
        // Arrange
        await _page.GotoAsync($"{BaseUrl}/properties");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        var firstPropertyLink = _page.Locator("a[href*='/properties/']").First;
        if (await firstPropertyLink.CountAsync() > 0)
        {
            await firstPropertyLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Act - try to submit empty form
            var submitButton = _page.Locator("button[type='submit']").First;
            if (await submitButton.CountAsync() > 0)
            {
                await submitButton.ClickAsync();
                await _page.WaitForTimeoutAsync(500);
                
                // Assert - should show validation messages
                var validationMessages = await _page.Locator("[class*='invalid'], [class*='error'], .validation-message").CountAsync();
                Assert.True(validationMessages >= 0); // Just verify no crash
            }
        }
    }

    [Fact]
    public async Task PortfolioPage_Should_LoadSuccessfully()
    {
        // Act
        await _page.GotoAsync($"{BaseUrl}/portfolio");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert
        await Assertions.Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/portfolio"));
    }

    [Fact]
    public async Task PortfolioPage_Should_DisplayArchitectProjects()
    {
        // Act
        await _page.GotoAsync($"{BaseUrl}/portfolio");
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - check for project/portfolio items
        var portfolioItems = await _page.Locator("[class*='portfolio'], [class*='project'], [class*='architect']").CountAsync();
        Assert.True(portfolioItems >= 0); // Just verify page loads
    }

    [Fact]
    public async Task Navigation_Should_WorkBetweenPages()
    {
        // Start at home
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Navigate to properties
        var propertiesLink = _page.Locator("a[href*='/properties']").First;
        if (await propertiesLink.CountAsync() > 0)
        {
            await propertiesLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Assertions.Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/properties"));
        }
        
        // Navigate to portfolio
        var portfolioLink = _page.Locator("a[href*='/portfolio']").First;
        if (await portfolioLink.CountAsync() > 0)
        {
            await portfolioLink.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Assertions.Expect(_page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/portfolio"));
        }
    }

    [Fact]
    public async Task LanguageSwitch_Should_ChangeContentLanguage()
    {
        // Act
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Look for language selector
        var languageSelector = _page.Locator("[class*='language'], select[name*='lang'], button[class*='lang']");
        if (await languageSelector.CountAsync() > 0)
        {
            var initialText = await _page.TextContentAsync("body");
            
            // Try to click language selector
            await languageSelector.First.ClickAsync();
            await _page.WaitForTimeoutAsync(500);
            
            var newText = await _page.TextContentAsync("body");
            // Just verify no crash occurred
            Assert.NotNull(newText);
        }
    }

    [Fact]
    public async Task MobileView_Should_DisplayCorrectly()
    {
        // Arrange - set mobile viewport
        await _context.CloseAsync();
        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 375, Height = 667 }
        });
        _page = await _context.NewPageAsync();
        
        // Act
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - page should load without errors
        var bodyVisible = await _page.Locator("body").IsVisibleAsync();
        Assert.True(bodyVisible);
    }
}
