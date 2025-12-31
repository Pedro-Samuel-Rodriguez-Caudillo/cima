using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Security.Principal;
using Shouldly;
using Xunit;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.Security.Claims;
using cima.ContactRequests;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Entities.Listings;

namespace cima.ApplicationServices;

public sealed class ContactRequestAppService_Tests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly IContactRequestAppService _contactRequestAppService;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public ContactRequestAppService_Tests()
    {
        _contactRequestAppService = GetRequiredService<IContactRequestAppService>();
        _listingRepository = GetRequiredService<IRepository<Listing, Guid>>();
        _architectRepository = GetRequiredService<IRepository<Architect, Guid>>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private IDisposable Login(Guid userId, string[]? roles = null)
    {
        var claims = new List<Claim>
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, "testuser"),
            new Claim(AbpClaimTypes.Email, "test@example.com")
        };

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(AbpClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        return _currentPrincipalAccessor.Change(principal);
    }

    [Fact]
    public async Task Should_Create_Contact_Request_For_Listing()
    {
        // 1. Arrange: Create a listing in PUBLISHED status
        var listingId = Guid.NewGuid();
        var architectId = Guid.Empty;

        await WithUnitOfWorkAsync(async () => {
            var a = new Architect { UserId = Guid.NewGuid(), IsActive = true, RegistrationDate = DateTime.UtcNow };
            var savedA = await _architectRepository.InsertAsync(a, autoSave: true);
            architectId = savedA.Id;

            var listing = new Listing(
                listingId,
                "Property for Contact",
                "Description long enough",
                new Address("Location"),
                1000, 100, 100, 1, 1,
                PropertyCategory.Residential, PropertyType.House, TransactionType.Sale,
                architectId, Guid.NewGuid()
            );
            
            // WE MUST PUBLISH OR PORTFOLIO FOR CONTACT REQUESTS TO WORK
            listing.AddImage(Guid.NewGuid(), "url", "thumb", "alt", 1024, "image/jpeg");
            listing.Publish(null);
            
            await _listingRepository.InsertAsync(listing, autoSave: true);
        });

        var input = new CreateContactRequestDto
        {
            ListingId = listingId,
            Name = "Interested Buyer",
            Email = "buyer@example.com",
            Phone = "1234567890",
            Message = "I would like to visit this property."
        };

        // 2. Act
        var result = await _contactRequestAppService.CreateAsync(input);

        // 3. Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(input.Name);
        result.Email.ShouldBe(input.Email);
        result.Status.ShouldBe(ContactRequestStatus.New);
        result.ListingId.ShouldBe(listingId);
        result.ArchitectId.ShouldBe(architectId);
    }

    [Fact]
    public async Task Should_Mark_As_Replied()
    {
        // 1. Arrange: Create a request
        var listingId = Guid.NewGuid();
        var architectId = Guid.Empty;
        var userId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () => {
            var a = new Architect { UserId = userId, IsActive = true, RegistrationDate = DateTime.UtcNow };
            var savedA = await _architectRepository.InsertAsync(a, autoSave: true);
            architectId = savedA.Id;

            var listing = new Listing(
                listingId, "Prop", "Desc long enough", new Address("Loc"), 1000, 100, 100, 1, 1,
                PropertyCategory.Residential, PropertyType.House, TransactionType.Sale, architectId, userId
            );
            listing.AddImage(Guid.NewGuid(), "url", "thumb", "alt", 1024, "image/jpeg");
            listing.Publish(userId);
            await _listingRepository.InsertAsync(listing, autoSave: true);
        });

        var createResult = await _contactRequestAppService.CreateAsync(new CreateContactRequestDto
        {
            ListingId = listingId,
            Name = "Test",
            Email = "test@test.com",
            Message = "Test message long enough"
        });

        // 2. Act
        using (Login(userId)) // Log in as the architect owner
        {
            await _contactRequestAppService.MarkAsRepliedAsync(createResult.Id, new MarkAsRepliedDto
            {
                ReplyNotes = "Sent email to buyer."
            });

            // 3. Assert
            var updated = await _contactRequestAppService.GetAsync(createResult.Id);
            updated.Status.ShouldBe(ContactRequestStatus.Replied);
            updated.ReplyNotes.ShouldBe("Sent email to buyer.");
            updated.RepliedAt.ShouldNotBeNull();
        }
    }
}