using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using cima.Listings;
using cima.Listings.Inputs;
using cima.Listings.Outputs;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Entities.Listings;

namespace cima.ApplicationServices;

/// <summary>
/// Tests de integración para ListingAppService
/// </summary>
public sealed class ListingAppServiceTests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly IListingAppService _listingAppService;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly ICurrentUser _currentUser;

    public ListingAppServiceTests()
    {
        _listingAppService = GetRequiredService<IListingAppService>();
        _listingRepository = GetRequiredService<IRepository<Listing, Guid>>();
        _architectRepository = GetRequiredService<IRepository<Architect, Guid>>();
        _currentUser = GetRequiredService<ICurrentUser>();
    }

    #region CRUD Tests

    [Fact]
    public async Task Should_Get_List_Of_Listings()
    {
        // Arrange
        await CreateTestListingAsync();

        // Act
        var result = await _listingAppService.GetListAsync(
            new GetListingsInput { MaxResultCount = 10 }
        );

        // Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_Create_A_New_Listing()
    {
        // Arrange
        var architect = await CreateTestArchitectAsync();
        var input = new CreateListingDto
        {
            Title = "Nueva Propiedad Test",
            Description = "Descripción detallada de la propiedad con más de 20 caracteres para validación",
            Price = 1200000m,
            LandArea = 150m,
            ConstructionArea = 100m,
            Bedrooms = 3,
            Bathrooms = 2,
            Category = PropertyCategory.Residential,
            Type = PropertyType.House,
            TransactionType = TransactionType.Sale,
            Address = new AddressDto { Value = "Test Location Guadalajara" },
            ArchitectId = architect.Id
        };

        // Act
        var result = await _listingAppService.CreateAsync(input);

        // Assert
        result.Id.ShouldNotBe(Guid.Empty);
        result.Title.ShouldBe(input.Title);
        result.Status.ShouldBe(ListingStatus.Draft);
    }

    [Fact]
    public async Task Should_Update_An_Existing_Listing()
    {
        // Arrange
        var listing = await CreateTestListingAsync();
        var updatedTitle = "Título Actualizado";
        var input = new UpdateListingDto
        {
            Id = listing.Id,
            Title = updatedTitle,
            Description = listing.Description,
            Price = listing.Price + 100000m,
            LandArea = listing.LandArea,
            ConstructionArea = listing.ConstructionArea,
            Bedrooms = listing.Bedrooms,
            Bathrooms = listing.Bathrooms,
            Category = listing.Category,
            Type = listing.Type,
            TransactionType = listing.TransactionType,
            Address = new AddressDto { Value = "Nueva Ubicación" }
        };

        // Act
        var result = await _listingAppService.UpdateAsync(listing.Id, input);

        // Assert
        result.Title.ShouldBe(updatedTitle);
        
        var dbListing = await _listingRepository.GetAsync(listing.Id);
        dbListing.Title.ShouldBe(updatedTitle);
    }

    [Fact]
    public async Task Should_Delete_A_Listing()
    {
        // Arrange
        var listing = await CreateTestListingAsync();
        var listingId = listing.Id;

        // Act
        await _listingAppService.DeleteAsync(listingId);

        // Assert
        var dbListing = await _listingRepository.FindAsync(listingId);
        dbListing.ShouldBeNull();
    }

    #endregion

    #region Status Change Tests

    [Fact]
    public async Task PublishAsync_Should_Change_Status_To_Published()
    {
        // Arrange
        var listing = await CreateTestListingAsync(status: ListingStatus.Draft);
        // Necesitamos al menos una imagen para publicar
        await WithUnitOfWorkAsync(async () => {
            var l = await _listingRepository.GetAsync(listing.Id);
            l.AddImage(Guid.NewGuid(), "url", "thumb", "alt", 1024, "image/jpeg");
            await _listingRepository.UpdateAsync(l);
        });

        // Act
        var result = await _listingAppService.PublishAsync(new PublishListingDto { ListingId = listing.Id });

        // Assert
        result.Status.ShouldBe(ListingStatus.Published);
    }

    [Fact]
    public async Task ArchiveAsync_Should_Change_Status_To_Archived()
    {
        // Arrange
        var listing = await CreateTestListingAsync(status: ListingStatus.Published);

        // Act
        var result = await _listingAppService.ArchiveAsync(listing.Id);

        // Assert
        result.Status.ShouldBe(ListingStatus.Archived);
    }

    [Fact]
    public async Task MoveToPortfolioAsync_Should_Change_Status_To_Portfolio()
    {
        // Arrange
        var listing = await CreateTestListingAsync(status: ListingStatus.Published);

        // Act
        var result = await _listingAppService.MoveToPortfolioAsync(listing.Id);

        // Assert
        result.Status.ShouldBe(ListingStatus.Portfolio);
    }

    [Fact]
    public async Task UnarchiveAsync_Should_Change_Status_From_Archived_To_Draft()
    {
        // Arrange
        var listing = await CreateTestListingAsync(status: ListingStatus.Archived);

        // Act
        var result = await _listingAppService.UnarchiveAsync(listing.Id);

        // Assert
        result.Status.ShouldBe(ListingStatus.Draft); // Unarchive returns to Draft, not Published
    }

    #endregion

    #region DuplicateAsync Tests

    [Fact]
    public async Task DuplicateAsync_Should_Create_Copy_In_Draft_Status()
    {
        // Arrange
        var original = await CreateTestListingAsync(
            title: "Propiedad Original",
            price: 2500000m,
            status: ListingStatus.Published);

        // Act
        var result = await _listingAppService.DuplicateAsync(original.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(original.Id); // Diferente ID
        result.Title.ShouldContain(original.Title); 
        result.Price.ShouldBe(original.Price); // Mismo precio
        result.Status.ShouldBe(ListingStatus.Draft); // Estado Draft
    }

    #endregion

    #region GetPublishedAsync Tests

    [Fact]
    public async Task GetPublishedAsync_Should_Only_Return_Published_Listings()
    {
        // Arrange
        await CreateTestListingAsync(status: ListingStatus.Published);
        await CreateTestListingAsync(status: ListingStatus.Draft);
        await CreateTestListingAsync(status: ListingStatus.Archived);

        var input = new GetListingsInput { MaxResultCount = 100 };

        // Act
        var result = await _listingAppService.GetPublishedAsync(input);

        // Assert
        result.Items.ShouldAllBe(l => l.Status == ListingStatus.Published);
    }

    #endregion

    #region GetPortfolioAsync Tests

    [Fact]
    public async Task GetPortfolioAsync_Should_Only_Return_Portfolio_Listings()
    {
        // Arrange
        await CreateTestListingAsync(status: ListingStatus.Portfolio);
        await CreateTestListingAsync(status: ListingStatus.Published);

        var input = new GetListingsInput { MaxResultCount = 100 };

        // Act
        var result = await _listingAppService.GetPortfolioAsync(input);

        // Assert
        result.Items.ShouldAllBe(l => l.Status == ListingStatus.Portfolio);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_Should_Filter_By_Price_Range()
    {
        // Arrange
        await CreateTestListingAsync(price: 500000m, status: ListingStatus.Published);
        await CreateTestListingAsync(price: 1500000m, status: ListingStatus.Published);
        await CreateTestListingAsync(price: 3000000m, status: ListingStatus.Published);

        var searchDto = new PropertySearchDto
        {
            MinPrice = 1000000m,
            MaxPrice = 2000000m,
            PageSize = 100
        };

        // Act
        var result = await _listingAppService.SearchAsync(searchDto);

        // Assert
        result.Items.ShouldAllBe(l => l.Price >= 1000000m && l.Price <= 2000000m);
    }

    [Fact]
    public async Task SearchAsync_Should_Filter_By_Category()
    {
        // Arrange
        await CreateTestListingAsync(category: PropertyCategory.Residential, status: ListingStatus.Published);
        await CreateTestListingAsync(category: PropertyCategory.Commercial, status: ListingStatus.Published);

        var searchDto = new PropertySearchDto
        {
            Category = PropertyCategory.Residential,
            PageSize = 100
        };

        // Act
        var result = await _listingAppService.SearchAsync(searchDto);

        // Assert
        result.Items.ShouldAllBe(l => l.Category == PropertyCategory.Residential);
    }

    #endregion

    #region Helper Methods

    private async Task<Listing> CreateTestListingAsync(
        string title = "Test Listing Property",
        decimal price = 1500000m,
        ListingStatus status = ListingStatus.Published,
        PropertyCategory category = PropertyCategory.Residential)
    {
        var architect = await CreateTestArchitectAsync();

        var listing = new Listing(
            Guid.NewGuid(),
            title,
            "Test description with minimum 20 characters required for validation",
            new Address("Test Location Guadalajara Mexico"),
            price,
            200m,
            120m,
            3,
            2,
            category,
            PropertyType.House,
            TransactionType.Sale,
            architect.Id,
            _currentUser.Id
        );

        if (status == ListingStatus.Published)
        {
            listing.AddImage(Guid.NewGuid(), "url", "thumb", "alt", 1024, "image/jpeg");
            listing.Publish(_currentUser.Id);
        }
        else if (status == ListingStatus.Archived)
        {
            listing.Archive(_currentUser.Id);
        }
        else if (status == ListingStatus.Portfolio)
        {
            listing.MoveToPortfolio(_currentUser.Id);
        }

        await WithUnitOfWorkAsync(async () =>
        {
            await _listingRepository.InsertAsync(listing, autoSave: true);
        });

        return listing;
    }

    private async Task<Architect> CreateTestArchitectAsync()
    {
        var adminUserId = _currentUser.Id ?? Guid.NewGuid();

        return await WithUnitOfWorkAsync(async () =>
        {
            var existing = await _architectRepository.FirstOrDefaultAsync(a => a.UserId == adminUserId);
            if (existing != null)
            {
                return existing;
            }

            var architect = new Architect
            {
                UserId = adminUserId,
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            await _architectRepository.InsertAsync(architect, autoSave: true);
            return architect;
        });
    }

    #endregion
}
