using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace cima.Listings;

public class ListingSaleDto : EntityDto<Guid>
{
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public Guid ArchitectId { get; set; }
    public string ArchitectName { get; set; } = string.Empty;
    public DateTime SoldAt { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "MXN";
    public string? Notes { get; set; }
}

public class CreateListingSaleDto
{
    [Required]
    public Guid ListingId { get; set; }

    [Required]
    public DateTime SoldAt { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "MXN";

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class ArchitectSalesSummaryDto
{
    public Guid ArchitectId { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? LastSaleAt { get; set; }
}
