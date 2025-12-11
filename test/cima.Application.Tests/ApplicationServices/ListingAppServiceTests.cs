using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Listings;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

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

    #region GetListAsync Tests

    [Fact]
    public async Task GetListAsync_Should_Return_Paginated_Results()
    {
        // Arrange
        var input = new GetListingsInput
        {
            SkipCount = 0,
            MaxResultCount = 10
        };

        // Act
        PagedResultDto<ListingDto> result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.GetListAsync(input);
        });

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetListAsync_Should_Filter_By_Status()
    {
        // Arrange
        await CreateTestListingAsync(status: ListingStatus.Published);
        await CreateTestListingAsync(status: ListingStatus.Draft);

        var input = new GetListingsInput
        {
            Status = (int)ListingStatus.Published,
            MaxResultCount = 100
        };

        // Act
        PagedResultDto<ListingDto> result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.GetListAsync(input);
        });

        // Assert
        result.Items.ShouldAllBe(l => l.Status == ListingStatus.Published);
    }

    [Fact]
    public async Task GetListAsync_Should_Filter_By_SearchTerm()
    {
        // Arrange
        await CreateTestListingAsync(title: "Casa en Polanco única");
        await CreateTestListingAsync(title: "Departamento Centro");

        var input = new GetListingsInput
        {
            SearchTerm = "Polanco",
            MaxResultCount = 100
        };

        // Act
        PagedResultDto<ListingDto> result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.GetListAsync(input);
        });

        // Assert
        result.Items.ShouldContain(l => l.Title.Contains("Polanco"));
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_Should_Return_Listing_When_Exists()
    {
        // Arrange
        var listing = await CreateTestListingAsync();

        // Act
        ListingDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.GetAsync(listing.Id);
        });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(listing.Id);
        result.Title.ShouldBe(listing.Title);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_Should_Create_New_Listing_In_Draft_Status()
    {
        // Arrange
        var architect = await CreateTestArchitectAsync();
        var input = new CreateUpdateListingDto
        {
            Title = "Nueva Propiedad Test",
            Description = "Descripción de prueba con más de 20 caracteres para validación",
            Location = "Guadalajara, Jalisco, México",
            Price = 1500000m,
            LandArea = 200m,
            ConstructionArea = 120m,
            Bedrooms = 3,
            Bathrooms = 2,
            Category = PropertyCategory.Residential,
            Type = PropertyType.House,
            TransactionType = TransactionType.Sale,
            ArchitectId = architect.Id
        };

        // Act
        ListingDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.CreateAsync(input);
        });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Title.ShouldBe(input.Title);
        result.Price.ShouldBe(input.Price);
        result.Status.ShouldBe(ListingStatus.Draft); // Debe iniciar en Draft
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Listing()
    {
        // Arrange
        var listing = await CreateTestListingAsync();
        var input = new CreateUpdateListingDto
        {
            Title = "Titulo Actualizado para Test",
            Description = listing.Description,
            Location = "Nueva Ubicacion Actualizada Mexico",
            Price = 2000000m,
            LandArea = listing.LandArea,
            ConstructionArea = listing.ConstructionArea,
            Bedrooms = 4,
            Bathrooms = 3,
            Category = listing.Category,
            Type = listing.Type,
            TransactionType = listing.TransactionType,
            ArchitectId = listing.ArchitectId
        };

        // Act
        ListingDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.UpdateAsync(listing.Id, input);
        });

        // Assert
        result.Title.ShouldBe(input.Title);
        (result.Location?.ToString()).ShouldBe(input.Location);
        result.Price.ShouldBe(2000000m);
        result.Bedrooms.ShouldBe(4);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_Should_Delete_Listing()
    {
        // Arrange
        var listing = await CreateTestListingAsync();
        var listingId = listing.Id;

        // Act
        await WithUnitOfWorkAsync(async () =>
        {
            await _listingAppService.DeleteAsync(listingId);
        });

        // Assert
        bool exists = false;
        await WithUnitOfWorkAsync(async () =>
        {
            exists = await _listingRepository.AnyAsync(l => l.Id == listingId);
        });
        
        exists.ShouldBeFalse();
    }

    #endregion

    #region Status Change Tests

    [Fact]
    public async Task PublishAsync_Should_Change_Status_To_Published()
    {
        // Arrange
        var listing = await CreateTestListingAsync(status: ListingStatus.Draft);

        // Act
        ListingDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.PublishAsync(listing.Id);
        });

        // Assert
        result.Status.ShouldBe(ListingStatus.Published);
    }

    [Fact]
    public async Task ArchiveAsync_Should_Change_Status_To_Archived()
    {
        // Arrange
        var listing = await CreateTestListingAsync(status: ListingStatus.Published);

        // Act
        ListingDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.ArchiveAsync(listing.Id);
        });

        // Assert
        result.Status.ShouldBe(ListingStatus.Archived);
    }

    [Fact]
    public async Task MoveToPortfolioAsync_Should_Change_Status_To_Portfolio()
    {
        // Arrange
        var listing = await CreateTestListingAsync(status: ListingStatus.Published);

        // Act
        ListingDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.MoveToPortfolioAsync(listing.Id);
        });

        // Assert
        result.Status.ShouldBe(ListingStatus.Portfolio);
    }

    [Fact]
    public async Task UnarchiveAsync_Should_Change_Status_From_Archived_To_Published()
    {
        // Arrange
        var listing = await CreateTestListingAsync(status: ListingStatus.Archived);

        // Act
        ListingDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.UnarchiveAsync(listing.Id);
        });

        // Assert
        result.Status.ShouldBe(ListingStatus.Published);
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
        ListingDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.DuplicateAsync(original.Id);
        });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(original.Id); // Diferente ID
        result.Title.ShouldContain("Copia"); // Título con (Copia)
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
        PagedResultDto<ListingDto> result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.GetPublishedAsync(input);
        });

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
        PagedResultDto<ListingDto> result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.GetPortfolioAsync(input);
        });

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
        PagedResultDto<ListingDto> result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.SearchAsync(searchDto);
        });

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
        PagedResultDto<ListingDto> result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _listingAppService.SearchAsync(searchDto);
        });

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

        var listing = new Listing
        {
            Title = title,
            Description = "Test description with minimum 20 characters required for validation",
            Location = "Test Location Guadalajara Mexico",
            Price = price,
            LandArea = 200m,
            ConstructionArea = 120m,
            Bedrooms = 3,
            Bathrooms = 2,
            Category = category,
            Type = PropertyType.House,
            TransactionType = TransactionType.Sale,
            ArchitectId = architect.Id,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        await WithUnitOfWorkAsync(async () =>
        {
            await _listingRepository.InsertAsync(listing, autoSave: true);
        });

        return listing;
    }

    private async Task<Architect> CreateTestArchitectAsync()
    {
        var adminUserId = _currentUser.Id ?? Guid.NewGuid();

        var existing = await WithUnitOfWorkAsync(async () =>
        {
            return await _architectRepository.FirstOrDefaultAsync(a => a.UserId == adminUserId);
        });

        if (existing != null)
        {
            return existing;
        }
        
        var architect = new Architect
        {
            UserId = adminUserId,
            TotalListingsPublished = 0,
            ActiveListings = 0,
            RegistrationDate = DateTime.UtcNow,
            IsActive = true
        };

        await WithUnitOfWorkAsync(async () =>
        {
            await _architectRepository.InsertAsync(architect, autoSave: true);
        });

        return architect;
    }

    #endregion
}
