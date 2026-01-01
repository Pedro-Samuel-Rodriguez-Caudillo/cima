using System;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Entities.Listings;
using cima.Domain.Entities.Portfolio;
using cima.Domain.Shared;
using cima.Listings;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;

namespace cima.Portfolio;

public class PortfolioAppServiceTests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly IPortfolioAppService _portfolioAppService;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IGuidGenerator _guidGenerator;

    public PortfolioAppServiceTests()
    {
        _portfolioAppService = GetRequiredService<IPortfolioAppService>();
        _listingRepository = GetRequiredService<IRepository<Listing, Guid>>();
        _architectRepository = GetRequiredService<IRepository<Architect, Guid>>();
        _guidGenerator = GetRequiredService<IGuidGenerator>();
    }

    [Fact]
    public async Task Should_Create_Portfolio_From_Listing()
    {
        // Arrange
        var architectId = await CreateTestArchitectAsync();
        var listing = await CreateTestListingAsync(architectId);

        // Act
        var portfolioDto = await _portfolioAppService.CreateFromListingAsync(listing.Id);

        // Assert
        portfolioDto.ShouldNotBeNull();
        portfolioDto.Title.ShouldBe(listing.Title);
        portfolioDto.Description.ShouldBe(listing.Description);
        
        // Verificar mapeo de categorias
        portfolioDto.CategoryId.ShouldBe(listing.CategoryId);

        // Verificar imagenes
        portfolioDto.Gallery.Count.ShouldBe(listing.Images.Count);
        portfolioDto.Gallery.First().Url.ShouldBe(listing.Images.First().Url);
        portfolioDto.CoverImage.ShouldBe(listing.Images.First().Url); // Primer imagen debe ser cover por defecto

        // Verificar cambio de estado en Listing original
        var updatedListing = await _listingRepository.GetAsync(listing.Id);
        updatedListing.Status.ShouldBe(ListingStatus.Portfolio);
    }

    private async Task<Guid> CreateTestArchitectAsync()
    {
        var architectId = _guidGenerator.Create();
        // Necesitamos un UserId valido, asumimos uno ficticio o el actual si el framework lo provee mockeado
        // En TestBase suele haber un usuario default.
        var userId = Guid.NewGuid(); 

        var architect = new Architect(architectId, userId);
        
        await _architectRepository.InsertAsync(architect);
        return architectId;
    }

    private async Task<Listing> CreateTestListingAsync(Guid architectId)
    {
        var listingId = _guidGenerator.Create();
        var listing = new Listing(
            listingId,
            "Casa de Prueba para Portafolio",
            "Descripción detallada para la prueba de migración",
            new Address("{ \"Address\": \"Calle Falsa 123\" }"),
            5000000,
            200,
            180,
            3,
            2,
            PropertyCatalogIds.Categories.Residential,
            PropertyCatalogIds.Types.House,
            TransactionType.Sale,
            architectId,
            Guid.NewGuid()
        );

        // Agregar imagenes
        listing.AddImage(
            _guidGenerator.Create(),
            "http://test.com/image1.jpg",
            "http://test.com/thumb1.jpg",
            "Fachada",
            1024,
            "image/jpeg"
        );
        
        listing.AddImage(
            _guidGenerator.Create(),
            "http://test.com/image2.jpg",
            "http://test.com/thumb2.jpg",
            "Interiores",
            2048,
            "image/jpeg"
        );

        // Publicar para que sea un caso real (aunque puede ser draft)
        listing.Publish(Guid.NewGuid());

        await _listingRepository.InsertAsync(listing);
        return listing;
    }
}
