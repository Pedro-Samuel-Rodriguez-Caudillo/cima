using System;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace cima.Application.Data;

/// <summary>
/// Seeder de datos de prueba para desarrollo
/// Solo se ejecuta en entorno de desarrollo
/// </summary>
public class DevelopmentDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IRepository<FeaturedListing, Guid> _featuredListingRepository;
    private readonly IRepository<ContactRequest, Guid> _contactRequestRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ILogger<DevelopmentDataSeeder> _logger;

    public DevelopmentDataSeeder(
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository,
        IRepository<FeaturedListing, Guid> featuredListingRepository,
        IRepository<ContactRequest, Guid> contactRequestRepository,
        IGuidGenerator guidGenerator,
        ILogger<DevelopmentDataSeeder> logger)
    {
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
        _featuredListingRepository = featuredListingRepository;
        _contactRequestRepository = contactRequestRepository;
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

        _logger.LogInformation("Seeding development data...");

        // Crear arquitecto de prueba
        var architect = await SeedArchitect();

        // Crear propiedades de prueba
        var listings = await SeedListings(architect.Id);

        // Marcar algunas como destacadas
        await SeedFeaturedListings(listings);

        // Crear solicitudes de contacto de prueba
        await SeedContactRequests(listings);

        _logger.LogInformation("Development data seeded successfully");
    }

    private async Task<Architect> SeedArchitect()
    {
        var architect = new Architect
        {
            UserId = Guid.Empty, // Sin usuario asignado por ahora
            Bio = "Arquitecto especializado en diseño contemporáneo y sustentable. Más de 15 años de experiencia en proyectos residenciales de lujo.",
            PortfolioUrl = "https://ejemplo.com/portfolio"
        };

        await _architectRepository.InsertAsync(architect);
        return architect;
    }

    private async Task<Listing[]> SeedListings(Guid architectId)
    {
        var listings = new[]
        {
            // Casa de lujo en venta
            CreateListing(
                "Casa Moderna con Vista al Mar",
                "Espectacular residencia de lujo con vista panorámica al océano. Diseño arquitectónico contemporáneo con acabados de primera calidad. Amplios espacios, terrazas y alberca infinity.",
                "Playa del Carmen, Quintana Roo",
                12500000m,
                450m,
                4,
                5,
                ListingStatus.Published,
                PropertyCategory.Residential,
                PropertyType.House,
                TransactionType.Sale,
                architectId,
                -30
            ),

            // Departamento en renta
            CreateListing(
                "Departamento de Lujo en Polanco",
                "Exclusivo departamento amueblado en la mejor zona de Polanco. 2 recámaras con baño completo, sala-comedor, cocina integral, balcón con vista. Amenidades: gimnasio, alberca, seguridad 24/7.",
                "Polanco, Ciudad de México",
                45000m,
                120m,
                2,
                2,
                ListingStatus.Published,
                PropertyCategory.Residential,
                PropertyType.Apartment,
                TransactionType.Rent,
                architectId,
                -25
            ),

            // Oficina comercial
            CreateListing(
                "Oficina Corporativa en Santa Fe",
                "Moderna oficina corporativa en edificio AAA. Espacio diáfano con posibilidad de diseño personalizado. Piso de acceso controlado, estacionamiento, helipuerto.",
                "Santa Fe, Ciudad de México",
                8500000m,
                300m,
                0,
                4,
                ListingStatus.Published,
                PropertyCategory.Commercial,
                PropertyType.Office,
                TransactionType.Sale,
                architectId,
                -20
            ),

            // Villa de lujo
            CreateListing(
                "Villa de Lujo en Los Cabos",
                "Impresionante villa frente al mar con 6 suites, cine en casa, gimnasio, spa, cancha de tenis y alberca infinity. Diseño arquitectónico único con materiales de importación.",
                "Los Cabos, Baja California Sur",
                28000000m,
                800m,
                6,
                8,
                ListingStatus.Published,
                PropertyCategory.Residential,
                PropertyType.Villa,
                TransactionType.Sale,
                architectId,
                -15
            ),

            // Condominio
            CreateListing(
                "Condominio Familiar en Querétaro",
                "Hermoso condominio de 3 niveles en privada cerrada. 3 recámaras, jardín privado, 2 cajones de estacionamiento. Áreas comunes con alberca y área de juegos.",
                "Juriquilla, Querétaro",
                3800000m,
                180m,
                3,
                3,
                ListingStatus.Published,
                PropertyCategory.Residential,
                PropertyType.Condo,
                TransactionType.Sale,
                architectId,
                -10
            ),

            // Local comercial
            CreateListing(
                "Local Comercial en Zona Rosa",
                "Excelente local comercial en la mejor ubicación de Zona Rosa. Ideal para restaurante, boutique o showroom. Amplios ventanales, 2 niveles.",
                "Zona Rosa, Ciudad de México",
                65000m,
                150m,
                0,
                2,
                ListingStatus.Published,
                PropertyCategory.Commercial,
                PropertyType.RetailSpace,
                TransactionType.Rent,
                architectId,
                -8
            ),

            // Proyecto en portafolio (ya vendido)
            CreateListing(
                "Residencia Contemporánea Valle de Bravo",
                "PROYECTO COMPLETADO - Residencia de diseño contemporáneo con integración al paisaje. Premio de arquitectura sustentable 2023.",
                "Valle de Bravo, Estado de México",
                15000000m,
                500m,
                5,
                6,
                ListingStatus.Portfolio,
                PropertyCategory.Residential,
                PropertyType.House,
                TransactionType.Sale,
                architectId,
                -60
            ),

            // Propiedad archivada (vendida, no pública)
            CreateListing(
                "Casa en San Miguel de Allende",
                "VENDIDA - Hermosa casa colonial restaurada en el centro histórico.",
                "San Miguel de Allende, Guanajuato",
                9500000m,
                350m,
                4,
                4,
                ListingStatus.Archived,
                PropertyCategory.Residential,
                PropertyType.House,
                TransactionType.Sale,
                architectId,
                -90
            ),

            // Borrador (no publicado)
            CreateListing(
                "Proyecto en Desarrollo Coyoacán",
                "Proyecto en proceso de documentación...",
                "Coyoacán, Ciudad de México",
                7200000m,
                280m,
                3,
                3,
                ListingStatus.Draft,
                PropertyCategory.Residential,
                PropertyType.House,
                TransactionType.Sale,
                architectId,
                -2,
                addImage: false
            )
        };

        foreach (var listing in listings)
        {
            await _listingRepository.InsertAsync(listing);
        }

        return listings;
    }

    private Listing CreateListing(
        string title,
        string description,
        string location,
        decimal price,
        decimal area,
        int bedrooms,
        int bathrooms,
        ListingStatus status,
        PropertyCategory category,
        PropertyType type,
        TransactionType transactionType,
        Guid architectId,
        int daysAgo,
        bool addImage = true)
    {
        var listing = new Listing
        {
            Title = title,
            Description = description,
            Location = location,
            Price = price,
            Area = area,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            Status = status,
            Category = category,
            Type = type,
            TransactionType = transactionType,
            ArchitectId = architectId,
            CreatedAt = DateTime.UtcNow.AddDays(daysAgo)
        };

        if (addImage)
        {
            listing.Images = new[]
            {
                new ListingImage(
                    imageId: _guidGenerator.Create(),
                    url: "/images/getting-started/bg-01.png",
                    displayOrder: 1,
                    altText: $"Imagen de {title}",
                    fileSize: 500000,
                    contentType: "image/png"
                )
            };
        }

        return listing;
    }

    private async Task SeedFeaturedListings(Listing[] listings)
    {
        // Marcar las primeras 6 propiedades publicadas como destacadas
        var publishedListings = listings
            .Where(l => l.Status == ListingStatus.Published || l.Status == ListingStatus.Portfolio)
            .OrderByDescending(l => l.CreatedAt)
            .Take(6)
            .ToArray();

        for (int i = 0; i < publishedListings.Length; i++)
        {
            var featuredListing = new FeaturedListing(
                publishedListings[i].Id,
                displayOrder: i + 1,
                createdBy: null
            );

            await _featuredListingRepository.InsertAsync(featuredListing);
        }
    }

    private async Task SeedContactRequests(Listing[] listings)
    {
        var publishedListings = listings
            .Where(l => l.Status == ListingStatus.Published)
            .Take(3)
            .ToArray();

        if (!publishedListings.Any())
        {
            return;
        }

        var contactRequests = new[]
        {
            new ContactRequest
            {
                Name = "Juan Pérez García",
                Email = "juan.perez@ejemplo.com",
                Phone = "+52 55 1234 5678",
                Message = "Me interesa agendar una visita para conocer la propiedad. ¿Cuándo tendría disponibilidad?",
                ListingId = publishedListings[0].Id,
                ArchitectId = publishedListings[0].ArchitectId,
                Status = ContactRequestStatus.New,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new ContactRequest
            {
                Name = "María González López",
                Email = "maria.gonzalez@ejemplo.com",
                Phone = "+52 33 9876 5432",
                Message = "Quisiera más información sobre las amenidades del edificio y costos de mantenimiento.",
                ListingId = publishedListings.Length > 1 ? publishedListings[1].Id : publishedListings[0].Id,
                ArchitectId = publishedListings.Length > 1 ? publishedListings[1].ArchitectId : publishedListings[0].ArchitectId,
                Status = ContactRequestStatus.Replied,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                RepliedAt = DateTime.UtcNow.AddDays(-4),
                ReplyNotes = "Se envió información por correo electrónico"
            },
            new ContactRequest
            {
                Name = "Carlos Rodríguez Sánchez",
                Email = "carlos.rodriguez@ejemplo.com",
                Phone = "+52 81 5555 1234",
                Message = "Me gustaría hacer una oferta por la propiedad. ¿Podríamos agendar una videollamada?",
                ListingId = publishedListings.Length > 2 ? publishedListings[2].Id : publishedListings[0].Id,
                ArchitectId = publishedListings.Length > 2 ? publishedListings[2].ArchitectId : publishedListings[0].ArchitectId,
                Status = ContactRequestStatus.Closed,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                RepliedAt = DateTime.UtcNow.AddDays(-6),
                ReplyNotes = "Se agendó visita presencial. Cliente muy interesado."
            }
        };

        foreach (var request in contactRequests)
        {
            await _contactRequestRepository.InsertAsync(request);
        }
    }
}
