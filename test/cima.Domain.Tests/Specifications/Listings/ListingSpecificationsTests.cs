using System;
using System.Collections.Generic;
using System.Linq;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Specifications.Listings;
using Shouldly;
using Volo.Abp.Specifications;
using Xunit;

namespace cima.Specifications.Listings;

/// <summary>
/// Tests unitarios para las Specifications de Listings.
/// </summary>
public sealed class ListingSpecificationsTests : cimaDomainTestBase<cimaDomainTestModule>
{
    #region PublishedListingSpecification Tests

    [Fact]
    public void PublishedListingSpecification_Should_Return_Only_Published()
    {
        // Arrange
        var listings = CreateTestListings();
        var spec = new PublishedListingSpecification();

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.Status == ListingStatus.Published);
        result.Count.ShouldBe(2); // Solo las publicadas
    }

    [Fact]
    public void PublishedListingSpecification_Should_Exclude_Draft_Archived_Portfolio()
    {
        // Arrange
        var listings = CreateTestListings();
        var spec = new PublishedListingSpecification();

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldNotContain(l => l.Status == ListingStatus.Draft);
        result.ShouldNotContain(l => l.Status == ListingStatus.Archived);
        result.ShouldNotContain(l => l.Status == ListingStatus.Portfolio);
    }

    #endregion

    #region PortfolioListingSpecification Tests

    [Fact]
    public void PortfolioListingSpecification_Should_Return_Only_Portfolio()
    {
        // Arrange
        var listings = CreateTestListings();
        var spec = new PortfolioListingSpecification();

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.Status == ListingStatus.Portfolio);
        result.Count.ShouldBe(1);
    }

    #endregion

    #region PublicVisibleListingSpecification Tests

    [Fact]
    public void PublicVisibleListingSpecification_Should_Return_Published_And_Portfolio()
    {
        // Arrange
        var listings = CreateTestListings();
        var spec = new PublicVisibleListingSpecification();

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => 
            l.Status == ListingStatus.Published || 
            l.Status == ListingStatus.Portfolio);
        result.Count.ShouldBe(3); // 2 Published + 1 Portfolio
    }

    [Fact]
    public void PublicVisibleListingSpecification_Should_Exclude_Draft_And_Archived()
    {
        // Arrange
        var listings = CreateTestListings();
        var spec = new PublicVisibleListingSpecification();

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldNotContain(l => l.Status == ListingStatus.Draft);
        result.ShouldNotContain(l => l.Status == ListingStatus.Archived);
    }

    #endregion

    #region ListingByArchitectSpecification Tests

    [Fact]
    public void ListingByArchitectSpecification_Should_Filter_By_ArchitectId()
    {
        // Arrange
        var targetArchitectId = Guid.NewGuid();
        var listings = new List<Listing>
        {
            CreateListing("Casa 1", targetArchitectId, ListingStatus.Published),
            CreateListing("Casa 2", targetArchitectId, ListingStatus.Draft),
            CreateListing("Casa 3", Guid.NewGuid(), ListingStatus.Published),
            CreateListing("Casa 4", Guid.NewGuid(), ListingStatus.Published),
        };
        var spec = new ListingByArchitectSpecification(targetArchitectId);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.ArchitectId == targetArchitectId);
        result.Count.ShouldBe(2);
    }

    [Fact]
    public void ListingByArchitectSpecification_Should_Return_Empty_When_No_Match()
    {
        // Arrange
        var listings = CreateTestListings();
        var nonExistentArchitectId = Guid.NewGuid();
        var spec = new ListingByArchitectSpecification(nonExistentArchitectId);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldBeEmpty();
    }

    #endregion

    #region ListingSearchSpecification Tests

    [Fact]
    public void ListingSearchSpecification_Should_Filter_By_TransactionType()
    {
        // Arrange
        var listings = CreateSearchTestListings();
        var criteria = new ListingSearchCriteria
        {
            TransactionType = TransactionType.Sale
        };
        var spec = new ListingSearchSpecification(criteria);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.TransactionType == TransactionType.Sale);
    }

    [Fact]
    public void ListingSearchSpecification_Should_Filter_By_Category()
    {
        // Arrange
        var listings = CreateSearchTestListings();
        var criteria = new ListingSearchCriteria
        {
            Category = PropertyCategory.Residential
        };
        var spec = new ListingSearchSpecification(criteria);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.Category == PropertyCategory.Residential);
    }

    [Fact]
    public void ListingSearchSpecification_Should_Filter_By_PriceRange()
    {
        // Arrange
        var listings = CreateSearchTestListings();
        var criteria = new ListingSearchCriteria
        {
            MinPrice = 1500000m,
            MaxPrice = 3000000m
        };
        var spec = new ListingSearchSpecification(criteria);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.Price >= 1500000m && l.Price <= 3000000m);
    }

    [Fact]
    public void ListingSearchSpecification_Should_Filter_By_MinBedrooms()
    {
        // Arrange
        var listings = CreateSearchTestListings();
        var criteria = new ListingSearchCriteria
        {
            MinBedrooms = 3
        };
        var spec = new ListingSearchSpecification(criteria);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.Bedrooms >= 3);
    }

    [Fact]
    public void ListingSearchSpecification_Should_Filter_By_Location()
    {
        // Arrange
        var listings = CreateSearchTestListings();
        var criteria = new ListingSearchCriteria
        {
            Location = "Guadalajara"
        };
        var spec = new ListingSearchSpecification(criteria);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.Location != null && l.Location.Contains("Guadalajara"));
    }

    [Fact]
    public void ListingSearchSpecification_Should_Filter_By_AreaRange()
    {
        // Arrange
        var listings = CreateSearchTestListings();
        var criteria = new ListingSearchCriteria
        {
            MinArea = 150m,
            MaxArea = 300m
        };
        var spec = new ListingSearchSpecification(criteria);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.LandArea >= 150m && l.LandArea <= 300m);
    }

    [Fact]
    public void ListingSearchSpecification_Should_Only_Return_Published()
    {
        // Arrange
        var listings = new List<Listing>
        {
            CreateListing("Casa Draft", Guid.NewGuid(), ListingStatus.Draft),
            CreateListing("Casa Published", Guid.NewGuid(), ListingStatus.Published),
            CreateListing("Casa Archived", Guid.NewGuid(), ListingStatus.Archived),
        };
        var criteria = new ListingSearchCriteria(); // Sin filtros adicionales
        var spec = new ListingSearchSpecification(criteria);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.Status == ListingStatus.Published);
        result.Count.ShouldBe(1);
    }

    [Fact]
    public void ListingSearchSpecification_Should_Apply_Multiple_Filters()
    {
        // Arrange
        var listings = CreateSearchTestListings();
        var criteria = new ListingSearchCriteria
        {
            TransactionType = TransactionType.Sale,
            Category = PropertyCategory.Residential,
            MinPrice = 1000000m,
            MaxPrice = 3000000m,
            MinBedrooms = 2
        };
        var spec = new ListingSearchSpecification(criteria);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l =>
            l.Status == ListingStatus.Published &&
            l.TransactionType == TransactionType.Sale &&
            l.Category == PropertyCategory.Residential &&
            l.Price >= 1000000m && l.Price <= 3000000m &&
            l.Bedrooms >= 2);
    }

    [Fact]
    public void ListingSearchSpecification_Should_Handle_Null_Location_In_Listings()
    {
        // Arrange
        var listings = new List<Listing>
        {
            CreateListing("Casa sin ubicación", Guid.NewGuid(), ListingStatus.Published, location: null),
            CreateListing("Casa en Guadalajara", Guid.NewGuid(), ListingStatus.Published, location: "Guadalajara"),
        };
        var criteria = new ListingSearchCriteria
        {
            Location = "Guadalajara"
        };
        var spec = new ListingSearchSpecification(criteria);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Location.ShouldBe("Guadalajara");
    }

    #endregion

    #region Combined Specifications Tests

    [Fact]
    public void Specifications_Should_Be_Combinable_With_And()
    {
        // Arrange
        var architectId = Guid.NewGuid();
        var listings = new List<Listing>
        {
            CreateListing("Casa 1", architectId, ListingStatus.Published),
            CreateListing("Casa 2", architectId, ListingStatus.Draft),
            CreateListing("Casa 3", Guid.NewGuid(), ListingStatus.Published),
        };

        var publishedSpec = new PublishedListingSpecification();
        var byArchitectSpec = new ListingByArchitectSpecification(architectId);

        // Act - Combinar specifications manualmente
        var result = listings.AsQueryable()
            .Where(publishedSpec.ToExpression())
            .Where(byArchitectSpec.ToExpression())
            .ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Title.ShouldBe("Casa 1");
        result[0].ArchitectId.ShouldBe(architectId);
        result[0].Status.ShouldBe(ListingStatus.Published);
    }

    [Fact]
    public void Specifications_Should_Work_With_Or_Logic()
    {
        // Arrange
        var listings = CreateTestListings();

        // Act - Usar PublicVisibleListingSpecification que ya implementa OR
        var spec = new PublicVisibleListingSpecification();
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l =>
            l.Status == ListingStatus.Published ||
            l.Status == ListingStatus.Portfolio);
        result.Count.ShouldBe(3);
    }

    #endregion

    #region Helper Methods

    private static List<Listing> CreateTestListings()
    {
        var architectId = Guid.NewGuid();
        return new List<Listing>
        {
            CreateListing("Casa Draft", architectId, ListingStatus.Draft),
            CreateListing("Casa Published 1", architectId, ListingStatus.Published),
            CreateListing("Casa Published 2", architectId, ListingStatus.Published),
            CreateListing("Casa Archived", architectId, ListingStatus.Archived),
            CreateListing("Casa Portfolio", architectId, ListingStatus.Portfolio),
        };
    }

    private static List<Listing> CreateSearchTestListings()
    {
        var architectId = Guid.NewGuid();
        return new List<Listing>
        {
            CreateListing("Casa Venta GDL", architectId, ListingStatus.Published,
                price: 2500000m, bedrooms: 3, bathrooms: 2,
                transactionType: TransactionType.Sale,
                category: PropertyCategory.Residential,
                type: PropertyType.House,
                location: "Guadalajara, Jalisco",
                landArea: 200m),

            CreateListing("Depto Renta CDMX", architectId, ListingStatus.Published,
                price: 15000m, bedrooms: 2, bathrooms: 1,
                transactionType: TransactionType.Rent,
                category: PropertyCategory.Residential,
                type: PropertyType.Apartment,
                location: "Ciudad de México",
                landArea: 80m),

            CreateListing("Oficina Venta GDL", architectId, ListingStatus.Published,
                price: 4000000m, bedrooms: 0, bathrooms: 2,
                transactionType: TransactionType.Sale,
                category: PropertyCategory.Commercial,
                type: PropertyType.Office,
                location: "Guadalajara, Jalisco",
                landArea: 150m),

            CreateListing("Casa Venta MTY", architectId, ListingStatus.Published,
                price: 1800000m, bedrooms: 4, bathrooms: 3,
                transactionType: TransactionType.Sale,
                category: PropertyCategory.Residential,
                type: PropertyType.House,
                location: "Monterrey, Nuevo León",
                landArea: 250m),

            CreateListing("Casa Draft", architectId, ListingStatus.Draft,
                price: 1000000m, bedrooms: 2, bathrooms: 1),
        };
    }

    private static Listing CreateListing(
        string title,
        Guid architectId,
        ListingStatus status,
        decimal price = 1000000m,
        int bedrooms = 2,
        int bathrooms = 1,
        TransactionType transactionType = TransactionType.Sale,
        PropertyCategory category = PropertyCategory.Residential,
        PropertyType type = PropertyType.House,
        string? location = "Ubicación por defecto",
        decimal landArea = 100m)
    {
        return new Listing
        {
            Title = title,
            Description = "Descripción de prueba",
            Location = location,
            Price = price,
            LandArea = landArea,
            ConstructionArea = landArea * 0.75m,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            Category = category,
            Type = type,
            TransactionType = transactionType,
            ArchitectId = architectId,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
