using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EventBus;
using Volo.Abp.Emailing;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.Azure;
using Volo.Abp.TextTemplating.Scriban;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using cima.Images;
using cima.Notifications;
using cima.BackgroundWorkers;

namespace cima;

[DependsOn(
    typeof(cimaDomainModule),
    typeof(cimaApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpBackgroundJobsModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpEventBusModule),
    typeof(AbpEmailingModule),
    typeof(AbpBlobStoringAzureModule),
    typeof(AbpTextTemplatingScribanModule)
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
        context.Services.AddTransient<AbpBlobImageStorageService>();
        context.Services.Replace(ServiceDescriptor.Transient<IImageStorageService>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<ImageStorageOptions>>().Value;
            var provider = opts.Provider?.Trim().ToLowerInvariant() ?? "azureblob";
            return provider switch
            {
                "local" => sp.GetRequiredService<LocalImageStorageService>(),
                "abpblob" => sp.GetRequiredService<AbpBlobImageStorageService>(),
                _ => sp.GetRequiredService<AzureBlobImageStorageService>()
            };
        }));

        // Configurar Azure BlobStoring
        Configure<AbpBlobStoringOptions>(options =>
        {
            options.Containers.ConfigureDefault(container =>
            {
                container.UseAzure(azure =>
                {
                    azure.ConnectionString = configuration["ImageStorage:Azure:ConnectionString"] ?? string.Empty;
                    azure.ContainerName = configuration["ImageStorage:Azure:ContainerName"] ?? "listing-images";
                });
            });
        });
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        // Registrar BackgroundWorker para limpieza de drafts
        await context.AddBackgroundWorkerAsync<DraftCleanupWorker>();
    }
}
