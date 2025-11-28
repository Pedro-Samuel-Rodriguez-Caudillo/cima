using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Shared.Dtos;
using cima.ContactRequests;
using Volo.Abp.Domain.Repositories;

namespace cima.ApplicationServices;

/// <summary>
/// Tests básicos de integración para ContactRequestAppService
/// </summary>
public sealed class ContactRequestAppServiceTests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly IContactRequestAppService _contactRequestAppService;
    private readonly IRepository<ContactRequest, Guid> _contactRequestRepository;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;

    public ContactRequestAppServiceTests()
    {
        _contactRequestAppService = GetRequiredService<IContactRequestAppService>();
        _contactRequestRepository = GetRequiredService<IRepository<ContactRequest, Guid>>();
        _listingRepository = GetRequiredService<IRepository<Listing, Guid>>();
        _architectRepository = GetRequiredService<IRepository<Architect, Guid>>();
    }

    #region GetListAsync Tests

    [Fact]
    public async Task GetListAsync_Should_Return_Paginated_Results()
    {
        // Arrange
        int skipCount = 0;
        int maxResultCount = 10;

        // Act
        PagedResultDto<ContactRequestDto> result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _contactRequestAppService.GetListAsync(skipCount, maxResultCount);
        });

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeNull();
    }

    #endregion

    #region GetAsync Tests

    [Fact]
    public async Task GetAsync_Should_Return_ContactRequest_When_Exists()
    {
        // Arrange
        var contactRequest = await CreateTestContactRequestAsync();

        // Act
        ContactRequestDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _contactRequestAppService.GetAsync(contactRequest.Id);
        });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(contactRequest.Id);
        result.Name.ShouldBe(contactRequest.Name);
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_Should_Create_New_ContactRequest()
    {
        // Arrange
        var listing = await CreateTestListingAsync();
        
        var input = new CreateContactRequestDto
        {
            ListingId = listing.Id,
            Name = "Juan Pérez García",
            Email = "juan.perez@example.com",
            Phone = "+52 33 1234 5678",
            Message = "Me interesa conocer más detalles sobre esta propiedad"
        };

        // Act
        ContactRequestDto result = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            result = await _contactRequestAppService.CreateAsync(input);
        });

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe(input.Name);
        result.Email.ShouldBe(input.Email);
        result.Status.ShouldBe(ContactRequestStatus.New);
    }

    #endregion

    #region Helper Methods

    private async Task<ContactRequest> CreateTestContactRequestAsync()
    {
        var listing = await CreateTestListingAsync();
        
        var contactRequest = new ContactRequest
        {
            ListingId = listing.Id,
            ArchitectId = listing.ArchitectId,
            Name = "Test User Name",
            Email = "test@example.com",
            Phone = "+52 33 0000 0000",
            Message = "This is a test message",
            Status = ContactRequestStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        await WithUnitOfWorkAsync(async () =>
        {
            await _contactRequestRepository.InsertAsync(contactRequest, autoSave: true);
        });

        return contactRequest;
    }

    private async Task<Listing> CreateTestListingAsync()
    {
        var architect = await CreateTestArchitectAsync();

        var listing = new Listing
        {
            Title = "Test Property for Contact",
            Description = "Test description with enough characters for validation purposes",
            Location = "Test Location City Country",
            Price = 1500000m,
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
            Bio = "Test Architect Bio",
            PortfolioUrl = "https://test.com"
        };

        await WithUnitOfWorkAsync(async () =>
        {
            await _architectRepository.InsertAsync(architect, autoSave: true);
        });

        return architect;
    }

    #endregion
}
