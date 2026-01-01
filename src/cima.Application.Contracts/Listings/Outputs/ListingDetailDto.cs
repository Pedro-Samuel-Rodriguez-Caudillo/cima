using System;
using System.Collections.Generic;
using cima.Architects;
using cima.Domain.Shared;

namespace cima.Listings.Outputs;

public class ListingDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LocationDto? Location { get; set; }
    public bool IsPriceOnRequest { get; set; }
    public decimal? Price { get; set; }
    public decimal LandArea { get; set; }
    public decimal ConstructionArea { get; set; }
    
    // Compatibility property for existing UI
    public decimal Area { get; set; }

    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public ListingStatus Status { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid TypeId { get; set; }
    public string? TypeName { get; set; }
    public TransactionType TransactionType { get; set; }
    
    public Guid ArchitectId { get; set; }
    public ArchitectDto? Architect { get; set; }
    
    public List<ListingImageDto> Images { get; set; } = new();
    
    public DateTime? FirstPublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Admin specific fields can be subclasses or separate DTOs. 
    // For "ListingDetailDetailedOutput", this covers the main detail. 
}
