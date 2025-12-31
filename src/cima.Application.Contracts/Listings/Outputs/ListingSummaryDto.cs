using System;
using cima.Domain.Shared;
using cima.Listings.Inputs; // For AddressInput logic sharing if needed, but Outputs usually have their own AddressDto. Assuming re-using shared DTOs for now or creating Output specific. 
// User chose context specific, so let's stick to Dtos.ListingDto style but renamed.
// Actually, Address is a simple value object. Let's output it as the LocationDto we have or a new AddressOutput. 
// Existing LocationDto is fine.

namespace cima.Listings.Outputs;

public class ListingSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public LocationDto? Location { get; set; }
    public bool IsPriceOnRequest { get; set; }
    public decimal? Price { get; set; }
    public ListingStatus Status { get; set; }
    public PropertyCategory Category { get; set; }
    public PropertyType Type { get; set; }
    public TransactionType TransactionType { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public decimal ConstructionArea { get; set; }
    public int ImageCount { get; set; }
    public ListingImageDto? MainImage { get; set; }
    public DateTime CreatedAt { get; set; }
}
