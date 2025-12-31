using System;
using cima.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace cima.Domain.Entities.Listings;

public class ListingSale : FullAuditedEntity<Guid>
{
    public Guid ListingId { get; private set; }
    public Guid ArchitectId { get; private set; }
    public DateTime SoldAt { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "MXN";
    public string? Notes { get; private set; }

    public Listing? Listing { get; private set; }
    public Architect? Architect { get; private set; }

    private ListingSale()
    {
    }

    public ListingSale(Guid listingId, Guid architectId, DateTime soldAt, decimal amount, string currency, string? notes)
    {
        ListingId = listingId;
        ArchitectId = architectId;
        SoldAt = soldAt;
        Amount = amount;
        Currency = currency;
        Notes = notes;
    }
}
