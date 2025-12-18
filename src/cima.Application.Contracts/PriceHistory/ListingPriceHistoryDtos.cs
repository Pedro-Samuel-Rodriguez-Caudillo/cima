using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace cima.PriceHistory;

public class ListingPriceHistoryDto
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }
    
    // Metadatos anti-fraude
    public Guid? ChangedByUserId { get; set; }
    public string? ChangedByUserName { get; set; }
    public string? ClientIpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public string? ChangeReason { get; set; }
    public string? SessionId { get; set; }
    public string? AuthenticationMethod { get; set; }
    
    /// <summary>
    /// Diferencia de precio (NewPrice - OldPrice)
    /// </summary>
    public decimal PriceDifference => NewPrice - OldPrice;
    
    /// <summary>
    /// Porcentaje de cambio
    /// </summary>
    public decimal PercentageChange => OldPrice != 0 ? ((NewPrice - OldPrice) / OldPrice) * 100 : 0;
}

public class GetPriceHistoryInput : PagedAndSortedResultRequestDto
{
    public Guid? ListingId { get; set; }
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
