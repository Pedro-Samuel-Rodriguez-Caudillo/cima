using System;
using cima.Domain.Shared;

namespace cima.Domain.Events.Listings;

/// <summary>
/// Evento de dominio que se dispara cuando cambia el estado de un Listing.
/// Utilizado para actualizar estadísticas del arquitecto, invalidar caché, etc.
/// </summary>
public class ListingStatusChangedEto
{
    public Guid ListingId { get; }
    public Guid ArchitectId { get; }
    public ListingStatus OldStatus { get; }
    public ListingStatus NewStatus { get; }
    public DateTime ChangedAt { get; }
    
    /// <summary>
    /// Indica si esta es la primera vez que el listing se publica.
    /// Se usa para evitar incrementar TotalListingsPublished en republicaciones.
    /// </summary>
    public bool IsFirstTimePublished { get; }

    public ListingStatusChangedEto(
        Guid listingId,
        Guid architectId,
        ListingStatus oldStatus,
        ListingStatus newStatus,
        bool isFirstTimePublished = false)
    {
        ListingId = listingId;
        ArchitectId = architectId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = DateTime.UtcNow;
        IsFirstTimePublished = isFirstTimePublished;
    }

    /// <summary>
    /// Indica si el listing se hizo visible públicamente.
    /// </summary>
    public bool BecamePubliclyVisible =>
        OldStatus == ListingStatus.Draft && 
        (NewStatus == ListingStatus.Published || NewStatus == ListingStatus.Portfolio);

    /// <summary>
    /// Indica si el listing dejó de ser visible públicamente.
    /// </summary>
    public bool BecameHidden =>
        (OldStatus == ListingStatus.Published || OldStatus == ListingStatus.Portfolio) &&
        (NewStatus == ListingStatus.Draft || NewStatus == ListingStatus.Archived);

    /// <summary>
    /// Indica si el listing se publicó (específicamente a Published).
    /// Solo incrementa TotalListingsPublished si es primera publicación.
    /// </summary>
    public bool WasPublished =>
        OldStatus == ListingStatus.Draft && NewStatus == ListingStatus.Published;

    /// <summary>
    /// Indica si el listing se archivó.
    /// </summary>
    public bool WasArchived => NewStatus == ListingStatus.Archived;

    /// <summary>
    /// Indica si el listing se movió al portafolio.
    /// </summary>
    public bool WasMovedToPortfolio => NewStatus == ListingStatus.Portfolio;
}
