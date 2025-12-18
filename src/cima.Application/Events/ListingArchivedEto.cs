using System;
using Volo.Abp.EventBus;

namespace cima.Events;

/// <summary>
/// Event Transfer Object para cuando una propiedad es archivada.
/// </summary>
[EventName("cima.listing.archived")]
public class ListingArchivedEto
{
    public Guid ListingId { get; set; }
    public Guid ArchitectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ArchivedAt { get; set; }
}
