using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using cima.Data;
using Volo.Abp.DependencyInjection;

namespace cima.EntityFrameworkCore;

public class EntityFrameworkCorecimaDbSchemaMigrator
    : IcimaDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCorecimaDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the cimaDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        var dbContext = _serviceProvider.GetRequiredService<cimaDbContext>();
        
        var connectionString = dbContext.Database.GetConnectionString();
        Console.WriteLine($"[MIGRATION] Connection String: {MaskConnectionString(connectionString)}");
        
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        Console.WriteLine($"[MIGRATION] Pending Migrations: {pendingMigrations.Count()}");
        
        foreach (var migration in pendingMigrations)
        {
            Console.WriteLine($"[MIGRATION]   - {migration}");
        }
        
        Console.WriteLine("[MIGRATION] Ejecutando Database.MigrateAsync()...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("[MIGRATION] MigrateAsync() completado");
        
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
        Console.WriteLine($"[MIGRATION] Applied Migrations: {appliedMigrations.Count()}");
    }
    
    private string MaskConnectionString(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return "[NULL]";
        }
        
        // Ocultar password pero mostrar host y database
        var parts = connectionString.Split(';');
        var masked = string.Join(";", parts.Select(p =>
        {
            if (p.Contains("Password=", StringComparison.OrdinalIgnoreCase))
            {
                return "Password=***";
            }
            return p;
        }));
        
        return masked;
    }
}
