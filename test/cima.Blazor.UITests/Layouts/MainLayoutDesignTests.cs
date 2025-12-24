using Bunit;
using Xunit;
using cima.Blazor.Client.Layouts;
using cima.Blazor.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;
using System;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using cima.Architects;
using Volo.Abp.Application.Services;
using Microsoft.Extensions.Localization;
using cima.Localization;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using cima.Blazor.Client.Components.Public;

namespace cima.Blazor.UITests.Layouts;

public class MainLayoutDesignTests : TestContext
{
    public MainLayoutDesignTests()
    {
        Services.AddMudServices();
        Services.AddScoped<ICimaThemeService, FakeThemeService>();
        Services.AddAuthorizationCore();
        Services.AddScoped<AuthenticationStateProvider, TestAuthStateProvider>();
        Services.AddScoped<IArchitectAppService, FakeArchitectAppService>();
        Services.AddScoped<MustChangePasswordStateService>();
        Services.AddLogging();
        Services.AddScoped<IStringLocalizer<cimaResource>, FakeLocalizer>();
        Services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        
        ComponentFactories.AddStub<Navbar>();
        ComponentFactories.AddStub<Footer>();
    }

    [Fact]
    public void MainLayout_Should_HaveTopPadding_ToPreventOverlap()
    {
        // Arrange
        JSInterop.Setup<int>("mudpopoverHelper.countProviders").SetResult(0);
        JSInterop.SetupVoid("watchDarkThemeMedia", _ => true);

        // Act
        var cut = RenderComponent<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder =>
            {
                builder.AddMarkupContent(0, "<div>Content</div>");
            })));

        // Assert
        // We are looking for a class that provides top padding or margin.
        // Based on Tailwind, usually 'pt-16' or 'mt-16' (4rem) or similar is used for sticky headers.
        var mainContainer = cut.Find("main#main-content");
        
        // Assert that the main container or its parent has padding-top or a margin-top class
        // This is a heuristic check. We expect some class that handles spacing.
        // Let's check if the header is sticky and main has padding.
        
        var header = cut.Find("header");
        Assert.Contains("sticky", header.ClassName);
        Assert.Contains("top-0", header.ClassName);

        // The requirement is to have margin/padding.
        // Let's assert that we added a class like 'pt-20' or 'mt-20' to the main element or a wrapper div.
        // Currently, it might fail if we haven't implemented it.
        // We will implement 'pt-20' (5rem = 80px) which is usually enough for a standard navbar.
        Assert.Contains("pt-20", cut.Markup); 
    }

    class FakeThemeService : ICimaThemeService
    {
        public MudTheme CurrentTheme { get; } = new MudTheme();
        public MudTheme LightTheme { get; } = new MudTheme();
        public MudTheme DarkTheme { get; } = new MudTheme();
        public bool IsDarkMode { get; } = false;
        public event Action<MudTheme>? ThemeChanged;
        public void Toggle() { }
        public void UseLightTheme() { }
        public void UseDarkTheme() { }
    }

    class TestAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }
    
    class FakeArchitectAppService : IArchitectAppService
    {
        public Task<bool> MustChangePasswordAsync(System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task ChangePasswordAsync(ChangeArchitectPasswordDto input, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ArchitectDto> CreateAsync(CreateArchitectDto input, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<CreateArchitectResultDto> CreateWithUserAsync(CreateArchitectWithUserDto input, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ArchitectDto> GetAsync(Guid id, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ArchitectDto> GetByUserIdAsync(Guid userId, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ArchitectDto?> GetCurrentAsync(System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Volo.Abp.Application.Dtos.PagedResultDto<ArchitectDto>> GetListAsync(Volo.Abp.Application.Dtos.PagedAndSortedResultRequestDto input, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<string> ResetPasswordAsync(Guid id, ResetArchitectPasswordDto input, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<ArchitectDto> UpdateAsync(Guid id, UpdateArchitectDto input, System.Threading.CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    class FakeLocalizer : IStringLocalizer<cimaResource>
    {
        public LocalizedString this[string name] => new LocalizedString(name, name);
        public LocalizedString this[string name, params object[] arguments] => new LocalizedString(name, string.Format(name, arguments));
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => throw new NotImplementedException();
    }
}
