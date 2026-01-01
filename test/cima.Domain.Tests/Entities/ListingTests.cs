using System;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Xunit;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Entities.Listings;

namespace cima.Entities;

/// <summary>
/// Tests unitarios para la entidad Listing
/// </summary>
public sealed class ListingTests : cimaDomainTestBase<cimaDomainTestModule>
{
    private static Listing CreateTestListing(
        Guid? id = null,
        string title = "Casa de prueba",
        string description = "Descripción de prueba",
        Address? location = null,
        decimal price = 1000000m,
        decimal landArea = 200m,
        decimal constructionArea = 150m,
        int bedrooms = 3,
        int bathrooms = 2,
        PropertyCategory category = PropertyCategory.Residential,
        PropertyType type = PropertyType.House,
        TransactionType transactionType = TransactionType.Sale,
        Guid? architectId = null,
        Guid? createdBy = null)
    {
        return new Listing(
            id ?? Guid.NewGuid(),
            title,
            description,
            location,
            price,
            landArea,
            constructionArea,
            bedrooms,
            bathrooms,
            category,
            type,
            transactionType,
            architectId ?? Guid.NewGuid(),
            createdBy
        );
    }

    [Fact]
    public void Should_Create_Listing_With_Required_Properties()
    {
        // Arrange & Act
        var listing = CreateTestListing();

        // Assert
        listing.ShouldNotBeNull();
        listing.Title.ShouldNotBeNullOrEmpty();
        listing.Description.ShouldNotBeNullOrEmpty();
        listing.Images.ShouldNotBeNull();
        listing.Images.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Allow_Null_Location()
    {
        // Arrange & Act
        var listing = CreateTestListing(location: null);

        // Assert
        listing.Location.ShouldBeNull();
    }

    [Fact]
    public void Should_Set_Location_When_Provided()
    {
        // Arrange
        var location = new Address("Guadalajara, Jalisco");

        // Act
        var listing = CreateTestListing(location: location);

        // Assert
        listing.Location.ShouldBe(location);
    }

    [Fact]
    public void Should_Update_Basic_Properties()
    {
        // Arrange
        var listing = CreateTestListing();
        var title = "Casa en Venta - Zona Residencial";
        var description = "Hermosa casa con jardín";
        var location = new Address("Guadalajara, Jalisco");
        var price = 2500000m;
        var landArea = 250m;
        var constructionArea = 150m;
        var modifiedBy = Guid.NewGuid();

        // Act
        listing.UpdateInfo(
            title,
            description,
            location,
            price,
            landArea,
            constructionArea,
            listing.Bedrooms,
            listing.Bathrooms,
            listing.Category,
            listing.Type,
            listing.TransactionType,
            modifiedBy
        );

        // Assert
        listing.Title.ShouldBe(title);
        listing.Description.ShouldBe(description);
        listing.Location.ShouldBe(location);
        listing.Price.ShouldBe(price);
        listing.LandArea.ShouldBe(landArea);
        listing.ConstructionArea.ShouldBe(constructionArea);
        listing.LastModifiedBy.ShouldBe(modifiedBy);
    }

    [Fact]
    public void Should_Update_Property_Specifications()
    {
        // Arrange
        var listing = CreateTestListing();
        var bedrooms = 3;
        var bathrooms = 2;

        // Act
        listing.UpdateInfo(
            listing.Title,
            listing.Description,
            listing.Location,
            listing.Price,
            listing.LandArea,
            listing.ConstructionArea,
            bedrooms,
            bathrooms,
            listing.Category,
            listing.Type,
            listing.TransactionType,
            null
        );

        // Assert
        listing.Bedrooms.ShouldBe(bedrooms);
        listing.Bathrooms.ShouldBe(bathrooms);
    }

    [Fact]
    public void Should_Update_Status_Via_Methods()
    {
        // Arrange
        var listing = CreateTestListing();
        listing.AddImage(Guid.NewGuid(), "url", "thumb", "alt", 1024, "image/jpeg");

        // Act
        listing.Publish(Guid.NewGuid());
        listing.Status.ShouldBe(ListingStatus.Published);

        listing.Unpublish(Guid.NewGuid());
        listing.Status.ShouldBe(ListingStatus.Draft);

        listing.Archive(Guid.NewGuid());
        listing.Status.ShouldBe(ListingStatus.Archived);

        listing.Unarchive(Guid.NewGuid());
        listing.Status.ShouldBe(ListingStatus.Draft);

        listing.MoveToPortfolio(Guid.NewGuid());
        listing.Status.ShouldBe(ListingStatus.Portfolio);
    }

    [Fact]
    public void Should_Not_Move_To_Portfolio_Without_Images()
    {
        var listing = CreateTestListing();

        var exception = Should.Throw<BusinessException>(() => listing.MoveToPortfolio(Guid.NewGuid()));

        exception.Code.ShouldBe("Listing:NoImages");
    }

    [Fact]
    public void Should_Update_PropertyCategory()
    {
        // Arrange
        var listing = CreateTestListing();
        var category = PropertyCategory.Commercial;

        // Act
        listing.UpdateInfo(
            listing.Title,
            listing.Description,
            listing.Location,
            listing.Price,
            listing.LandArea,
            listing.ConstructionArea,
            listing.Bedrooms,
            listing.Bathrooms,
            category,
            listing.Type,
            listing.TransactionType,
            null
        );

        // Assert
        listing.Category.ShouldBe(category);
    }

    [Fact]
    public void Should_Update_PropertyType()
    {
        // Arrange
        var listing = CreateTestListing();
        var type = PropertyType.Office;

        // Act
        listing.UpdateInfo(
            listing.Title,
            listing.Description,
            listing.Location,
            listing.Price,
            listing.LandArea,
            listing.ConstructionArea,
            listing.Bedrooms,
            listing.Bathrooms,
            listing.Category,
            type,
            listing.TransactionType,
            null
        );

        // Assert
        listing.Type.ShouldBe(type);
    }

    [Fact]
    public void Should_Update_TransactionType()
    {
        // Arrange
        var listing = CreateTestListing();
        var transactionType = TransactionType.Rent;

        // Act
        listing.UpdateInfo(
            listing.Title,
            listing.Description,
            listing.Location,
            listing.Price,
            listing.LandArea,
            listing.ConstructionArea,
            listing.Bedrooms,
            listing.Bathrooms,
            listing.Category,
            listing.Type,
            transactionType,
            null
        );

        // Assert
        listing.TransactionType.ShouldBe(transactionType);
    }

    [Fact]
    public void Should_Track_Creation_Metadata()
    {
        // Arrange & Act
        var createdBy = Guid.NewGuid();
        var listing = CreateTestListing(createdBy: createdBy);

        // Assert
        listing.CreatedBy.ShouldBe(createdBy);
        listing.CreatedAt.ShouldNotBe(default);
    }

    [Fact]
    public void Should_Track_Modification_Metadata()
    {
        // Arrange
        var listing = CreateTestListing();
        var modifiedBy = Guid.NewGuid();

        // Act
        listing.UpdateInfo(
            listing.Title,
            listing.Description,
            listing.Location,
            listing.Price,
            listing.LandArea,
            listing.ConstructionArea,
            listing.Bedrooms,
            listing.Bathrooms,
            listing.Category,
            listing.Type,
            listing.TransactionType,
            modifiedBy
        );

        // Assert
        listing.LastModifiedAt.ShouldNotBeNull();
        listing.LastModifiedBy.ShouldBe(modifiedBy);
    }

    [Fact]
    public void Should_Initialize_Empty_Images_Collection()
    {
        // Arrange & Act
        var listing = CreateTestListing();

        // Assert
        listing.Images.ShouldNotBeNull();
        listing.Images.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Be_AggregateRoot()
    {
        // Arrange & Act
        var listing = CreateTestListing();

        // Assert
        listing.ShouldBeAssignableTo<AggregateRoot<Guid>>();
    }
}
