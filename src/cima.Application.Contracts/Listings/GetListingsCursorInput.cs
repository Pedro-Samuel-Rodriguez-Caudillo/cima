using System;
using System.ComponentModel.DataAnnotations;
using cima.Common;
using cima.Domain.Shared;

namespace cima.Listings;

/// <summary>
/// Input para obtener listings con paginacion basada en cursor
/// </summary>
public class GetListingsCursorInput : CursorPagedRequestDto
{
    [MaxLength(200)]
    public string? SearchTerm { get; set; }

    public ListingStatus? Status { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; set; }

    [Range(0, 50)]
    public int? MinBedrooms { get; set; }

    [Range(0, 50)]
    public int? MinBathrooms { get; set; }

    public Guid? ArchitectId { get; set; }

    public Guid? TypeId { get; set; }

    public Guid? CategoryId { get; set; }

    public TransactionType? TransactionType { get; set; }

    [MaxLength(50)]
    public string? SortBy { get; set; } = "CreatedAt";

    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// DTO para listing en resultados de cursor pagination
/// </summary>
public class ListingCursorDto
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Location { get; set; }
    public decimal Price { get; set; }
    public decimal LandArea { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public ListingStatus Status { get; set; }
    public Guid TypeId { get; set; }
    public string? TypeName { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public TransactionType TransactionType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? MainImageUrl { get; set; }
    public string? ArchitectName { get; set; }
}
