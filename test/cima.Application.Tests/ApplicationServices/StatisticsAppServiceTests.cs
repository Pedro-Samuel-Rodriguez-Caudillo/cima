using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Statistics;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using cima.Domain.Entities.Listings;

namespace cima.ApplicationServices;

/// <summary>
/// Tests de integración para StatisticsAppService
/// </summary>
public sealed class StatisticsAppServiceTests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly IStatisticsAppService _statisticsAppService;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IRepository<ContactRequest, Guid> _contactRequestRepository;
    private readonly ICurrentUser _currentUser;

    public StatisticsAppServiceTests()
    {
        _statisticsAppService = GetRequiredService<IStatisticsAppService>();
        _listingRepository = GetRequiredService<IRepository<Listing, Guid>>();
        _architectRepository = GetRequiredService<IRepository<Architect, Guid>>();
        _contactRequestRepository = GetRequiredService<IRepository<ContactRequest, Guid>>();
        _currentUser = GetRequiredService<ICurrentUser>();
    }

    #region GetDashboardAsync Tests

    [Fact]
    public async Task GetDashboardAsync_Should_Return_Statistics()
    {
        // Act
        var result = await _statisticsAppService.GetDashboardAsync();

        // Assert
        result.ShouldNotBeNull();
        result.LastUpdated.ShouldNotBe(default);
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Count_Listings_By_Status()
    {
        // Arrange
        await CreateTestListingAsync(status: ListingStatus.Published);
        await CreateTestListingAsync(status: ListingStatus.Draft);
        await CreateTestListingAsync(status: ListingStatus.Portfolio);
        await CreateTestListingAsync(status: ListingStatus.Archived);

        // Act
        var result = await _statisticsAppService.GetDashboardAsync();

        // Assert
        result.TotalListings.ShouldBeGreaterThanOrEqualTo(4);
        result.PublishedListings.ShouldBeGreaterThanOrEqualTo(1);
        result.DraftListings.ShouldBeGreaterThanOrEqualTo(1);
        result.PortfolioListings.ShouldBeGreaterThanOrEqualTo(1);
        result.ArchivedListings.ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task GetDashboardAsync_Should_Count_Contact_Requests()
    {
        // Arrange
        var listing = await CreateTestListingAsync();
        await CreateTestContactRequestAsync(listing.Id, ContactRequestStatus.New);
        await CreateTestContactRequestAsync(listing.Id, ContactRequestStatus.Replied);
        await CreateTestContactRequestAsync(listing.Id, ContactRequestStatus.Closed);

        // Act
        var result = await _statisticsAppService.GetDashboardAsync();

        // Assert
        result.TotalContactRequests.ShouldBeGreaterThanOrEqualTo(3);
        result.PendingContactRequests.ShouldBeGreaterThanOrEqualTo(1);
        result.ClosedContactRequests.ShouldBeGreaterThanOrEqualTo(1);
    }

    #endregion

    #region GetListingStatsAsync Tests

    [Fact]
    public async Task GetListingStatsAsync_Should_Return_Stats_By_Type()
    {
        // Arrange
        await CreateTestListingAsync(type: PropertyType.House);
        await CreateTestListingAsync(type: PropertyType.Apartment);

        // Act
        var result = await _statisticsAppService.GetListingStatsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.ByType.ShouldNotBeNull();
        result.ByStatus.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetListingStatsAsync_Should_Calculate_Average_Price()
    {
        // Arrange
        await CreateTestListingAsync(price: 1000000m, status: ListingStatus.Published);
        await CreateTestListingAsync(price: 2000000m, status: ListingStatus.Published);

        // Act
        var result = await _statisticsAppService.GetListingStatsAsync();

        // Assert
        result.AveragePrice.ShouldBeGreaterThan(0);
    }

    #endregion

    #region GetContactRequestStatsAsync Tests

    [Fact]
    public async Task GetContactRequestStatsAsync_Should_Return_Stats()
    {
        // Arrange
        var listing = await CreateTestListingAsync();
        await CreateTestContactRequestAsync(listing.Id);

        // Act
        var result = await _statisticsAppService.GetContactRequestStatsAsync();

        // Assert
        result.ShouldNotBeNull();
        result.ByStatus.ShouldNotBeNull();
    }

    #endregion

    #region Helper Methods

    private async Task<Listing> CreateTestListingAsync(
        ListingStatus status = ListingStatus.Published,
        PropertyType type = PropertyType.House,
        decimal price = 1500000m)
    {
        var architect = await CreateTestArchitectAsync();

        var listing = new Listing(
            Guid.NewGuid(),
            $"Test Listing {Guid.NewGuid():N}",
            "Test description with minimum 20 characters",
            new Address("Test Location Mexico"),
            price,
            200m,
            120m,
            3,
            2,
            PropertyCategory.Residential,
            type,
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
        var adminUserId = Guid.NewGuid(); // usar siempre un usuario nuevo para evitar colisiones en tests

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

    private async Task<ContactRequest> CreateTestContactRequestAsync(
        Guid listingId,
        ContactRequestStatus status = ContactRequestStatus.New)
    {
        Listing listing = null!;
        await WithUnitOfWorkAsync(async () =>
        {
            listing = await _listingRepository.GetAsync(listingId);
        });

        var contactRequest = new ContactRequest
        {
            ListingId = listingId,
            ArchitectId = listing.ArchitectId,
            Name = "Test User",
            Email = $"test{Guid.NewGuid():N}@example.com",
            Phone = "1234567890",
            Message = "Test message for contact request",
            Status = status,
            CreatedAt = DateTime.UtcNow
        };

        if (status == ContactRequestStatus.Replied)
        {
            contactRequest.RepliedAt = DateTime.UtcNow;
            contactRequest.ReplyNotes = "Test reply";
        }

        await WithUnitOfWorkAsync(async () =>
        {
            await _contactRequestRepository.InsertAsync(contactRequest, autoSave: true);
        });

        return contactRequest;
    }

    #endregion
}
