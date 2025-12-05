using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using cima.Blazor.Client.Services;
using cima.Blazor.Client.Authentication;
using cima.Localization;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.AutoMapper;
using Volo.Abp.Localization;
using Volo.Abp.AspNetCore.Components.Web;
using Volo.Abp.Modularity;
using MudBlazor.Services;


namespace cima.Blazor.Client;

[DependsOn(
    typeof(AbpAutofacWebAssemblyModule),
    typeof(cimaHttpApiClientModule),
    typeof(AbpAspNetCoreComponentsWebModule)
)]
public class cimaBlazorClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();

        ConfigureAuthentication(context);
        ConfigureHttpClient(context, environment);
        
        ConfigureMudBlazor(context);
        ConfigureAutoMapper(context);
        ConfigureLocalization();
        ConfigureApplicationServices(context);
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();
        context.Services.AddAuthorizationCore();
    }

    private void ConfigureApplicationServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<EnumLocalizationService>();
        context.Services.AddSingleton<SeoJsonLdService>();
        
        // Toast service con accesibilidad
        context.Services.AddCimaToastService();
    }

    private void ConfigureLocalization()
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<cimaResource>();

            options.Languages.Clear();
            options.Languages.Add(new LanguageInfo("es", "es", "Español"));
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
        });
    }

    private void ConfigureMudBlazor(ServiceConfigurationContext context)
    {
        context.Services.AddMudServices();
    }

    private static void ConfigureHttpClient(ServiceConfigurationContext context, IWebAssemblyHostEnvironment environment)
    {
        context.Services.AddTransient(sp => new HttpClient
        {
            BaseAddress = new Uri(environment.BaseAddress)
        });
    }

    private void ConfigureAutoMapper(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<cimaBlazorClientModule>();
        });
    }
}
