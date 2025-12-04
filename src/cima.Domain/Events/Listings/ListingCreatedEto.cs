using System;

namespace cima.Domain.Events.Listings;

/// <summary>
/// Evento de dominio que se dispara cuando se crea un nuevo Listing.
/// </summary>
public class ListingCreatedEto
{
    public Guid ListingId { get; }
    public Guid ArchitectId { get; }
    public string Title { get; }
    public DateTime CreatedAt { get; }

    public ListingCreatedEto(
        Guid listingId,
        Guid architectId,
        string title,
        DateTime createdAt)
    {
        ListingId = listingId;
        ArchitectId = architectId;
        Title = title;
        CreatedAt = createdAt;
    }
}
