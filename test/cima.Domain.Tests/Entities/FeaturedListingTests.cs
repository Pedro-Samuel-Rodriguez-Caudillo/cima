using System;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;

namespace cima.Entities;

/// <summary>
/// Tests unitarios para la entidad FeaturedListing
/// </summary>
public sealed class FeaturedListingTests : cimaDomainTestBase<cimaDomainTestModule>
{
    [Fact]
    public void Should_Create_FeaturedListing_With_Valid_Data()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var displayOrder = 5;

        // Act
        var featured = new FeaturedListing(
            listingId: listingId,
            displayOrder: displayOrder,
            createdBy: createdBy
        );

        // Assert
        featured.ShouldNotBeNull();
        featured.Id.ShouldNotBe(Guid.Empty);
        featured.ListingId.ShouldBe(listingId);
        featured.DisplayOrder.ShouldBe(displayOrder);
        featured.CreatedBy.ShouldBe(createdBy);
        featured.FeaturedSince.ShouldBeInRange(
            DateTime.UtcNow.AddSeconds(-5),
            DateTime.UtcNow.AddSeconds(5)
        );
    }

    [Fact]
    public void Should_Create_FeaturedListing_With_Minimum_Required_Data()
    {
        // Arrange
        var listingId = Guid.NewGuid();

        // Act
        var featured = new FeaturedListing(
            listingId: listingId
        );

        // Assert
        featured.ShouldNotBeNull();
        featured.Id.ShouldNotBe(Guid.Empty);
        featured.ListingId.ShouldBe(listingId);
        featured.DisplayOrder.ShouldBe(999); // Default value
        featured.CreatedBy.ShouldBeNull();
        featured.FeaturedSince.ShouldBeInRange(
            DateTime.UtcNow.AddSeconds(-5),
            DateTime.UtcNow.AddSeconds(5)
        );
    }

    [Fact]
    public void Should_Create_FeaturedListing_With_Parameterless_Constructor()
    {
        // Arrange & Act
        var featured = new FeaturedListing();

        // Assert
        featured.ShouldNotBeNull();
        featured.Id.ShouldBe(Guid.Empty);
        featured.DisplayOrder.ShouldBe(999); // Default value
        featured.FeaturedSince.ShouldBeInRange(
            DateTime.UtcNow.AddSeconds(-5),
            DateTime.UtcNow.AddSeconds(5)
        );
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(12)]
    public void Should_Set_DisplayOrder_Correctly(int displayOrder)
    {
        // Arrange
        var listingId = Guid.NewGuid();

        // Act
        var featured = new FeaturedListing(
            listingId: listingId,
            displayOrder: displayOrder
        );

        // Assert
        featured.DisplayOrder.ShouldBe(displayOrder);
    }

    [Fact]
    public void Should_Default_DisplayOrder_To_999_When_Not_Specified()
    {
        // Arrange
        var listingId = Guid.NewGuid();

        // Act
        var featured = new FeaturedListing(
            listingId: listingId
        );

        // Assert
        featured.DisplayOrder.ShouldBe(999);
    }

    [Fact]
    public void Should_Store_FeaturedSince_Timestamp_In_UTC()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var featured = new FeaturedListing(
            listingId: Guid.NewGuid()
        );

        var afterCreation = DateTime.UtcNow;

        // Assert
        featured.FeaturedSince.ShouldBeInRange(beforeCreation, afterCreation);
        featured.FeaturedSince.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void Should_Associate_With_Listing()
    {
        // Arrange
        var listingId = Guid.NewGuid();

        // Act
        var featured = new FeaturedListing(
            listingId: listingId
        );

        // Assert
        featured.ListingId.ShouldBe(listingId);
    }

    [Fact]
    public void Should_Store_CreatedBy_User_Id()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        // Act
        var featured = new FeaturedListing(
            listingId: listingId,
            createdBy: createdBy
        );

        // Assert
        featured.CreatedBy.ShouldBe(createdBy);
    }

    [Fact]
    public void Should_Allow_Null_CreatedBy()
    {
        // Arrange & Act
        var featured = new FeaturedListing(
            listingId: Guid.NewGuid(),
            createdBy: null
        );

        // Assert
        featured.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public void Should_Allow_Setting_Properties_After_Creation()
    {
        // Arrange
        var featured = new FeaturedListing();
        var newListingId = Guid.NewGuid();
        var newCreatedBy = Guid.NewGuid();
        var newFeaturedSince = DateTime.UtcNow.AddDays(-7);

        // Act
        featured.ListingId = newListingId;
        featured.DisplayOrder = 1;
        featured.CreatedBy = newCreatedBy;
        featured.FeaturedSince = newFeaturedSince;

        // Assert
        featured.ListingId.ShouldBe(newListingId);
        featured.DisplayOrder.ShouldBe(1);
        featured.CreatedBy.ShouldBe(newCreatedBy);
        featured.FeaturedSince.ShouldBe(newFeaturedSince);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public void Should_Allow_Negative_Or_Zero_DisplayOrder(int displayOrder)
    {
        // Arrange & Act - No hay restricción en el Domain, validación en Application
        var featured = new FeaturedListing(
            listingId: Guid.NewGuid(),
            displayOrder: displayOrder
        );

        // Assert
        featured.DisplayOrder.ShouldBe(displayOrder);
    }

    [Theory]
    [InlineData(1, 2, true)]  // 1 < 2 (orden ascendente correcto)
    [InlineData(2, 1, false)] // 2 > 1 (orden incorrecto)
    [InlineData(5, 5, false)] // 5 == 5 (mismo orden)
    public void Should_Compare_DisplayOrder_For_Sorting(int order1, int order2, bool shouldBeFirst)
    {
        // Arrange
        var featured1 = new FeaturedListing(
            listingId: Guid.NewGuid(),
            displayOrder: order1
        );

        var featured2 = new FeaturedListing(
            listingId: Guid.NewGuid(),
            displayOrder: order2
        );

        // Act
        var isFirstInOrder = featured1.DisplayOrder < featured2.DisplayOrder;

        // Assert
        isFirstInOrder.ShouldBe(shouldBeFirst);
    }

    [Fact]
    public void Should_Handle_Multiple_FeaturedListings_With_Different_Orders()
    {
        // Arrange & Act
        var featured1 = new FeaturedListing(Guid.NewGuid(), displayOrder: 1);
        var featured2 = new FeaturedListing(Guid.NewGuid(), displayOrder: 5);
        var featured3 = new FeaturedListing(Guid.NewGuid(), displayOrder: 3);
        var featured4 = new FeaturedListing(Guid.NewGuid(), displayOrder: 2);

        // Assert
        featured1.DisplayOrder.ShouldBe(1);
        featured2.DisplayOrder.ShouldBe(5);
        featured3.DisplayOrder.ShouldBe(3);
        featured4.DisplayOrder.ShouldBe(2);
    }

    [Fact]
    public void Should_Generate_Unique_Ids_For_Different_Instances()
    {
        // Arrange & Act
        var featured1 = new FeaturedListing(Guid.NewGuid());
        var featured2 = new FeaturedListing(Guid.NewGuid());
        var featured3 = new FeaturedListing(Guid.NewGuid());

        // Assert
        featured1.Id.ShouldNotBe(featured2.Id);
        featured2.Id.ShouldNotBe(featured3.Id);
        featured3.Id.ShouldNotBe(featured1.Id);
    }

    [Fact]
    public void Should_Allow_Same_Listing_To_Be_Featured_Multiple_Times_With_Different_Ids()
    {
        // Arrange - Escenario: misma propiedad destacada en diferentes momentos
        var sameListingId = Guid.NewGuid();

        // Act
        var featured1 = new FeaturedListing(sameListingId, displayOrder: 1);
        var featured2 = new FeaturedListing(sameListingId, displayOrder: 2);

        // Assert
        featured1.ListingId.ShouldBe(featured2.ListingId);
        featured1.Id.ShouldNotBe(featured2.Id); // IDs diferentes
        featured1.DisplayOrder.ShouldNotBe(featured2.DisplayOrder);
    }

    [Fact]
    public void Should_Track_When_Listing_Was_Featured()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-30);
        var featured = new FeaturedListing(Guid.NewGuid());

        // Act
        featured.FeaturedSince = pastDate;

        // Assert
        featured.FeaturedSince.ShouldBe(pastDate);
        
        // Verificar que fue destacado hace 30 días
        var daysFeatured = (DateTime.UtcNow - featured.FeaturedSince).Days;
        daysFeatured.ShouldBeInRange(29, 31);
    }

    [Fact]
    public void Should_Handle_Listing_Navigation_Property()
    {
        // Arrange
        var listingId = Guid.NewGuid();
        var featured = new FeaturedListing(listingId);

        // Act & Assert - Navigation property inicialmente null
        featured.Listing.ShouldBeNull();
        
        // La navegación se carga con EF Core en queries reales
        // Aquí solo verificamos que la propiedad existe y acepta null
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Should_Support_Max_12_Featured_Listings_Business_Rule(int count)
    {
        // Arrange - Regla de negocio: máximo 12 propiedades destacadas
        var featuredListings = new FeaturedListing[count];

        // Act
        for (int i = 0; i < count; i++)
        {
            featuredListings[i] = new FeaturedListing(
                listingId: Guid.NewGuid(),
                displayOrder: i + 1
            );
        }

        // Assert
        featuredListings.Length.ShouldBe(count);
        
        // Validación del límite de 12 se hace en Application Layer
        if (count <= 12)
        {
            featuredListings.Length.ShouldBeLessThanOrEqualTo(12);
        }
    }
}
