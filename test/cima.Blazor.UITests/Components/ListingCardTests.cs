using Bunit;
using Xunit;
using cima.Blazor.Client.Components.Public;
using cima.Domain.Shared.Dtos;
using cima.Domain.Shared;
using System;

namespace cima.Blazor.UITests.Components;

public class ListingCardTests : Bunit.TestContext
{
    [Fact]
    public void ListingCard_Should_RenderWithValidData()
    {
        // Arrange
        var listing = new ListingDto
        {
            Id = Guid.NewGuid(),
            Title = "Beautiful House",
            Description = "A wonderful property",
            Price = 500000,
            Type = PropertyType.House,
            Category = PropertyCategory.Residential,
            Status = ListingStatus.Published,
            Location = "123 Main St, Test City",
            Bedrooms = 3,
            Bathrooms = 2,
            Area = 2000,
            TransactionType = TransactionType.Sale,
            ArchitectId = Guid.NewGuid()
        };

        // Act
        var cut = RenderComponent<ListingCard>(parameters => parameters
            .Add(p => p.Listing, listing));

        // Assert
        Assert.Contains("Beautiful House", cut.Markup);
    }

    [Fact]
    public void ListingCard_Should_DisplayPrice()
    {
        // Arrange
        var listing = new ListingDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Property",
            Price = 750000,
            Type = PropertyType.House,
            Category = PropertyCategory.Residential,
            Status = ListingStatus.Published,
            Location = "Test Location",
            Area = 100,
            ArchitectId = Guid.NewGuid()
        };

        // Act
        var cut = RenderComponent<ListingCard>(parameters => parameters
            .Add(p => p.Listing, listing));

        // Assert
        Assert.Contains("750", cut.Markup);
    }

    [Fact]
    public void ListingCard_Should_DisplayPropertyDetails()
    {
        // Arrange
        var listing = new ListingDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Property",
            Price = 500000,
            Type = PropertyType.House,
            Category = PropertyCategory.Residential,
            Status = ListingStatus.Published,
            Location = "Test Location",
            Bedrooms = 4,
            Bathrooms = 3,
            Area = 2500,
            ArchitectId = Guid.NewGuid()
        };

        // Act
        var cut = RenderComponent<ListingCard>(parameters => parameters
            .Add(p => p.Listing, listing));

        // Assert
        Assert.Contains("4", cut.Markup); // Bedrooms
        Assert.Contains("3", cut.Markup); // Bathrooms
        Assert.Contains("2500", cut.Markup); // Area
    }

    [Fact]
    public void ListingCard_Should_HandleNullListing()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            RenderComponent<ListingCard>(parameters => parameters
                .Add(p => p.Listing, null!));
        });
    }
}
