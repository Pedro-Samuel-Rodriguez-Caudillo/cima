using System;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Entities.Listings;
using cima.Domain.Shared;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace cima.Domain.Services.Listings;

public class ListingManager : DomainService, IListingManager
{
    public ListingManager()
    {
    }

    public Task<Listing> CreateAsync(
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
        Guid? createdBy)
    {
        // Reglas de negocio de creación (si las hubiera, ej: verificar unicidad de título)

        // Usar Address VO
        var address = !string.IsNullOrWhiteSpace(location) ? new Address(location) : null;

        var listing = new Listing(
            GuidGenerator.Create(),
            title,
            description,
            address,
            price,
            landArea,
            constructionArea,
            bedrooms,
            bathrooms,
            category,
            type,
            transactionType,
            architectId,
            createdBy
        );

        return Task.FromResult(listing);
    }

    public Task<Listing> UpdateAsync(
        Listing listing,
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
        Guid modifiedBy)
    {
        // Reglas de negocio de actualización
        // (Por ejemplo: validar cambios en estado Published, etc. - delegado a entidad por ahora)

        // Usar Address VO
        var address = !string.IsNullOrWhiteSpace(location) ? new Address(location) : null;

        listing.UpdateInfo(
            title,
            description,
            address,
            price,
            landArea,
            constructionArea,
            bedrooms,
            bathrooms,
            category,
            type,
            transactionType,
            modifiedBy
        );

        return Task.FromResult(listing);
    }


    public Task PublishAsync(Listing listing, Guid publishedBy)
    {
        listing.Publish(publishedBy);
        return Task.CompletedTask;
    }

    public Task UnpublishAsync(Listing listing, Guid unpublishedBy)
    {
        listing.Unpublish(unpublishedBy);
        return Task.CompletedTask;
    }

    public Task ArchiveAsync(Listing listing, Guid archivedBy)
    {
        listing.Archive(archivedBy);
        return Task.CompletedTask;
    }

    public Task UnarchiveAsync(Listing listing, Guid unarchivedBy)
    {
        listing.Unarchive(unarchivedBy);
        return Task.CompletedTask;
    }

    public Task MoveToPortfolioAsync(Listing listing, Guid movedBy)
    {
        listing.MoveToPortfolio(movedBy);
        return Task.CompletedTask;
    }

    public void ValidateListingData(
        string title,
        string description,
        decimal price,
        decimal landArea,
        decimal constructionArea)
    {
        // Esta validación ahora es redundante porque la Entidad se valida a sí misma,
        // pero podemos mantenerla si queremos validaciones previas sin instanciar.
        // Por ahora, delegamos.
    }
}
