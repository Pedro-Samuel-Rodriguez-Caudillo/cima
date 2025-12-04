using System;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Services.Listings;
using cima.Domain.Shared;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;
using Xunit;

namespace cima.Services.Listings;

/// <summary>
/// Tests unitarios para ListingManager (Domain Service).
/// </summary>
public sealed class ListingManagerTests : cimaDomainTestBase<cimaDomainTestModule>
{
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly ListingManager _listingManager;
    private readonly DateTime _testDateTime = new DateTime(2024, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    public ListingManagerTests()
    {
        _architectRepository = Substitute.For<IRepository<Architect, Guid>>();
        _guidGenerator = Substitute.For<IGuidGenerator>();
        _clock = Substitute.For<IClock>();
        
        _guidGenerator.Create().Returns(Guid.NewGuid());
        _clock.Now.Returns(_testDateTime);
        
        _listingManager = new ListingManager(_architectRepository, _guidGenerator, _clock);
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_Should_Create_Listing_With_Valid_Data()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(architectId, isActive: true);
        _architectRepository.GetAsync(architectId).Returns(architect);

        // Act
        var listing = await _listingManager.CreateAsync(
            title: "Casa de Prueba",
            description: "Una hermosa casa en zona residencial",
            location: "Guadalajara, Jalisco",
            price: 2500000m,
            landArea: 200m,
            constructionArea: 150m,
            bedrooms: 3,
            bathrooms: 2,
            category: PropertyCategory.Residential,
            type: PropertyType.House,
            transactionType: TransactionType.Sale,
            architectId: architectId);

        // Assert
        listing.ShouldNotBeNull();
        listing.Title.ShouldBe("Casa de Prueba");
        listing.Description.ShouldBe("Una hermosa casa en zona residencial");
        listing.Location.ShouldBe("Guadalajara, Jalisco");
        listing.Price.ShouldBe(2500000m);
        listing.Status.ShouldBe(ListingStatus.Draft);
        listing.ArchitectId.ShouldBe(architectId);
        listing.FirstPublishedAt.ShouldBeNull(); // Nunca publicado
    }

    [Fact]
    public async Task CreateAsync_Should_Set_FirstPublishedAt_To_Null()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(architectId, isActive: true);
        _architectRepository.GetAsync(architectId).Returns(architect);

        // Act
        var listing = await _listingManager.CreateAsync(
            title: "Nueva Casa",
            description: "Descripcion de prueba",
            location: "Ciudad",
            price: 1000000m,
            landArea: 100m,
            constructionArea: 80m,
            bedrooms: 2,
            bathrooms: 1,
            category: PropertyCategory.Residential,
            type: PropertyType.House,
            transactionType: TransactionType.Sale,
            architectId: architectId);

        // Assert
        listing.FirstPublishedAt.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAsync_Should_Trim_Title_And_Description()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(architectId, isActive: true);
        _architectRepository.GetAsync(architectId).Returns(architect);

        // Act
        var listing = await _listingManager.CreateAsync(
            title: "  Casa con espacios  ",
            description: "  Descripcion con espacios  ",
            location: "  Ubicacion  ",
            price: 1000000m,
            landArea: 100m,
            constructionArea: 80m,
            bedrooms: 2,
            bathrooms: 1,
            category: PropertyCategory.Residential,
            type: PropertyType.House,
            transactionType: TransactionType.Sale,
            architectId: architectId);

        // Assert
        listing.Title.ShouldBe("Casa con espacios");
        listing.Description.ShouldBe("Descripcion con espacios");
        listing.Location.ShouldBe("Ubicacion");
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Architect_Is_Inactive()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(architectId, isActive: false);
        _architectRepository.GetAsync(architectId).Returns(architect);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
            await _listingManager.CreateAsync(
                title: "Casa de Prueba",
                description: "Descripcion",
                location: "Ubicacion",
                price: 1000000m,
                landArea: 100m,
                constructionArea: 80m,
                bedrooms: 2,
                bathrooms: 1,
                category: PropertyCategory.Residential,
                type: PropertyType.House,
                transactionType: TransactionType.Sale,
                architectId: architectId));

