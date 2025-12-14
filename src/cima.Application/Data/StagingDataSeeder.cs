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
/// Seeder con datos mínimos para el entorno de Staging.
/// Crea los roles y usuarios base para que el equipo pueda usar el panel sin configuraciones manuales.
/// </summary>
public class StagingDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IdentityRoleManager _roleManager;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<StagingDataSeeder> _logger;

    private const string DefaultPassword = "1q2w3E*";

    public StagingDataSeeder(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager,
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        IRepository<Architect, Guid> architectRepository,
        IGuidGenerator guidGenerator,
        ILogger<StagingDataSeeder> logger)
    {
        _roleRepository = roleRepository;
        _roleManager = roleManager;
        _userRepository = userRepository;
        _userManager = userManager;
        _architectRepository = architectRepository;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        if (!environment.Equals("Staging", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("[STAGING SEEDER] Entorno {Environment} detectado. Seeder omitido.", environment);
            return;
        }

        _logger.LogInformation("========================================");
        _logger.LogInformation("[STAGING SEEDER] Inicializando datos predeterminados para Staging...");
        _logger.LogInformation("========================================");

        await EnsureRoleAsync("admin");
        await EnsureRoleAsync("architect");

        var adminUser = await EnsureUserAsync(
            email: "admin@cima.com",
            name: "Admin",
            surname: "CIMA",
            roleName: "admin");

        var architectUser = await EnsureUserAsync(
            email: "arq@cima.com",
            name: "Arquitecto",
            surname: "CIMA",
            roleName: "architect");

        await EnsureArchitectProfileAsync(adminUser);
        await EnsureArchitectProfileAsync(architectUser);

        _logger.LogInformation("========================================");
        _logger.LogInformation("[STAGING SEEDER] Usuarios/roles base confirmados correctamente.");
        _logger.LogInformation("========================================");
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        var normalizedName = roleName.ToUpperInvariant();
        var existingRole = await _roleRepository.FindByNormalizedNameAsync(normalizedName);
        if (existingRole != null)
        {
            _logger.LogInformation("[STAGING SEEDER] Rol '{Role}' ya existe.", roleName);
            return;
        }

        var role = new IdentityRole(_guidGenerator.Create(), roleName);

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            _logger.LogWarning("[STAGING SEEDER] No se pudo crear el rol '{Role}': {Errors}", roleName, string.Join(", ", result.Errors));
        }
        else
        {
            _logger.LogInformation("[STAGING SEEDER] Rol '{Role}' creado.", roleName);
        }
    }

    private async Task<IdentityUser> EnsureUserAsync(string email, string name, string surname, string roleName)
    {
        var normalizedEmail = email.ToUpperInvariant();
        var user = await _userRepository.FindByNormalizedEmailAsync(normalizedEmail);
        if (user == null)
        {
            user = new IdentityUser(_guidGenerator.Create(), email, email)
            {
                Name = name,
                Surname = surname,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, DefaultPassword);
            if (!createResult.Succeeded)
            {
                _logger.LogWarning("[STAGING SEEDER] Error creando usuario '{Email}': {Errors}", email, string.Join(", ", createResult.Errors));
            }
            else
            {
                _logger.LogInformation("[STAGING SEEDER] Usuario '{Email}' creado con la contraseña predeterminada.", email);
            }
        }
        else
        {
            _logger.LogInformation("[STAGING SEEDER] Usuario '{Email}' ya existe.", email);
        }

        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, roleName);
            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("[STAGING SEEDER] No se pudo asignar el rol '{Role}' al usuario '{Email}': {Errors}", roleName, email, string.Join(", ", roleResult.Errors));
            }
            else
            {
                _logger.LogInformation("[STAGING SEEDER] Rol '{Role}' asignado a '{Email}'.", roleName, email);
            }
        }

        return user;
    }

    private async Task EnsureArchitectProfileAsync(IdentityUser user)
    {
        var profile = await _architectRepository.FirstOrDefaultAsync(a => a.UserId == user.Id);
        if (profile != null)
        {
            return;
        }

        var architect = new Architect
        {
            UserId = user.Id,
            RegistrationDate = DateTime.UtcNow,
            TotalListingsPublished = 0,
            ActiveListings = 0,
            IsActive = true
        };

        await _architectRepository.InsertAsync(architect);
        _logger.LogInformation("[STAGING SEEDER] Perfil de arquitecto creado para '{Email}'.", user.Email);
    }
}
