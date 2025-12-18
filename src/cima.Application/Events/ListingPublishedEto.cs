using System;
using Volo.Abp.EventBus;

namespace cima.Events;

/// <summary>
/// Event Transfer Object para cuando una propiedad es publicada.
/// Se usa para desacoplar lógica como actualización de estadísticas,
/// envío de notificaciones, etc.
/// </summary>
[EventName("cima.listing.published")]
public class ListingPublishedEto
{
    /// <summary>
    /// ID del listing publicado
    /// </summary>
    public Guid ListingId { get; set; }

    /// <summary>
    /// ID del arquitecto propietario
    /// </summary>
    public Guid ArchitectId { get; set; }

    /// <summary>
    /// Título del listing
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora de publicación
    /// </summary>
    public DateTime PublishedAt { get; set; }
}
