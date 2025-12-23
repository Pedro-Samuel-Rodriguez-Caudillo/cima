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

public class MainLayoutTests : TestContext
{
    public MainLayoutTests()
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
    public void MainLayout_Should_ShowErrorUI_When_ChildThrows()
    {
        // Arrange
        JSInterop.Setup<int>("mudpopoverHelper.countProviders").SetResult(0);
        JSInterop.SetupVoid("watchDarkThemeMedia", _ => true);
        
        // We render a component inside the layout that throws
        var cut = RenderComponent<MainLayout>(parameters => parameters
            .Add(p => p.Body, (RenderFragment)(builder =>
            {
                builder.OpenComponent<ThrowingComponent>(0);
                builder.CloseComponent();
            })));

        // Act & Assert
        // Expecting "Something went wrong" or similar friendly message
        Assert.Contains("Something went wrong", cut.Markup); 
    }

    class ThrowingComponent : ComponentBase
    {
        protected override void OnInitialized()
        {
            throw new Exception("Test Exception");
        }
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
        public Task<bool> MustChangePasswordAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

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


