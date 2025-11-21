using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Guids;

namespace cima.Data;

public class ListingsDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IGuidGenerator _guidGenerator;

    public ListingsDataSeeder(
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IGuidGenerator guidGenerator)
    {
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
        _userRepository = userRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _listingRepository.GetCountAsync() > 0)
        {
            return;
        }

        var adminUser = await _userRepository.FirstOrDefaultAsync(u => u.UserName == "admin");
        if (adminUser == null)
        {
            return;
        }

        var architect = await _architectRepository.FirstOrDefaultAsync(a => a.UserId == adminUser.Id);
        if (architect == null)
        {
            architect = new Architect
            {
                UserId = adminUser.Id,
                Bio = "Arquitecto especializado en diseño minimalista y arquitectura contemporánea. Más de 15 años de experiencia en proyectos residenciales de alta gama.",
                PortfolioUrl = "https://example.com/portfolio"
            };
            await _architectRepository.InsertAsync(architect);
        }

        var listing1 = new Listing
        {
            ArchitectId = architect.Id,
            Title = "Casa Minimalista en Polanco",
            Description = "Exquisita residencia de diseño contemporáneo ubicada en el corazón de Polanco. Espacios abiertos, líneas limpias y acabados de lujo definen esta propiedad única.\n\nCaracterísticas destacadas:\n- Doble altura en sala principal\n- Cocina italiana totalmente equipada\n- Terraza con vista panorámica\n- Sistema de domótica integrado",
            Location = "Polanco, Ciudad de México",
            Price = 12500000,
            Area = 350,
            Bedrooms = 4,
            Bathrooms = 3,
            Status = ListingStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            CreatedBy = adminUser.Id,
            Images = new List<ListingImage>
            {
                new ListingImage
                {
                    ImageId = _guidGenerator.Create(),
                    Url = "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9",
                    AltText = "Fachada principal",
                    DisplayOrder = 1,
                    ContentType = "image/jpeg",
                    FileSize = 2048000
                }
            }
        };

        var listing2 = new Listing
        {
            ArchitectId = architect.Id,
            Title = "Penthouse Contemporáneo Santa Fe",
            Description = "Impresionante penthouse de lujo con diseño vanguardista. Vistas de 360 grados, acabados premium y tecnología de punta.",
            Location = "Santa Fe, Ciudad de México",
            Price = 18900000,
            Area = 420,
            Bedrooms = 5,
            Bathrooms = 4,
            Status = ListingStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-25),
            CreatedBy = adminUser.Id,
            Images = new List<ListingImage>
            {
                new ListingImage
                {
                    ImageId = _guidGenerator.Create(),
                    Url = "https://images.unsplash.com/photo-1600607687939-ce8a6c25118c",
                    AltText = "Vista exterior",
                    DisplayOrder = 1,
                    ContentType = "image/jpeg",
                    FileSize = 2148000
                }
            }
        };

        var listing3 = new Listing
        {
            ArchitectId = architect.Id,
            Title = "Loft Industrial Roma Norte",
            Description = "Auténtico loft de estilo industrial con toques modernos. Techos altos, espacios diáfanos y diseño único.",
            Location = "Roma Norte, Ciudad de México",
            Price = 6500000,
            Area = 180,
            Bedrooms = 2,
            Bathrooms = 2,
            Status = ListingStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-20),
            CreatedBy = adminUser.Id,
            Images = new List<ListingImage>
            {
                new ListingImage
                {
                    ImageId = _guidGenerator.Create(),
                    Url = "https://images.unsplash.com/photo-1600585154340-be6161a56a0c",
                    AltText = "Sala principal",
                    DisplayOrder = 1,
                    ContentType = "image/jpeg",
                    FileSize = 1948000
                }
            }
        };

        var listing4 = new Listing
        {
            ArchitectId = architect.Id,
            Title = "Villa Moderna Coyoacán",
            Description = "Hermosa villa de diseño moderno en zona histórica. Combina tradición con vanguardia arquitectónica.",
            Location = "Coyoacán, Ciudad de México",
            Price = 9800000,
            Area = 280,
            Bedrooms = 3,
            Bathrooms = 3,
            Status = ListingStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-15),
            CreatedBy = adminUser.Id,
            Images = new List<ListingImage>
            {
                new ListingImage
                {
                    ImageId = _guidGenerator.Create(),
                    Url = "https://images.unsplash.com/photo-1600607687644-aac4c8e5e1a5",
                    AltText = "Jardín interior",
                    DisplayOrder = 1,
                    ContentType = "image/jpeg",
                    FileSize = 2248000
                }
            }
        };

        var listing5 = new Listing
        {
            ArchitectId = architect.Id,
            Title = "Departamento Luxury Reforma",
            Description = "Exclusivo departamento en Paseo de la Reforma. Ubicación privilegiada y diseño excepcional.",
            Location = "Reforma, Ciudad de México",
            Price = 15200000,
            Area = 320,
            Bedrooms = 4,
            Bathrooms = 3,
            Status = ListingStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            CreatedBy = adminUser.Id,
            Images = new List<ListingImage>
            {
                new ListingImage
                {
                    ImageId = _guidGenerator.Create(),
                    Url = "https://images.unsplash.com/photo-1600607687920-4e2a09cf159d",
                    AltText = "Vista nocturna",
                    DisplayOrder = 1,
                    ContentType = "image/jpeg",
                    FileSize = 2348000
                }
            }
        };

        var listing6 = new Listing
        {
            ArchitectId = architect.Id,
            Title = "Casa Ecológica Valle de Bravo",
            Description = "Residencia sustentable con certificación LEED. Diseño bioclimático y materiales ecológicos.",
            Location = "Valle de Bravo, Estado de México",
            Price = 11500000,
            Area = 400,
            Bedrooms = 4,
            Bathrooms = 4,
            Status = ListingStatus.Published,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            CreatedBy = adminUser.Id,
            Images = new List<ListingImage>
            {
                new ListingImage
                {
                    ImageId = _guidGenerator.Create(),
                    Url = "https://images.unsplash.com/photo-1600566753190-17f0baa2a6c3",
                    AltText = "Integración con naturaleza",
                    DisplayOrder = 1,
                    ContentType = "image/jpeg",
                    FileSize = 2448000
                }
            }
        };

        await _listingRepository.InsertManyAsync(new[] { listing1, listing2, listing3, listing4, listing5, listing6 });
    }
}
