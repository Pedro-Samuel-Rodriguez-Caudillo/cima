using System;
using System.Threading.Tasks;
using cima.Domain.Entities;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace cima.Application.Data;

/// <summary>
/// Seeder de datos de prueba para desarrollo
/// Solo crea usuarios fijos (admin + arquitecto)
/// Las propiedades se crean manualmente desde el panel de admin
/// </summary>
public class DevelopmentDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    public DevelopmentDataSeeder(
        IRepository<Architect, Guid> architectRepository,
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        IGuidGenerator guidGenerator,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _architectRepository = architectRepository;
        _userRepository = userRepository;
        _userManager = userManager;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        
        _logger.LogInformation("========================================");
        _logger.LogInformation("[SEEDER] DevelopmentDataSeeder.SeedAsync() INICIADO");
        _logger.LogInformation($"[SEEDER] Entorno actual: {environment}");
        _logger.LogInformation("========================================");
        
        if (environment == "Production")
        {
            _logger.LogInformation("[SEEDER] Saltando seeding (Entorno Production)");
            return;
        }

        try
        {
            // Crear usuarios fijos
            _logger.LogInformation("[SEEDER] Creando usuarios...");
            await SeedUsersAndArchitect();

            _logger.LogInformation("========================================");
            _logger.LogInformation("[SEEDER] SEEDING COMPLETADO EXITOSAMENTE");
            _logger.LogInformation("[SEEDER] Resumen:");
            _logger.LogInformation("[SEEDER]    - 1 Admin: admin@cima.com / 1q2w3E*");
            _logger.LogInformation("[SEEDER]    - 1 Arquitecto: arq@cima.com / 1q2w3E*");
            _logger.LogInformation("[SEEDER]    - 0 Propiedades (crear desde admin)");
            _logger.LogInformation("========================================");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SEEDER] Error durante el seeding");
            throw;
        }
    }

    private async Task SeedUsersAndArchitect()
    {
        // Admin user
        var admin = await _userRepository.FindByNormalizedUserNameAsync("ADMIN@CIMA.COM");
        if (admin == null)
        {
            admin = new IdentityUser(_guidGenerator.Create(), "admin@cima.com", "admin@cima.com")
            {
                Name = "Admin",
                Surname = "CIMA"
            };
            await _userManager.CreateAsync(admin, "1q2w3E*");
            await _userManager.AddToRoleAsync(admin, "admin");
            _logger.LogInformation("Created admin user: admin@cima.com / 1q2w3E*");
        }

        // Architect user
        var architectUser = await _userRepository.FindByNormalizedUserNameAsync("ARQ@CIMA.COM");
        if (architectUser == null)
        {
            architectUser = new IdentityUser(_guidGenerator.Create(), "arq@cima.com", "arq@cima.com")
            {
                Name = "Juan",
                Surname = "Arquitecto"
            };
            await _userManager.CreateAsync(architectUser, "1q2w3E*");
            _logger.LogInformation("Created architect user: arq@cima.com / 1q2w3E*");
        }

        // Architect profile
        var architectProfile = await _architectRepository.FirstOrDefaultAsync(a => a.UserId == architectUser.Id);
        if (architectProfile == null)
        {
            architectProfile = new Architect
            {
                UserId = architectUser.Id,
                TotalListingsPublished = 0,
                ActiveListings = 0,
                RegistrationDate = DateTime.UtcNow.AddDays(-90),
                IsActive = true
            };
            await _architectRepository.InsertAsync(architectProfile);
            _logger.LogInformation("Created architect profile");
        }
    }
}
