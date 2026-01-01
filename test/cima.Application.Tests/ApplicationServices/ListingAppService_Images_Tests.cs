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
using cima.Listings;
using cima.Listings.Inputs;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Entities.Listings;

namespace cima.ApplicationServices;

public sealed class ListingAppService_Images_Tests : cimaApplicationTestBase<cimaApplicationTestModule>
{
    private readonly IListingAppService _listingAppService;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly ICurrentPrincipalAccessor _currentPrincipalAccessor;

    public ListingAppService_Images_Tests()
    {
        _listingAppService = GetRequiredService<IListingAppService>();
        _listingRepository = GetRequiredService<IRepository<Listing, Guid>>();
        _architectRepository = GetRequiredService<IRepository<Architect, Guid>>();
        _currentPrincipalAccessor = GetRequiredService<ICurrentPrincipalAccessor>();
    }

    private IDisposable Login(Guid userId)
    {
        var claims = new[]
        {
            new Claim(AbpClaimTypes.UserId, userId.ToString()),
            new Claim(AbpClaimTypes.UserName, "testuser"),
            new Claim(AbpClaimTypes.Email, "test@example.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        return _currentPrincipalAccessor.Change(principal);
    }

    [Fact]
    public async Task Should_Add_And_Reorder_Images()
    {
        var userId = Guid.NewGuid();
        using (Login(userId))
        {
            // 1. Arrange: Create a listing
            var listingId = Guid.NewGuid();
            var architectId = Guid.Empty;

            await WithUnitOfWorkAsync(async () => {
                var a = new Architect { UserId = userId, IsActive = true, RegistrationDate = DateTime.UtcNow };
                var savedA = await _architectRepository.InsertAsync(a, autoSave: true);
                architectId = savedA.Id;

                var listing = new Listing(
                    listingId,
                    "Integration Test Prop",
                    "Description long enough for validation",
                    new Address("Location"),
                    1000, 100, 100, 1, 1,
                    PropertyCatalogIds.Categories.Residential, PropertyCatalogIds.Types.House, TransactionType.Sale,
                    architectId, userId
                );
                await _listingRepository.InsertAsync(listing, autoSave: true);
            });
            
            // 2. Act: Add 3 images
            var img1 = await _listingAppService.AddImageAsync(new AddListingImageDto
            {
                ListingId = listingId,
                Url = "https://example.com/img1.jpg",
                AltText = "Image 1",
                FileSize = 100,
                ContentType = "image/jpeg"
            });

            var img2 = await _listingAppService.AddImageAsync(new AddListingImageDto
            {
                ListingId = listingId,
                Url = "https://example.com/img2.jpg",
                AltText = "Image 2",
                FileSize = 200,
                ContentType = "image/jpeg"
            });

            var img3 = await _listingAppService.AddImageAsync(new AddListingImageDto
            {
                ListingId = listingId,
                Url = "https://example.com/img3.jpg",
                AltText = "Image 3",
                FileSize = 300,
                ContentType = "image/jpeg"
            });

            // 3. Act: Reorder (Reverse them: img3, img2, img1)
            var reorderInput = new List<UpdateImageOrderDto>
            {
                new UpdateImageOrderDto { ImageId = img3.ImageId, DisplayOrder = 0 },
                new UpdateImageOrderDto { ImageId = img2.ImageId, DisplayOrder = 1 },
                new UpdateImageOrderDto { ImageId = img1.ImageId, DisplayOrder = 2 }
            };

            await _listingAppService.UpdateImagesOrderAsync(listingId, reorderInput);

            // 4. Assert: Check database state
            await WithUnitOfWorkAsync(async () => {
                var query = await _listingRepository.WithDetailsAsync(l => l.Images);
                var dbListing = query.First(l => l.Id == listingId);
                
                dbListing.Images.Count.ShouldBe(3);
                
                // We check that SortOrder is preserved and correctly assigned
                var imagesInDb = dbListing.Images.ToList();
                
                var dbImg1 = imagesInDb.First(i => i.AltText == "Image 1");
                var dbImg2 = imagesInDb.First(i => i.AltText == "Image 2");
                var dbImg3 = imagesInDb.First(i => i.AltText == "Image 3");

                // Reverse order check by SortOrder
                dbImg3.SortOrder.ShouldBe(0);
                dbImg2.SortOrder.ShouldBe(1);
                dbImg1.SortOrder.ShouldBe(2);
            });
        }
    }

    [Fact]
    public async Task Should_Remove_Image_And_Reindex()
    {
        var userId = Guid.NewGuid();
        using (Login(userId))
        {
            // 1. Arrange: Create listing
            var listingId = Guid.NewGuid();
            await WithUnitOfWorkAsync(async () => {
                var a = new Architect { UserId = userId, IsActive = true, RegistrationDate = DateTime.UtcNow };
                var savedA = await _architectRepository.InsertAsync(a, autoSave: true);

                var listing = new Listing(
                    listingId,
                    "Integration Test Prop",
                    "Description long enough for validation",
                    new Address("Location"),
                    1000, 100, 100, 1, 1,
                    PropertyCatalogIds.Categories.Residential, PropertyCatalogIds.Types.House, TransactionType.Sale,
                    savedA.Id, userId
                );
                await _listingRepository.InsertAsync(listing, autoSave: true);
            });

            await _listingAppService.AddImageAsync(new AddListingImageDto { ListingId = listingId, Url = "url1", AltText = "Img1", ContentType = "image/jpeg" });
            var img2 = await _listingAppService.AddImageAsync(new AddListingImageDto { ListingId = listingId, Url = "url2", AltText = "Img2", ContentType = "image/jpeg" });
            await _listingAppService.AddImageAsync(new AddListingImageDto { ListingId = listingId, Url = "url3", AltText = "Img3", ContentType = "image/jpeg" });

            // 2. Act: Remove the middle image (img2)
            await _listingAppService.RemoveImageAsync(listingId, img2.ImageId);

            // 3. Assert
            await WithUnitOfWorkAsync(async () => {
                var query = await _listingRepository.WithDetailsAsync(l => l.Images);
                var dbListing = query.First(l => l.Id == listingId);
                
                dbListing.Images.Count.ShouldBe(2);
                dbListing.Images.Any(i => i.AltText == "Img2").ShouldBeFalse();
                
                var orderedImages = dbListing.Images.OrderBy(i => i.SortOrder).ToList();
                orderedImages[0].AltText.ShouldBe("Img1");
                orderedImages[1].AltText.ShouldBe("Img3");
                
                orderedImages[0].SortOrder.ShouldBe(0);
                orderedImages[1].SortOrder.ShouldBe(1);
            });
        }
    }
}
