using System;
using System.Linq;
using System.Linq.Expressions;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Volo.Abp.Specifications;

namespace cima.Domain.Specifications.Listings;

public class ListingSearchSpecification : Specification<Listing>
{
    private readonly string? _searchTerm;
    private readonly decimal? _minPrice;
    private readonly decimal? _maxPrice;
    private readonly decimal? _minArea;
    private readonly decimal? _maxArea;
    private readonly int? _minBedrooms;
    private readonly int? _minBathrooms;
    private readonly Guid? _propertyTypeId;
    private readonly Guid? _propertyCategoryId;
    private readonly TransactionType? _transactionType;
    private readonly string? _location;

    public ListingSearchSpecification(
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        decimal? minArea = null,
        decimal? maxArea = null,
        int? minBedrooms = null,
        int? minBathrooms = null,
        Guid? propertyTypeId = null,
        Guid? propertyCategoryId = null,
        TransactionType? transactionType = null,
        string? location = null)
    {
        _searchTerm = searchTerm;
        _minPrice = minPrice;
        _maxPrice = maxPrice;
        _minArea = minArea;
        _maxArea = maxArea;
        _minBedrooms = minBedrooms;
        _minBathrooms = minBathrooms;
        _propertyTypeId = propertyTypeId;
        _propertyCategoryId = propertyCategoryId;
        _transactionType = transactionType;
        _location = location;
    }

    public override Expression<Func<Listing, bool>> ToExpression()
    {
        return listing =>
            listing.Status == ListingStatus.Published &&
            (string.IsNullOrWhiteSpace(_searchTerm) || 
             listing.Title.Contains(_searchTerm) || 
             (listing.Location != null && listing.Location.Value.Contains(_searchTerm))) &&
            (!_minPrice.HasValue || listing.Price >= _minPrice.Value) &&
            (!_maxPrice.HasValue || listing.Price <= _maxPrice.Value) &&
            (!_minArea.HasValue || listing.LandArea >= _minArea.Value) &&
            (!_maxArea.HasValue || listing.LandArea <= _maxArea.Value) &&
            (!_minBedrooms.HasValue || listing.Bedrooms >= _minBedrooms.Value) &&
            (!_minBathrooms.HasValue || listing.Bathrooms >= _minBathrooms.Value) &&
            (!_propertyTypeId.HasValue || listing.TypeId == _propertyTypeId.Value) &&
            (!_propertyCategoryId.HasValue || listing.CategoryId == _propertyCategoryId.Value) &&
            (!_transactionType.HasValue || listing.TransactionType == _transactionType.Value) &&
            (string.IsNullOrWhiteSpace(_location) || (listing.Location != null && listing.Location.Value.Contains(_location)));
    }
}
