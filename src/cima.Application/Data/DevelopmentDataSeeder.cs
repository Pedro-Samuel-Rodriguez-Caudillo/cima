using System;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace cima.Application.Data;

/// <summary>
/// Seeder de datos de prueba para desarrollo
/// Hybrid Approach: Usuarios fijos (admin + arquitecto) + Listings generados con Bogus
/// Solo se ejecuta en entorno de desarrollo
/// </summary>
public class DevelopmentDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IRepository<FeaturedListing, Guid> _featuredListingRepository;
    private readonly IRepository<ContactRequest, Guid> _contactRequestRepository;
    private readonly IIdentityUserRepository _userRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    public DevelopmentDataSeeder(
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository,
        IRepository<FeaturedListing, Guid> featuredListingRepository,
        IRepository<ContactRequest, Guid> contactRequestRepository,
        IIdentityUserRepository userRepository,
        IdentityUserManager userManager,
        IGuidGenerator guidGenerator,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
        _featuredListingRepository = featuredListingRepository;
        _contactRequestRepository = contactRequestRepository;
        _userRepository = userRepository;
        _userManager = userManager;
        _guidGenerator = guidGenerator;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // Solo ejecutar en desarrollo
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (environment != "Development")
        {
            _logger.LogInformation("Skipping development data seeding (not in Development environment)");
            return;
        }

        // Verificar si ya hay datos
        if (await _listingRepository.AnyAsync())
        {
            _logger.LogInformation("Development data already seeded");
            return;
        }

        _logger.LogInformation("Seeding development data with Hybrid approach...");

        // Crear usuarios y arquitectos fijos
        var (adminUser, architectUser, architect) = await SeedUsersAndArchitect();

        // Crear propiedades con Bogus
        var listings = await SeedListingsWithBogus(architect.Id);

        // Marcar algunas como destacadas
        await SeedFeaturedListings(listings);

        _logger.LogInformation("Development data seeded successfully");
        _logger.LogInformation($"Created: 1 admin, 1 architect, {listings.Length} listings");
    }

    private async Task<(IdentityUser admin, IdentityUser architect, Architect architectProfile)> SeedUsersAndArchitect()
    {
        // Admin user (credenciales fijas para login)
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

        // Architect user (credenciales fijas para login)
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
            _logger.LogInformation("Created architect profile for user");
        }

        return (admin, architectUser, architectProfile);
    }

    private async Task<Listing[]> SeedListingsWithBogus(Guid architectId)
    {
        Randomizer.Seed = new Random(12345); // Seed fijo para datos consistentes

        var faker = new Faker("es_MX");
        
        // Listas de valores realistas
        var propertyTitles = new[]
        {
            "Casa Moderna", "Departamento de Lujo", "Villa Campestre", "Condominio Familiar",
            "Residencia Contemporánea", "Penthouse Exclusivo", "Casa Minimalista", "Loft Industrial"
        };
        
        var zones = new[]
        {
            "Polanco, Ciudad de México", "Santa Fe, Ciudad de México", "Condesa, Ciudad de México",
            "Playa del Carmen, Quintana Roo", "Guadalajara, Jalisco", "Monterrey, Nuevo León",
            "Querétaro, Querétaro", "Valle de Bravo, Estado de México", "Los Cabos, Baja California Sur"
        };

        var listings = Enumerable.Range(1, 12).Select(i =>
        {
            var status = i <= 8 ? ListingStatus.Published : 
                        i == 9 ? ListingStatus.Portfolio :
                        i == 10 ? ListingStatus.Archived : 
                        ListingStatus.Draft;

            var type = faker.PickRandom<PropertyType>();
            var transactionType = faker.PickRandom<TransactionType>();
            
            var landArea = faker.Random.Decimal(100, 600);
            var constructionArea = faker.Random.Decimal(landArea * 0.4m, landArea * 0.9m);
            
            var listing = new Listing
            {
                Title = $"{faker.PickRandom(propertyTitles)} {faker.PickRandom(zones).Split(',')[1].Trim()}",
                Description = GenerateRealisticDescription(type, faker),
                Location = faker.PickRandom(zones),
                Price = GenerateRealisticPrice(type, transactionType, faker),
                LandArea = Math.Round(landArea, 2),
                ConstructionArea = Math.Round(constructionArea, 2),
                Bedrooms = type == PropertyType.Office ? 0 : faker.Random.Int(1, 5),
                Bathrooms = type == PropertyType.Office ? 2 : faker.Random.Int(1, 4),
                Status = status,
                Category = DetermineCategory(type),
                Type = type,
                TransactionType = transactionType,
                ArchitectId = architectId,
                CreatedAt = DateTime.UtcNow.AddDays(-faker.Random.Int(2, 120))
            };

            // Agregar imagen genérica de ABP solo a publicadas/portfolio
            if (status == ListingStatus.Published || status == ListingStatus.Portfolio)
            {
                var imageId = _guidGenerator.Create();
                listing.Images = new[]
                {
                    new ListingImage(
                        imageId: imageId,
                        url: "/images/getting-started/bg-01.png",
                        altText: $"Imagen de {listing.Title}",
                        fileSize: 500000,
                        contentType: "image/png",
                        previousImageId: null,
                        nextImageId: null
                    )
                };
            }

            return listing;
        }).ToArray();

        foreach (var listing in listings)
        {
            await _listingRepository.InsertAsync(listing);
        }

        return listings;
    }

    private string GenerateRealisticDescription(PropertyType type, Faker faker)
    {
        var templates = type switch
        {
            PropertyType.House => new[]
            {
                "Hermosa casa de diseño contemporáneo con amplios espacios y acabados de lujo.",
                "Residencia familiar con jardín privado, perfecta para familias que buscan tranquilidad.",
                "Casa moderna con diseño arquitectónico único y vistas espectaculares."
            },
            PropertyType.Apartment => new[]
            {
                "Departamento de lujo con acabados premium en la mejor ubicación de la zona.",
                "Moderno apartamento con balcón, amenidades completas y seguridad 24/7.",
                "Exclusivo departamento amueblado listo para habitarse."
            },
            PropertyType.Office => new[]
            {
                "Oficina corporativa en edificio AAA con estacionamiento y helipuerto.",
                "Espacio comercial ideal para empresas en crecimiento.",
                "Local de oficinas con diseño moderno y excelente ubicación."
            },
            _ => new[]
            {
                "Propiedad exclusiva con características únicas.",
                "Excelente inversión en zona de alto plusvalía."
            }
        };

        return faker.PickRandom(templates);
    }

    private decimal GenerateRealisticPrice(PropertyType type, TransactionType transactionType, Faker faker)
    {
        if (transactionType == TransactionType.Rent)
        {
            return type switch
            {
                PropertyType.House => faker.Random.Decimal(15000, 80000),
                PropertyType.Apartment => faker.Random.Decimal(8000, 45000),
                PropertyType.Office => faker.Random.Decimal(20000, 100000),
                _ => faker.Random.Decimal(10000, 50000)
            };
        }
        else // Sale
        {
            return type switch
            {
                PropertyType.House => faker.Random.Decimal(2000000, 15000000),
                PropertyType.Apartment => faker.Random.Decimal(1500000, 8000000),
                PropertyType.Villa => faker.Random.Decimal(10000000, 30000000),
                PropertyType.Office => faker.Random.Decimal(3000000, 20000000),
                _ => faker.Random.Decimal(1000000, 10000000)
            };
        }
    }

    private PropertyCategory DetermineCategory(PropertyType type)
    {
        return type switch
        {
            PropertyType.House or PropertyType.Apartment or PropertyType.Condo or 
            PropertyType.Townhouse or PropertyType.Villa => PropertyCategory.Residential,
            PropertyType.Office or PropertyType.Warehouse or PropertyType.RetailSpace or 
            PropertyType.Restaurant or PropertyType.Hotel => PropertyCategory.Commercial,
            PropertyType.MixedUseBuilding or PropertyType.LiveWorkSpace => PropertyCategory.Mixed,
            _ => PropertyCategory.Residential
        };
    }

    private async Task SeedFeaturedListings(Listing[] listings)
    {
        var publishedListings = listings
            .Where(l => l.Status == ListingStatus.Published || l.Status == ListingStatus.Portfolio)
            .OrderByDescending(l => l.CreatedAt)
            .Take(6)
            .ToArray();

        foreach (var listing in publishedListings)
        {
            var featuredListing = new FeaturedListing(
                listing.Id,
                createdBy: null
            );

            await _featuredListingRepository.InsertAsync(featuredListing);
        }

        _logger.LogInformation($"Featured {publishedListings.Length} listings");
    }
}
