using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using cima.Images;
using cima.Notifications;

namespace cima;

[DependsOn(
    typeof(cimaDomainModule),
    typeof(cimaApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class cimaApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<cimaApplicationModule>();
        });

        Configure<ImageStorageOptions>(options =>
        {
            configuration.GetSection("ImageStorage").Bind(options);
        });

        context.Services.AddEmailNotificationService(configuration);

        context.Services.AddTransient<LocalImageStorageService>();
        context.Services.AddTransient<AzureBlobImageStorageService>();
        context.Services.Replace(ServiceDescriptor.Transient<IImageStorageService>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<ImageStorageOptions>>().Value;
            var provider = opts.Provider?.Trim().ToLowerInvariant() ?? "azureblob";
            return provider switch
            {
                "local" => sp.GetRequiredService<LocalImageStorageService>(),
                _ => sp.GetRequiredService<AzureBlobImageStorageService>()
            };
        }));
    }
}
