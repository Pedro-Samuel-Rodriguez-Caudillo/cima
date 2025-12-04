using System;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Shared;

namespace cima.Domain.Services.Listings;

/// <summary>
/// Domain Service para gestionar la lógica de negocio compleja de Listings.
/// Encapsula validaciones, transiciones de estado y coordinación entre agregados.
/// </summary>
public interface IListingManager
{
    /// <summary>
    /// Crea un nuevo Listing validando todas las reglas de negocio.
    /// </summary>
    Task<Listing> CreateAsync(
        string title,
        string description,
        string? location,
        decimal price,
        decimal landArea,
        decimal constructionArea,
        int bedrooms,
        int bathrooms,
        PropertyCategory category,
        PropertyType type,
        TransactionType transactionType,
        Guid architectId,
        Guid? createdBy = null);

    /// <summary>
    /// Publica un listing (Draft ? Published).
    /// Dispara evento ListingPublishedEto.
    /// </summary>
    Task PublishAsync(Listing listing, Guid? modifiedBy = null);

    /// <summary>
    /// Despublica un listing (Published ? Draft).
    /// </summary>
    Task UnpublishAsync(Listing listing, Guid? modifiedBy = null);

    /// <summary>
    /// Archiva un listing (Published/Portfolio ? Archived).
    /// Dispara evento ListingArchivedEto.
    /// </summary>
    Task ArchiveAsync(Listing listing, Guid? modifiedBy = null);

    /// <summary>
    /// Desarchiva un listing (Archived ? Published).
    /// </summary>
    Task UnarchiveAsync(Listing listing, Guid? modifiedBy = null);

    /// <summary>
    /// Mueve un listing al portafolio (Published ? Portfolio).
    /// Dispara evento ListingMovedToPortfolioEto.
    /// </summary>
    Task MoveToPortfolioAsync(Listing listing, Guid? modifiedBy = null);

    /// <summary>
    /// Verifica si una transición de estado es válida.
    /// </summary>
    bool CanChangeStatus(ListingStatus currentStatus, ListingStatus newStatus);

    /// <summary>
    /// Valida que los datos del listing sean correctos.
    /// </summary>
    void ValidateListingData(
        string title,
        string description,
        decimal price,
        decimal landArea,
        decimal constructionArea);
}
