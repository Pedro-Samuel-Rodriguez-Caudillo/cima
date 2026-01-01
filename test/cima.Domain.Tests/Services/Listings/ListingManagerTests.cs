using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Xunit;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Services.Listings;
using cima.Domain.Entities.Listings;

namespace cima.Services.Listings;

/// <summary>
/// Tests para el dominio de Listings (Listing y ListingManager)
/// </summary>
public sealed class ListingManagerTests : cimaDomainTestBase<cimaDomainTestModule>
{
    private readonly IListingManager _listingManager;
    private readonly DateTime _testDateTime;

    public ListingManagerTests()
    {
        _listingManager = GetRequiredService<IListingManager>();
        _testDateTime = new DateTime(2023, 6, 20, 15, 0, 0, DateTimeKind.Utc);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_Should_Create_Valid_Listing()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        // Act
        var listing = await _listingManager.CreateAsync(
            title: "Nueva Propiedad",
            description: "Descripcion de lujo",
            location: "Guadalajara, Centro",
            price: 5000000m,
            landArea: 300m,
            constructionArea: 250m,
            bedrooms: 4,
            bathrooms: 3,
            categoryId: PropertyCatalogIds.Categories.Residential,
            typeId: PropertyCatalogIds.Types.House,
            transactionType: TransactionType.Sale,
            architectId: architectId,
            createdBy: createdBy
        );

        // Assert
        listing.ShouldNotBeNull();
        listing.Title.ShouldBe("Nueva Propiedad");
        listing.ArchitectId.ShouldBe(architectId);
        listing.CreatedBy.ShouldBe(createdBy);
        listing.Status.ShouldBe(ListingStatus.Draft);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Price_Invalid()
    {
        // Act & Assert
        await Should.ThrowAsync<BusinessException>(async () =>
            await _listingManager.CreateAsync(
                title: "Invalida",
                description: "...",
                location: "...",
                price: -100m, // Invalido
                landArea: 100m,
                constructionArea: 100m,
                bedrooms: 1,
                bathrooms: 1,
                categoryId: PropertyCatalogIds.Categories.Residential,
                typeId: PropertyCatalogIds.Types.House,
                transactionType: TransactionType.Sale,
                architectId: Guid.NewGuid(),
                createdBy: Guid.NewGuid()
            ));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_Should_Update_Fields()
    {
        // Arrange
        var listing = CreateTestListing();
        var modifiedBy = Guid.NewGuid();

        // Act
        await _listingManager.UpdateAsync(
            listing,
            title: "Titulo Actualizado",
            description: "Nueva Descripcion",
            location: "Nueva Ubicacion",
            price: 6000000m,
            landArea: 350m,
            constructionArea: 300m,
            bedrooms: 5,
            bathrooms: 4,
            categoryId: PropertyCatalogIds.Categories.Commercial,
            typeId: PropertyCatalogIds.Types.Office,
            transactionType: TransactionType.Rent,
            modifiedBy: modifiedBy
        );

        // Assert
        listing.Title.ShouldBe("Titulo Actualizado");
        listing.Price.ShouldBe(6000000m);
        listing.LastModifiedBy.ShouldBe(modifiedBy);
    }

    #endregion

    #region Workflow (Publish, Unpublish, Archive, etc.)

    [Fact]
    public async Task PublishAsync_Should_Change_Status_To_Published()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Draft);
        listing.AddImage(Guid.NewGuid(), "url", "thumb", "alt", 1024, "image/jpeg");
        var userId = Guid.NewGuid();

        // Act
        await _listingManager.PublishAsync(listing, userId);

        // Assert
        listing.Status.ShouldBe(ListingStatus.Published);
        listing.LastModifiedBy.ShouldBe(userId);
    }

    [Fact]
    public async Task PublishAsync_Should_Set_FirstPublishedAt_When_First_Time()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Draft);
        listing.AddImage(Guid.NewGuid(), "url", "thumb", "alt", 1024, "image/jpeg");

        // Act
        await _listingManager.PublishAsync(listing, Guid.NewGuid());

        // Assert
        listing.FirstPublishedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task PublishAsync_Should_Throw_When_No_Images()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Draft);

        // Act & Assert
        await Should.ThrowAsync<BusinessException>(async () =>
            await _listingManager.PublishAsync(listing, Guid.NewGuid()));
    }

    [Fact]
    public async Task UnpublishAsync_Should_Change_Status_To_Draft()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Published);

        // Act
        await _listingManager.UnpublishAsync(listing, Guid.NewGuid());

        // Assert
        listing.Status.ShouldBe(ListingStatus.Draft);
    }

    [Fact]
    public async Task ArchiveAsync_Should_Set_Archived_Status()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Published);

        // Act
        await _listingManager.ArchiveAsync(listing, Guid.NewGuid());

        // Assert
        listing.Status.ShouldBe(ListingStatus.Archived);
    }

    [Fact]
    public async Task MoveToPortfolioAsync_Should_Set_Portfolio_Status()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Published);

        // Act
        await _listingManager.MoveToPortfolioAsync(listing, Guid.NewGuid());

        // Assert
        listing.Status.ShouldBe(ListingStatus.Portfolio);
    }

    #endregion

    #region Helper Methods

    private static Architect CreateTestArchitect(Guid userId, bool isActive = true)
    {
        return new Architect
        {
            UserId = userId,
            IsActive = isActive,
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = DateTime.UtcNow
        };
    }

    private static Listing CreateTestListing(ListingStatus status = ListingStatus.Draft)
    {
        var listing = new Listing(
            Guid.NewGuid(),
            "Casa de prueba",
            "Descripcion de prueba",
            new Address("Ubicacion"),
            1000000m,
            200m,
            150m,
            3,
            2,
            PropertyCatalogIds.Categories.Residential,
            PropertyCatalogIds.Types.House,
            TransactionType.Sale,
            Guid.NewGuid(),
            Guid.NewGuid()
        );

        if (status != ListingStatus.Draft)
        {
            // Forzar estado para simplificar tests de workflow si es necesario, 
            // aunque idealmente usaríamos los métodos de negocio.
            if (status == ListingStatus.Published) 
            {
                listing.AddImage(Guid.NewGuid(), "url", "thumb", "alt", 1024, "image/jpeg");
                listing.Publish(Guid.NewGuid());
            }
            else if (status == ListingStatus.Archived)
            {
                listing.Archive(Guid.NewGuid());
            }
            else if (status == ListingStatus.Portfolio)
            {
                listing.MoveToPortfolio(Guid.NewGuid());
            }
        }

        return listing;
    }

    #endregion
}