        exception.Code.ShouldBe(cimaDomainErrorCodes.ArchitectInactive);
    }

    [Theory]
    [InlineData("", "Descripcion valida")]
    [InlineData("   ", "Descripcion valida")]
    public async Task CreateAsync_Should_Throw_When_Title_Is_Empty(string title, string description)
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(architectId, isActive: true);
        _architectRepository.GetAsync(architectId).Returns(architect);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
            await _listingManager.CreateAsync(
                title: title,
                description: description,
                location: "Ubicacion",
                price: 1000000m,
                landArea: 100m,
                constructionArea: 80m,
                bedrooms: 2,
                bathrooms: 1,
                category: PropertyCategory.Residential,
                type: PropertyType.House,
                transactionType: TransactionType.Sale,
                architectId: architectId));

        exception.Code.ShouldBe(cimaDomainErrorCodes.ListingTitleRequired);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_Price_Is_Zero_Or_Negative()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(architectId, isActive: true);
        _architectRepository.GetAsync(architectId).Returns(architect);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
            await _listingManager.CreateAsync(
                title: "Casa",
                description: "Descripcion",
                location: "Ubicacion",
                price: 0m,
                landArea: 100m,
                constructionArea: 80m,
                bedrooms: 2,
                bathrooms: 1,
                category: PropertyCategory.Residential,
                type: PropertyType.House,
                transactionType: TransactionType.Sale,
                architectId: architectId));

        exception.Code.ShouldBe(cimaDomainErrorCodes.ListingInvalidPrice);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_When_ConstructionArea_Exceeds_LandArea()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var architect = CreateTestArchitect(architectId, isActive: true);
        _architectRepository.GetAsync(architectId).Returns(architect);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
            await _listingManager.CreateAsync(
                title: "Casa",
                description: "Descripcion",
                location: "Ubicacion",
                price: 1000000m,
                landArea: 100m,
                constructionArea: 150m, // Mayor que landArea
                bedrooms: 2,
                bathrooms: 1,
                category: PropertyCategory.Residential,
                type: PropertyType.House,
                transactionType: TransactionType.Sale,
                architectId: architectId));

        exception.Code.ShouldBe(cimaDomainErrorCodes.ConstructionAreaExceedsLandArea);
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public async Task PublishAsync_Should_Change_Status_From_Draft_To_Published()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Draft);

        // Act
        await _listingManager.PublishAsync(listing);

        // Assert
        listing.Status.ShouldBe(ListingStatus.Published);
        listing.LastModifiedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task PublishAsync_Should_Set_FirstPublishedAt_On_First_Publication()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Draft);
        listing.FirstPublishedAt = null; // Nunca publicado

        // Act
        await _listingManager.PublishAsync(listing);

        // Assert
        listing.FirstPublishedAt.ShouldBe(_testDateTime);
    }

    [Fact]
    public async Task PublishAsync_Should_Not_Change_FirstPublishedAt_On_Republication()
    {
        // Arrange
        var originalPublishDate = new DateTime(2023, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var listing = CreateTestListing(ListingStatus.Draft);
        listing.FirstPublishedAt = originalPublishDate; // Ya fue publicado antes

        // Act
        await _listingManager.PublishAsync(listing);

        // Assert
        listing.FirstPublishedAt.ShouldBe(originalPublishDate); // No debe cambiar
    }

    [Fact]
    public async Task PublishAsync_Should_Throw_When_Already_Published()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Published);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
            await _listingManager.PublishAsync(listing));

        exception.Code.ShouldBe(cimaDomainErrorCodes.InvalidStatusTransition);
    }

    [Fact]
    public async Task UnpublishAsync_Should_Change_Status_From_Published_To_Draft()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Published);

        // Act
        await _listingManager.UnpublishAsync(listing);

        // Assert
        listing.Status.ShouldBe(ListingStatus.Draft);
    }

    [Fact]
    public async Task UnpublishAsync_Should_Preserve_FirstPublishedAt()
    {
        // Arrange
        var originalPublishDate = new DateTime(2023, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var listing = CreateTestListing(ListingStatus.Published);
        listing.FirstPublishedAt = originalPublishDate;

        // Act
        await _listingManager.UnpublishAsync(listing);

        // Assert
        listing.FirstPublishedAt.ShouldBe(originalPublishDate); // No debe cambiar
    }

    [Fact]
    public async Task ArchiveAsync_Should_Change_Status_From_Published_To_Archived()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Published);

        // Act
        await _listingManager.ArchiveAsync(listing);

        // Assert
        listing.Status.ShouldBe(ListingStatus.Archived);
    }

    [Fact]
    public async Task ArchiveAsync_Should_Change_Status_From_Portfolio_To_Archived()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Portfolio);

        // Act
        await _listingManager.ArchiveAsync(listing);

        // Assert
        listing.Status.ShouldBe(ListingStatus.Archived);
    }

    [Fact]
    public async Task UnarchiveAsync_Should_Change_Status_From_Archived_To_Published()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Archived);

        // Act
        await _listingManager.UnarchiveAsync(listing);

        // Assert
        listing.Status.ShouldBe(ListingStatus.Published);
    }

    [Fact]
    public async Task UnarchiveAsync_Should_Throw_When_Not_Archived()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Published);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
            await _listingManager.UnarchiveAsync(listing));

        exception.Code.ShouldBe(cimaDomainErrorCodes.ListingNotArchived);
    }

    [Fact]
    public async Task MoveToPortfolioAsync_Should_Change_Status_From_Published_To_Portfolio()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Published);

        // Act
        await _listingManager.MoveToPortfolioAsync(listing);

        // Assert
        listing.Status.ShouldBe(ListingStatus.Portfolio);
    }

    [Fact]
    public async Task MoveToPortfolioAsync_Should_Throw_When_Draft()
    {
        // Arrange
        var listing = CreateTestListing(ListingStatus.Draft);

        // Act & Assert
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
            await _listingManager.MoveToPortfolioAsync(listing));

        exception.Code.ShouldBe(cimaDomainErrorCodes.InvalidStatusTransition);
    }

    #endregion

    #region CanChangeStatus Tests

    [Theory]
    [InlineData(ListingStatus.Draft, ListingStatus.Published, true)]
    [InlineData(ListingStatus.Published, ListingStatus.Draft, true)]
    [InlineData(ListingStatus.Published, ListingStatus.Archived, true)]
    [InlineData(ListingStatus.Published, ListingStatus.Portfolio, true)]
    [InlineData(ListingStatus.Archived, ListingStatus.Published, true)]
    [InlineData(ListingStatus.Portfolio, ListingStatus.Archived, true)]
    [InlineData(ListingStatus.Portfolio, ListingStatus.Published, true)]
    [InlineData(ListingStatus.Draft, ListingStatus.Archived, false)]
    [InlineData(ListingStatus.Draft, ListingStatus.Portfolio, false)]
    [InlineData(ListingStatus.Archived, ListingStatus.Draft, false)]
    [InlineData(ListingStatus.Archived, ListingStatus.Portfolio, false)]
    public void CanChangeStatus_Should_Return_Correct_Result(
        ListingStatus currentStatus,
        ListingStatus newStatus,
        bool expectedResult)
    {
        // Act
        var result = _listingManager.CanChangeStatus(currentStatus, newStatus);

        // Assert
        result.ShouldBe(expectedResult);
    }

    #endregion

    #region ValidateListingData Tests

    [Fact]
    public void ValidateListingData_Should_Not_Throw_With_Valid_Data()
    {
        // Act & Assert (no exception means success)
        Should.NotThrow(() => _listingManager.ValidateListingData(
            title: "Titulo valido",
            description: "Descripcion valida",
            price: 1000000m,
            landArea: 200m,
            constructionArea: 150m));
    }

    [Fact]
    public void ValidateListingData_Should_Throw_When_Title_Too_Long()
    {
        // Arrange
        var longTitle = new string('A', 201);

        // Act & Assert
        var exception = Should.Throw<BusinessException>(() =>
            _listingManager.ValidateListingData(
                title: longTitle,
                description: "Descripcion",
                price: 1000000m,
                landArea: 200m,
                constructionArea: 150m));

        exception.Code.ShouldBe(cimaDomainErrorCodes.ListingTitleTooLong);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void ValidateListingData_Should_Throw_When_LandArea_Invalid(decimal landArea)
    {
        // Act & Assert
        var exception = Should.Throw<BusinessException>(() =>
            _listingManager.ValidateListingData(
                title: "Titulo",
                description: "Descripcion",
                price: 1000000m,
                landArea: landArea,
                constructionArea: 50m));

        exception.Code.ShouldBe(cimaDomainErrorCodes.ListingInvalidLandArea);
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
        return new Listing
        {
            Title = "Casa de prueba",
            Description = "Descripcion de prueba",
            Location = "Ubicacion",
            Price = 1000000m,
            LandArea = 200m,
            ConstructionArea = 150m,
            Bedrooms = 3,
            Bathrooms = 2,
            Category = PropertyCategory.Residential,
            Type = PropertyType.House,
            TransactionType = TransactionType.Sale,
            ArchitectId = Guid.NewGuid(),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            FirstPublishedAt = null
        };
    }

    #endregion
}
