using System;
using System.Linq.Expressions;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Volo.Abp.Specifications;

namespace cima.Domain.Specifications.Listings;

/// <summary>
/// Specification compuesta para búsqueda avanzada de Listings.
/// Soporta múltiples filtros opcionales.
/// </summary>
public class ListingSearchSpecification : Specification<Listing>
{
    private readonly ListingSearchCriteria _criteria;

    public ListingSearchSpecification(ListingSearchCriteria criteria)
    {
        _criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));
    }

    public override Expression<Func<Listing, bool>> ToExpression()
    {
        return listing =>
            // Solo publicadas (siempre aplicado para búsqueda pública)
            listing.Status == ListingStatus.Published &&

            // Tipo de transacción
            (!_criteria.TransactionType.HasValue ||
             listing.TransactionType == _criteria.TransactionType.Value) &&

            // Categoría
            (!_criteria.Category.HasValue ||
             listing.Category == _criteria.Category.Value) &&

            // Tipo de propiedad
            (!_criteria.PropertyType.HasValue ||
             listing.Type == _criteria.PropertyType.Value) &&

            // Ubicación (contiene texto)
            (string.IsNullOrEmpty(_criteria.Location) ||
             (listing.Location != null && listing.Location.Contains(_criteria.Location))) &&

            // Rango de precio
            (!_criteria.MinPrice.HasValue || listing.Price >= _criteria.MinPrice.Value) &&
            (!_criteria.MaxPrice.HasValue || listing.Price <= _criteria.MaxPrice.Value) &&

            // Habitaciones y baños mínimos
            (!_criteria.MinBedrooms.HasValue || listing.Bedrooms >= _criteria.MinBedrooms.Value) &&
            (!_criteria.MinBathrooms.HasValue || listing.Bathrooms >= _criteria.MinBathrooms.Value) &&

            // Rango de área
            (!_criteria.MinArea.HasValue || listing.LandArea >= _criteria.MinArea.Value) &&
            (!_criteria.MaxArea.HasValue || listing.LandArea <= _criteria.MaxArea.Value) &&

            // Por arquitecto
            (!_criteria.ArchitectId.HasValue || listing.ArchitectId == _criteria.ArchitectId.Value);
    }
}

/// <summary>
/// Criterios de búsqueda para ListingSearchSpecification.
/// </summary>
public class ListingSearchCriteria
{
    public TransactionType? TransactionType { get; set; }
    public PropertyCategory? Category { get; set; }
    public PropertyType? PropertyType { get; set; }
    public string? Location { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public Guid? ArchitectId { get; set; }
}
