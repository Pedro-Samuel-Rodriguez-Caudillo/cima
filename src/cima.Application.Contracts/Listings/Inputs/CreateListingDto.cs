using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using cima.Domain.Shared;

namespace cima.Listings.Inputs;

public class CreateListingDto : IValidatableObject
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    public AddressDto? Address { get; set; }

    public bool IsPriceOnRequest { get; set; }

    /// <summary>
    /// Precio de la propiedad. Dejar null cuando es "precio a consultar".
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal LandArea { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ConstructionArea { get; set; }

    [Range(0, 100)]
    public int Bedrooms { get; set; }

    [Range(0, 100)]
    public int Bathrooms { get; set; }

    [Required]
    public ListingStatus Status { get; set; } = ListingStatus.Draft;

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid TypeId { get; set; }

    [Required]
    public TransactionType TransactionType { get; set; }

    [Required]
    public Guid ArchitectId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsPriceOnRequest && Price.HasValue)
        {
            yield return new ValidationResult(
                "Price debe ser null cuando IsPriceOnRequest es true.",
                new[] { nameof(Price) });
        }

        if (!IsPriceOnRequest && !Price.HasValue)
        {
            yield return new ValidationResult(
                "Price es requerido cuando IsPriceOnRequest es false.",
                new[] { nameof(Price) });
        }
    }
}
