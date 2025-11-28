using System;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;
using cima.Domain.Entities;
using cima.Domain.Shared;

namespace cima.Entities;

/// <summary>
/// Tests unitarios para la entidad Listing
/// </summary>
public sealed class ListingTests : cimaDomainTestBase<cimaDomainTestModule>
{
    [Fact]
    public void Should_Create_Listing_With_Parameterless_Constructor()
    {
        // Arrange & Act
        var listing = new Listing();

        // Assert
        listing.ShouldNotBeNull();
        listing.Images.ShouldNotBeNull();
        listing.Images.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Set_Basic_Properties()
    {
        // Arrange
        var listing = new Listing();
        var title = "Casa en Venta - Zona Residencial";
        var description = "Hermosa casa con jardín";
        var location = "Guadalajara, Jalisco";
        var price = 2500000m;
        var area = 150m;

        // Act
        listing.Title = title;
        listing.Description = description;
        listing.Location = location;
        listing.Price = price;
        listing.Area = area;

        // Assert
        listing.Title.ShouldBe(title);
        listing.Description.ShouldBe(description);
        listing.Location.ShouldBe(location);
        listing.Price.ShouldBe(price);
        listing.Area.ShouldBe(area);
    }

    [Fact]
    public void Should_Set_Property_Specifications()
    {
        // Arrange
        var listing = new Listing();

        // Act
        listing.Bedrooms = 3;
        listing.Bathrooms = 2;

        // Assert
        listing.Bedrooms.ShouldBe(3);
        listing.Bathrooms.ShouldBe(2);
    }

    [Theory]
    [InlineData(ListingStatus.Draft)]
    [InlineData(ListingStatus.Published)]
    [InlineData(ListingStatus.Archived)]
    [InlineData(ListingStatus.Portfolio)]
    public void Should_Set_Status(ListingStatus status)
    {
        // Arrange
        var listing = new Listing();

        // Act
        listing.Status = status;

        // Assert
        listing.Status.ShouldBe(status);
    }

    [Theory]
    [InlineData(PropertyCategory.Residential)]
    [InlineData(PropertyCategory.Commercial)]
    [InlineData(PropertyCategory.Mixed)]
    [InlineData(PropertyCategory.Land)]
    public void Should_Set_PropertyCategory(PropertyCategory category)
    {
        // Arrange
        var listing = new Listing();

        // Act
        listing.Category = category;

        // Assert
        listing.Category.ShouldBe(category);
    }

    [Theory]
    [InlineData(PropertyType.House)]
    [InlineData(PropertyType.Apartment)]
    [InlineData(PropertyType.Office)]
    [InlineData(PropertyType.ResidentialLand)]
    public void Should_Set_PropertyType(PropertyType type)
    {
        // Arrange
        var listing = new Listing();

        // Act
        listing.Type = type;

        // Assert
        listing.Type.ShouldBe(type);
    }

    [Theory]
    [InlineData(TransactionType.Sale)]
    [InlineData(TransactionType.Rent)]
    [InlineData(TransactionType.Lease)]
    public void Should_Set_TransactionType(TransactionType transactionType)
    {
        // Arrange
        var listing = new Listing();

        // Act
        listing.TransactionType = transactionType;

        // Assert
        listing.TransactionType.ShouldBe(transactionType);
    }

    [Fact]
    public void Should_Associate_With_Architect()
    {
        // Arrange
        var listing = new Listing();
        var architectId = Guid.NewGuid();

        // Act
        listing.ArchitectId = architectId;

        // Assert
        listing.ArchitectId.ShouldBe(architectId);
    }

    [Fact]
    public void Should_Track_Creation_Metadata()
    {
        // Arrange
        var listing = new Listing();
        var createdAt = DateTime.UtcNow;
        var createdBy = Guid.NewGuid();

        // Act
        listing.CreatedAt = createdAt;
        listing.CreatedBy = createdBy;

        // Assert
        listing.CreatedAt.ShouldBe(createdAt);
        listing.CreatedBy.ShouldBe(createdBy);
    }

    [Fact]
    public void Should_Track_Modification_Metadata()
    {
        // Arrange
        var listing = new Listing();
        var modifiedAt = DateTime.UtcNow;
        var modifiedBy = Guid.NewGuid();

        // Act
        listing.LastModifiedAt = modifiedAt;
        listing.LastModifiedBy = modifiedBy;

        // Assert
        listing.LastModifiedAt.ShouldBe(modifiedAt);
        listing.LastModifiedBy.ShouldBe(modifiedBy);
    }

    [Fact]
    public void Should_Initialize_Empty_Images_Collection()
    {
        // Arrange & Act
        var listing = new Listing();

        // Assert
        listing.Images.ShouldNotBeNull();
        listing.Images.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(100000)]
    [InlineData(2500000)]
    [InlineData(5000000)]
    public void Should_Store_Different_Prices(decimal price)
    {
        // Arrange
        var listing = new Listing();

        // Act
        listing.Price = price;

        // Assert
        listing.Price.ShouldBe(price);
    }

    [Theory]
    [InlineData(50)]
    [InlineData(120)]
    [InlineData(300)]
    public void Should_Store_Different_Areas(decimal area)
    {
        // Arrange
        var listing = new Listing();

        // Act
        listing.Area = area;

        // Assert
        listing.Area.ShouldBe(area);
    }

    [Fact]
    public void Should_Be_AggregateRoot()
    {
        // Arrange & Act
        var listing = new Listing();

        // Assert
        listing.ShouldBeAssignableTo<AggregateRoot<Guid>>();
    }
}
