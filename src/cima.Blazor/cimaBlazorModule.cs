using cima.Blazor.Client;
using cima.Blazor.Client.Navigation;
using cima.Blazor.Components;
using cima.Blazor.HealthChecks;
using cima.Blazor.Services;
using cima.Blazor.Infrastructure.Security;
using MudBlazor.Services;
using cima.Blazor.Infrastructure;
using cima.Data;
using cima.EntityFrameworkCore;
using cima.Localization;
using cima.MultiTenancy;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Components.Web;
using Volo.Abp.AspNetCore.Components.Web.Theming.Routing;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.AutoMapper;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.AspNetCore.Components.Messages;
using cima.Blazor.Client.Services;
using Volo.Abp.Data;
using Asp.Versioning;
using Swashbuckle.AspNetCore.SwaggerGen;
using Volo.Abp.AspNetCore.SignalR;

namespace cima.Blazor;

[DependsOn(
    typeof(cimaApplicationModule),
    typeof(cimaEntityFrameworkCoreModule),
    typeof(cimaHttpApiModule),
    typeof(AbpAutofacModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreMvcUiBasicThemeModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpAspNetCoreSignalRModule)
    // Note: ABP Blazor WebAssembly modules are registered in cimaBlazorClientModule
   )]
public class cimaBlazorModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(cimaResource),
                typeof(cimaDomainModule).Assembly,
                typeof(cimaDomainSharedModule).Assembly,
                typeof(cimaApplicationModule).Assembly,
                typeof(cimaApplicationContractsModule).Assembly,
                typeof(cimaBlazorModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("cima");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        PreConfigure<OpenIddictServerBuilder>(builder =>
        {
            builder
                .AllowAuthorizationCodeFlow()
                .RequireProofKeyForCodeExchange()
                .AllowRefreshTokenFlow();
        });



        if (hostingEnvironment.IsProduction())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
        else if (hostingEnvironment.IsStaging())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = true;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }

        PreConfigure<AbpAspNetCoreComponentsWebOptions>(options =>
        {
            options.IsBlazorWebApp = true;
        });
    }
    
    private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim().TrimEnd('/'))
                            .ToArray() ?? Array.Empty<string>()
                    )
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        // Add Razor Pages for ABP Account MVC pages (Login, Register, etc.)
        context.Services.AddRazorPages();
        
        // Add HttpClient for ImageProxyController
        context.Services.AddHttpClient();

        context.Services.AddCimaImprovements(configuration, hostingEnvironment);

        // ========================================
        // CONFIGURACIÓN DE MANEJO DE EXCEPCIONES
        // ========================================
        // No loggear OperationCanceledException como error (es normal cuando el usuario cancela)
        Configure<AbpExceptionHandlingOptions>(options =>
        {
            options.SendExceptionsDetailsToClients = hostingEnvironment.IsDevelopment();
            options.SendStackTraceToClients = false;
        });

        // Registrar finder personalizado para mapear códigos de error de negocio a HTTP status codes
        // Ej: Listing:ConcurrencyConflict -> 409 Conflict
        context.Services.AddTransient<IHttpExceptionStatusCodeFinder, CimaHttpExceptionStatusCodeFinder>();

        // Add services to the container.
        context.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        // Aumentar límite de tamaño para mensajes SignalR (uploads de imágenes en Blazor Server)
        context.Services.AddSignalR(options =>
        {
            // 10MB por mensaje permite imágenes base64 de hasta 5MB sin timeouts
            options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
            options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        });

        // Configurar límites de request body para API calls (uploads de imágenes)
        context.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            // Permitir archivos de hasta 10MB (para múltiples imágenes de 5MB cada una)
            options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
            options.ValueLengthLimit = 10 * 1024 * 1024;
            options.MultipartHeadersLengthLimit = 10 * 1024 * 1024;
        });

        // Configurar Kestrel para aceptar requests grandes
        context.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
        {
            options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
            options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(2);
        });

        // Added MudBlazor
        context.Services.AddMudServices();
        context.Services.AddScoped<ICimaThemeService, CimaThemeService>();
        context.Services.AddCimaClientCache();

        // Configurar CascadingAuthenticationState para Blazor Web App
        context.Services.AddCascadingAuthenticationState();

        Configure<AbpAntiForgeryOptions>(options =>
        {
            options.AutoValidate = true;
            options.TokenCookie.SecurePolicy = CookieSecurePolicy.Always;
            options.TokenCookie.SameSite = SameSiteMode.None;
        });

        // ========================================
        // CONFIGURACIÓN DE SEGURIDAD AVANZADA
        // ========================================
        
        // Bloqueo de cuenta después de intentos fallidos
        Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(options =>
        {
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // Password settings (más estrictos)
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;

            // User settings
            options.User.RequireUniqueEmail = true;

            // SignIn settings
            options.SignIn.RequireConfirmedEmail = false; // Cambiar a true si quieres verificación obligatoria
        });

        // Google Authentication (OAuth2)
        var googleClientId = configuration["Authentication:Google:ClientId"];
        var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        {
            context.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.CallbackPath = "/signin-google";
                });
        }

        // ========================================
        // CONFIGURACIÓN DE DATA PROTECTION (PERSISTENCIA DE LLAVES)
        // ========================================
        if (!hostingEnvironment.IsDevelopment())
        {
            var dataProtectionBuilder = context.Services.AddDataProtection()
                .SetApplicationName("cima");

            var azureConnectionString = configuration["ImageStorage:Azure:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(azureConnectionString))
            {
                // Persistir llaves en Azure Blob Storage para evitar errores de Antiforgery al reiniciar
                dataProtectionBuilder.PersistKeysToAzureBlobStorage(azureConnectionString, "data-protection-keys", "keys.xml");
            }
        }

        // ========================================
        // CONFIGURACIÓN DE SEGURIDAD DE IDENTITY
        // ========================================
        context.Services.ConfigureCimaIdentityOptions();
        context.Services.ConfigureCimaAuthCookies();
        context.Services.ConfigureCimaSecurityStamp();
        
        // ========================================
        // CONFIGURACIÓN DE ABP ACCOUNT - LOGIN LOCAL
        // ========================================
        // Habilitar login local explícitamente para que el formulario se muestre
        Configure<AbpAccountOptions>(options =>
        {
            options.WindowsAuthenticationSchemeName = null; // No usar Windows Auth
        });

        if (!configuration.GetValue<bool>("App:DisablePII"))
        {
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;
        }

        if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(options =>
            {
                options.DisableTransportSecurityRequirement = true;
            });
        }

        ConfigureCors(context, configuration);
        ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles(hostingEnvironment);
        ConfigureAutoMapper();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureSwaggerServices(context.Services);
        ConfigureAutoApiControllers();
        
        ConfigureRouter(context);
        ConfigureMenu(context);
        ConfigureHealthChecks(context, configuration);
        ConfigureApplicationServices(context);
        
        // Registrar el HostedService para seeding en Development
        if (hostingEnvironment.IsDevelopment())
        {
            context.Services.AddHostedService<DevelopmentDataSeedingService>();
        }
    }

    private void ConfigureApplicationServices(ServiceConfigurationContext context)
    {
        // Registrar EnumLocalizationService para renderizado en servidor
        context.Services.AddTransient<Client.Services.EnumLocalizationService>();
        
        // Registrar LoginRedirectService para paginas de post-login
        context.Services.AddScoped<Client.Services.ILoginRedirectService, Client.Services.LoginRedirectService>();
        
        // SEO helpers usados en prerender (paginas publicas)
        context.Services.AddSingleton<Client.Services.SeoJsonLdService>();

        // Mensajeria UI (toast/dialog) utilizada por ContactForm y otros componentes prerenderizados
        context.Services.AddTransient<IUiMessageService, FallbackUiMessageService>();
    }
    
    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
            options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"]?.Split(',') ?? Array.Empty<string>());
        });
    }

    private void ConfigureBundles(IWebHostEnvironment hostingEnvironment)
    {
        Configure<AbpBundlingOptions>(options =>
        {
            // Blazor Web App
            options.Parameters.InteractiveAuto = true;

            // En staging/production, deshabilitar pre-bundling para evitar errores de archivos faltantes
            // Los bundles se generarn bajo demanda
            if (hostingEnvironment.IsStaging() || hostingEnvironment.IsProduction())
            {
                // Configurar para modo minificado en produccin
                options.Mode = BundlingMode.BundleAndMinify;
            }

            // MVC UI
            options.StyleBundles.Configure(
                BasicThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );

            options.ScriptBundles.Configure(
                BasicThemeBundles.Scripts.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-scripts.js");
                }
            );
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<cimaDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}cima.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<cimaDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}cima.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<cimaApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}cima.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<cimaApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}cima.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<cimaBlazorModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "cima API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) =>
                {
                    if (!description.TryGetMethodInfo(out var methodInfo))
                    {
                        return false;
                    }

                    var versions = methodInfo.DeclaringType?
                        .GetCustomAttributes(true)
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions)
                        .ToList();

                    if (versions == null || !versions.Any())
                    {
                        return docName == "v1";
                    }

                    return versions.Any(v => $"v{v}" == docName);
                });
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }

    private void ConfigureMenu(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new Navigation.cimaMenuContributor(context.Services.GetConfiguration()));
        });
    }

    private void ConfigureRouter(ServiceConfigurationContext context)
    {
        Configure<AbpRouterOptions>(options =>
        {
            options.AppAssembly = typeof(cimaBlazorModule).Assembly;
            options.AdditionalAssemblies.Add(typeof(cimaBlazorClientModule).Assembly);
        });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(cimaApplicationModule).Assembly);
        });
    }

    private void ConfigureAutoMapper()
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<cimaBlazorModule>();
        });
    }

    private void ConfigureHealthChecks(ServiceConfigurationContext context, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        
        // Registrar health check de startup como singleton
        context.Services.AddSingleton<HealthChecks.StartupHealthCheck>();
        
        // Registrar background service que marca cuando la app esta lista
        context.Services.AddHostedService<HealthChecks.StartupBackgroundService>();
        
        var healthChecksBuilder = context.Services.AddHealthChecks();

        // 1. LIVENESS CHECK - Verifica que la app esta viva
        //    No requiere dependencias externas
        healthChecksBuilder.AddCheck(
            "self",
            () => HealthCheckResult.Healthy("Aplicacion esta corriendo"),
            tags: new[] { "live" }
        );

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Si no hay connection string, marcar como unhealthy en readiness
            healthChecksBuilder.AddCheck(
                "database-configuration",
                () => HealthCheckResult.Unhealthy("Connection string 'Default' no esta configurada"),
                tags: new[] { "ready" }
            );
            return;
        }

        // 2. READINESS CHECK - Verifica que la app esta lista para recibir trafico
        
        // a) Verifica que completo el startup
        healthChecksBuilder.AddCheck<HealthChecks.StartupHealthCheck>(
            "startup",
            tags: new[] { "ready" }
        );
        
        // b) Verifica PostgreSQL connection
        healthChecksBuilder.AddNpgSql(
            connectionString: connectionString,
            name: "postgresql-connection",
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "ready", "db" },
            timeout: TimeSpan.FromSeconds(3)
        );
        
        // c) Verifica base de datos y migraciones
        healthChecksBuilder.AddCheck<HealthChecks.DatabaseHealthCheck>(
            "database-migrations",
            failureStatus: HealthStatus.Degraded,
            tags: new[] { "ready", "db" }
        );
    }
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var env = context.GetEnvironment();
        var app = context.GetApplicationBuilder();
        var configuration = context.GetConfiguration();
        
        // EJECUTAR MIGRACIONES SOLO EN STAGING/PRODUCTION
        // En Development, el seeding se ejecuta a travs de DevelopmentDataSeedingService
        if (env.IsStaging() || env.IsProduction())
        {
            // En staging/production: ejecutar migraciones y seeding (BLOQUEA HASTA COMPLETAR)
            ExecutarMigracionesAsync(context.ServiceProvider, env).GetAwaiter().GetResult();
        }

        // Respetar encabezados enviados por el proxy (X-Forwarded-Proto/Host)
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
        });
        
        app.Use(async (ctx, next) =>
        {
            /* Converting to https to be able to include https URLs in `/.well-known/openid-configuration` endpoint.
             * This should only be done if the request is coming outside of the cluster.  */
            if (ctx.Request.Headers.ContainsKey("from-ingress"))
            {
                ctx.Request.Scheme = "https";
            }

            await next();
        });
        
        // Permitir Railway healthchecks - debe ir ANTES de cualquier middleware de autenticacin
        app.Use(async (ctx, next) =>
        {
            // Railway healthcheck bypass - permitir sin autenticacin ni CORS
            if (ctx.Request.Path.StartsWithSegments("/api/health/ping") ||
                ctx.Request.Path.StartsWithSegments("/api/health") ||
                ctx.Request.Path.StartsWithSegments("/health"))
            {
                // Permitir cualquier host para healthchecks (incluyendo healthcheck.railway.app)
                ctx.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                await next();
                return;
            }
            
            await next();
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
            app.UseHsts();
        }

        app.UseCorrelationId();
        app.UseCors();
        app.UseRouting();
        
        // Mejoras: Rate Limiting, OpenTelemetry
        app.UseCimaImprovements(configuration);
        
        if (Convert.ToBoolean(configuration["AuthServer:IsOnK8s"]))
        {
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.Value != null &&
                    context.Request.Path.Value.StartsWith("/appsettings", StringComparison.OrdinalIgnoreCase) &&
                    context.Request.Path.Value.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    // Set endpoint to null so the static files middleware will handle the request.
                    context.SetEndpoint(null);
                }
                await next(context);
            });

            app.UseStaticFilesForPatterns("appsettings*.json");
        }
        
        app.MapAbpStaticAssets();
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAntiforgery();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "cima API");
        });
        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        
        app.UseConfiguredEndpoints(endpoints =>
        {
            // Map Razor Pages for ABP Account (Login, Register, etc.)
            endpoints.MapRazorPages();
            
            // Map API Controllers
            endpoints.MapControllers();
            
            // Mapear Razor Components para Blazor Web App
            endpoints.MapRazorComponents<Components.App>()
                .AddInteractiveServerRenderMode()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(
                    typeof(cimaBlazorClientModule).Assembly,
                    typeof(Volo.Abp.Identity.Blazor.AbpIdentityBlazorModule).Assembly,
                    typeof(Volo.Abp.PermissionManagement.Blazor.AbpPermissionManagementBlazorModule).Assembly,
                    typeof(Volo.Abp.SettingManagement.Blazor.AbpSettingManagementBlazorModule).Assembly
                );
            
            // SignalR Hub para notificaciones en tiempo real
            endpoints.MapHub<cima.Hubs.CimaNotificationHub>("/hubs/notifications");
            
            // Health check endpoints
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        timestamp = DateTime.UtcNow,
                        checks = report.Entries.Select(e => new
                        {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            duration = e.Value.Duration.TotalMilliseconds
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            });
            
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready")
            });
            
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live")
            });
        });
    }

    private async Task ExecutarMigracionesAsync(IServiceProvider serviceProvider, IWebHostEnvironment env)
    {
        if (!env.IsStaging() && !env.IsProduction())
        {
            return;
        }
        
        var logger = serviceProvider.GetRequiredService<ILogger<cimaBlazorModule>>();
        
        try
        {
            logger.LogInformation("=== INICIANDO MIGRACIONES AUTOMATICAS ===");
            
            using (var scope = serviceProvider.CreateScope())
            {
                var migrator = scope.ServiceProvider.GetRequiredService<IcimaDbSchemaMigrator>();
                
                logger.LogInformation("Ejecutando MigrateAsync()...");
                await migrator.MigrateAsync();
                logger.LogInformation("MigrateAsync() completado");

                logger.LogInformation("Ejecutando SeedDataAsync()...");
                var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
                await dataSeeder.SeedAsync(new DataSeedContext()
                    .WithProperty(IdentityDataSeedContributor.AdminEmailPropertyName, cimaConsts.AdminEmailDefaultValue)
                    .WithProperty(IdentityDataSeedContributor.AdminPasswordPropertyName, cimaConsts.AdminPasswordDefaultValue));
                logger.LogInformation("SeedDataAsync() completado");
            }
            
            logger.LogInformation("=== MIGRACIONES COMPLETADAS EXITOSAMENTE ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante la migracion automatica");
            throw;
        }
    }
}
