using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using cima.Architects;
using cima.Domain.Shared;

namespace cima.Listings;

public class LocationDto
{
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    
    public override string ToString() => Address ?? $"{City}, {State}";
}

public class LocationSuggestionDto
{
    public string Location { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ListingDto : AuditedEntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LocationDto? Location { get; set; }
    public decimal Price { get; set; }
    public decimal LandArea { get; set; }
    public decimal ConstructionArea { get; set; }
    
    // Compatibility property
    public decimal Area { get; set; }

    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public ListingStatus Status { get; set; }
    public PropertyCategory Category { get; set; }
    public PropertyType Type { get; set; }
    public TransactionType TransactionType { get; set; }
    public Guid ArchitectId { get; set; }
    public ArchitectDto? Architect { get; set; }
    public List<ListingImageDto> Images { get; set; } = new();
    
    // Compatibility property
    public ListingImageDto? CoverImage { get; set; }
    
    public DateTime? FirstPublishedAt { get; set; }
}

public class ListingListDto : EntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    public LocationDto? Location { get; set; }
    public decimal Price { get; set; }
    public ListingStatus Status { get; set; }
    public PropertyCategory Category { get; set; }
    public TransactionType TransactionType { get; set; }
    public ListingImageDto? MainImage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUpdateListingDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; } 
    public decimal Price { get; set; }
    public decimal LandArea { get; set; }
    public decimal ConstructionArea { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public ListingStatus Status { get; set; }
    public PropertyCategory Category { get; set; }
    public PropertyType Type { get; set; }
    public TransactionType TransactionType { get; set; }
    public Guid ArchitectId { get; set; }
}

public class ListingImageDto : EntityDto<Guid>
{
    public Guid ImageId { get; set; } // Compatibility property
    public string Url { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsCover { get; set; }
}

public class CreateListingImageDto
{
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}

public class UpdateImageOrderDto
{
    public Guid ImageId { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateFeaturedListingDto
{
    [Required]
    public Guid ListingId { get; set; }
}

public class FeaturedListingDto : EntityDto<Guid>
{
    public Guid ListingId { get; set; }
    public ListingDto? Listing { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreationTime { get; set; }
}

public class GetFeaturedListingsDto : PagedAndSortedResultRequestDto
{
    public int PageNumber { get; set; }
    [Range(1, 12)]
    public int PageSize { get; set; } = 6;
}

public class PropertySearchDto : PagedAndSortedResultRequestDto
{
    public TransactionType? TransactionType { get; set; }
    public PropertyCategory? Category { get; set; }
    public PropertyType? Type { get; set; }
    public string? Location { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    
    public decimal? MinArea { get; set; }
    public decimal? MaxArea { get; set; }
    public string? SortBy { get; set; }
    
    public int PageNumber { get; set; } // Service uses this explicitly
    public int PageSize { get; set; } = 10;
}
