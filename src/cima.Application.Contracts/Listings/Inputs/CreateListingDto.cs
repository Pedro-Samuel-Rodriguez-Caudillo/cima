using System;
using System.ComponentModel.DataAnnotations;
using cima.Domain.Shared;

namespace cima.Listings.Inputs;

public class CreateListingDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    public AddressDto? Address { get; set; }

    /// <summary>
    /// Precio de la propiedad. Use -1 para indicar "precio a consultar".
    /// </summary>
    [Range(-1, double.MaxValue)]
    public decimal Price { get; set; }

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
    public PropertyCategory Category { get; set; }

    [Required]
    public PropertyType Type { get; set; }

    [Required]
    public TransactionType TransactionType { get; set; }

    [Required]
    public Guid ArchitectId { get; set; }
}
