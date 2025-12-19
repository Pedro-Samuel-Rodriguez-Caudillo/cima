using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Xunit;
using cima.Domain.Entities;
using cima.Domain.Shared;
using cima.Domain.Specifications.Listings;
using cima.Domain.Entities.Listings;

namespace cima.Specifications.Listings;

/// <summary>
/// Tests para las especificaciones de búsqueda de Listings
/// </summary>
public sealed class ListingSpecificationsTests : cimaDomainTestBase<cimaDomainTestModule>
{
    #region PublishedListingSpecification Tests

    [Fact]
    public void PublishedListingSpecification_Should_Filter_Only_Published()
    {
        // Arrange
        var listings = new List<Listing>
        {
            CreateListing("Casa Draft", Guid.NewGuid(), ListingStatus.Draft),
            CreateListing("Casa Published", Guid.NewGuid(), ListingStatus.Published),
            CreateListing("Casa Archived", Guid.NewGuid(), ListingStatus.Archived),
        };
        var spec = new PublishedListingSpecification();

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Status.ShouldBe(ListingStatus.Published);
    }

    #endregion

    #region ListingByArchitectSpecification Tests

    [Fact]
    public void ListingByArchitectSpecification_Should_Filter_By_Architect()
    {
        // Arrange
        var targetArchitectId = Guid.NewGuid();
        var otherArchitectId = Guid.NewGuid();
        var listings = new List<Listing>
        {
            CreateListing("Casa Arq 1", targetArchitectId, ListingStatus.Published),
            CreateListing("Casa Arq 1 - Draft", targetArchitectId, ListingStatus.Draft),
            CreateListing("Casa Arq 2", otherArchitectId, ListingStatus.Published),
        };
        var spec = new ListingByArchitectSpecification(targetArchitectId);

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldAllBe(l => l.ArchitectId == targetArchitectId);
    }

    #endregion

    #region ListingSearchSpecification Tests

    [Fact]
    public void ListingSearchSpecification_Should_Filter_By_PriceRange()
    {
        // Arrange
        var listings = CreateSearchTestListings();
        var spec = new ListingSearchSpecification(
            minPrice: 1000000m,
            maxPrice: 3000000m
        );

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.Status == ListingStatus.Published);
        result.ShouldAllBe(l => l.Price >= 1000000m && l.Price <= 3000000m);
    }

    [Fact]
    public void ListingSearchSpecification_Should_Filter_By_AreaRange()
    {
        // Arrange
        var listings = CreateSearchTestListings();
        var spec = new ListingSearchSpecification(
            minArea: 150m,
            maxArea: 300m
        );

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.ShouldAllBe(l => l.Status == ListingStatus.Published);
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
        var spec = new ListingSearchSpecification();

        // Act
        var result = listings.AsQueryable().Where(spec.ToExpression()).ToList();

        // Assert
        result.Count.ShouldBe(1);
        result[0].Status.ShouldBe(ListingStatus.Published);
    }

    [Fact]
    public void ListingSearchSpecification_Should_Apply_Multiple_Filters()
    {
        // Arrange
        var listings = CreateSearchTestListings();
        var spec = new ListingSearchSpecification(
            transactionType: TransactionType.Sale,
            propertyCategory: PropertyCategory.Residential,
            minPrice: 1000000m,
            maxPrice: 3000000m,
            minBedrooms: 2
        );

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

    #endregion

    #region Helper Methods

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
        var listing = new Listing(
            Guid.NewGuid(),
            title,
            "Descripción de prueba",
            !string.IsNullOrWhiteSpace(location) ? new Address(location) : null,
            price,
            landArea,
            landArea * 0.75m,
            bedrooms,
            bathrooms,
            category,
            type,
            transactionType,
            architectId,
            Guid.NewGuid()
        );

        if (status == ListingStatus.Published)
        {
            listing.AddImage(Guid.NewGuid(), "url", "thumb", "alt", 1024, "image/jpeg");
            listing.Publish(Guid.NewGuid());
        }
        else if (status == ListingStatus.Archived)
        {
            listing.Archive(Guid.NewGuid());
        }
        else if (status == ListingStatus.Portfolio)
        {
            listing.MoveToPortfolio(Guid.NewGuid());
        }

        return listing;
    }

    #endregion
}
