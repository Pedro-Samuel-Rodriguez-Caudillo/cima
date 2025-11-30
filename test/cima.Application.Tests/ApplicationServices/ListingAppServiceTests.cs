using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Shared.Dtos;
using cima.Listings;
using Volo.Abp.Domain.Repositories;

namespace cima.ApplicationServices;

/// <summary>
/// Tests básicos de integración para ListingAppService
/// </summary>
public sealed class ListingAppServiceTests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly IListingAppService _listingAppService;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;

    public ListingAppServiceTests()
    {
        _listingAppService = GetRequiredService<IListingAppService>();
        _listingRepository = GetRequiredService<IRepository<Listing, Guid>>();
        _architectRepository = GetRequiredService<IRepository<Architect, Guid>>();
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
    public async Task CreateAsync_Should_Create_New_Listing()
    {
        // Arrange
        var architect = await CreateTestArchitectAsync();
        var input = new CreateUpdateListingDto
        {
            Title = "Nueva Propiedad Test",
            Description = "Descripción de prueba con más de 20 caracteres para validación",
            Location = "Guadalajara, Jalisco, México",
            Price = 1500000m,
            Area = 120m,
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
            Title = "Título Actualizado para Test",
            Description = listing.Description,
            Location = "Nueva Ubicación Actualizada México",
            Price = 2000000m,
            Area = listing.Area,
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
        result.Title.ShouldBe("Título Actualizado para Test");
        result.Location.ShouldBe("Nueva Ubicación Actualizada México");
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

        // Act
        await WithUnitOfWorkAsync(async () =>
        {
            await _listingAppService.DeleteAsync(listing.Id);
        });

        // Assert
        var exists = await _listingRepository.AnyAsync(l => l.Id == listing.Id);
        exists.ShouldBeFalse();
    }

    #endregion

    #region Helper Methods

    private async Task<Listing> CreateTestListingAsync(
        string title = "Test Listing Property",
        decimal price = 1500000m)
    {
        var architect = await CreateTestArchitectAsync();

        var listing = new Listing
        {
            Title = title,
            Description = "Test description with minimum 20 characters required for validation",
            Location = "Test Location Guadalajara Mexico",
            Price = price,
            Area = 120m,
            Bedrooms = 3,
            Bathrooms = 2,
            Category = PropertyCategory.Residential,
            Type = PropertyType.House,
            TransactionType = TransactionType.Sale,
            ArchitectId = architect.Id,
            Status = ListingStatus.Published,
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
        var architect = new Architect
        {
            UserId = Guid.NewGuid(),
            Name = "Test Architect",  // ? Name es required
            Bio = "Test Architect Biography"
            // PortfolioUrl eliminado
        };

        await WithUnitOfWorkAsync(async () =>
        {
            await _architectRepository.InsertAsync(architect, autoSave: true);
        });

        return architect;
    }

    #endregion
}
