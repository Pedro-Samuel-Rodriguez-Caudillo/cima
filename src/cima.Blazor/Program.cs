using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace cima.Blazor;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        // Bootstrap logger para capturar errores de inicio
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("MachineName", Environment.MachineName)
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
            .WriteTo.Console()
            .WriteTo.File(
                new CompactJsonFormatter(),
                "Logs/bootstrap-.json",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting cima web host");
            var builder = WebApplication.CreateBuilder(args);
            
            // Configuración de puerto dinámico para Railway/Docker/Cloud
            var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            var port = Environment.GetEnvironmentVariable("PORT");
            
            if (!string.IsNullOrWhiteSpace(port) && string.IsNullOrWhiteSpace(urls))
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(int.Parse(port));
                });
                Log.Information("Using dynamic port from PORT env var: {Port}", port);
            }
            else if (!string.IsNullOrWhiteSpace(urls))
            {
                Log.Information("Using ASPNETCORE_URLS: {Urls}", urls);
            }
            else
            {
                Log.Information("Using launchSettings.json configuration (development)");
            }
            
            builder.Host
                .AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog((context, services, loggerConfiguration) =>
                {
                    var environment = context.HostingEnvironment.EnvironmentName;
                    
                    loggerConfiguration
                        // Nivel mínimo basado en entorno
                        .MinimumLevel.Is(context.HostingEnvironment.IsDevelopment() 
                            ? LogEventLevel.Debug 
                            : LogEventLevel.Information)
                        
                        // Overrides para reducir ruido
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                        .MinimumLevel.Override("System.Net.Http", LogEventLevel.Warning)
                        .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
                        
                        // Enrichers para contexto adicional
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Application", "cima")
                        .Enrich.WithProperty("Environment", environment)
                        .Enrich.WithProperty("MachineName", Environment.MachineName)
                        
                        // Consola con formato legible en desarrollo
                        .WriteTo.Console(
                            outputTemplate: context.HostingEnvironment.IsDevelopment()
                                ? "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
                                : "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                        
                        // Archivo JSON estructurado para análisis
                        .WriteTo.File(
                            new CompactJsonFormatter(),
                            "Logs/cima-.json",
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 30,
                            fileSizeLimitBytes: 50_000_000, // 50MB
                            rollOnFileSizeLimit: true)
                        
                        // Archivo de texto para lectura humana
                        .WriteTo.File(
                            "Logs/cima-.txt",
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 14,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                        
                        // ABP Studio integration
                        .WriteTo.Async(c => c.AbpStudio(services));
                });
                
            await builder.AddApplicationAsync<cimaBlazorModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
