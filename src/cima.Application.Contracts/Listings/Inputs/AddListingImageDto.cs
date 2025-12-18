using System;
using System.ComponentModel.DataAnnotations;

namespace cima.Listings.Inputs;

public class AddListingImageDto
{
    [Required]
    public Guid ListingId { get; set; }

    [Required]
    [MaxLength(10000000)] // ~10MB base64
    public string Url { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AltText { get; set; }

    [Range(0, long.MaxValue)]
    public long FileSize { get; set; }

    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;
}
