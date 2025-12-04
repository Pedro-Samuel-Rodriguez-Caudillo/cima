using System;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Events.Listings;
using cima.Domain.Shared;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace cima.Domain.Services.Listings;

/// <summary>
/// Domain Service que encapsula la lógica de negocio de Listings.
/// </summary>
public class ListingManager : DomainService, IListingManager
{
    private readonly IRepository<Architect, Guid> _architectRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public ListingManager(
        IRepository<Architect, Guid> architectRepository,
        IGuidGenerator guidGenerator,
        IClock clock)
    {
        _architectRepository = architectRepository;
        _guidGenerator = guidGenerator;
        _clock = clock;
    }

    public async Task<Listing> CreateAsync(
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
        Guid? createdBy = null)
    {
        // Validar datos del listing
        ValidateListingData(title, description, price, landArea, constructionArea);

        // Validar que el arquitecto existe y está activo
        var architect = await _architectRepository.GetAsync(architectId);
        if (!architect.IsActive)
        {
            throw new BusinessException(cimaDomainErrorCodes.ArchitectInactive)
                .WithData("ArchitectId", architectId);
        }

        var listing = new Listing
        {
            Title = title.Trim(),
            Description = description.Trim(),
            Location = location?.Trim(),
            Price = price,
            LandArea = landArea,
            ConstructionArea = constructionArea,
            Bedrooms = bedrooms,
            Bathrooms = bathrooms,
            Category = category,
            Type = type,
            TransactionType = transactionType,
            ArchitectId = architectId,
            Status = ListingStatus.Draft,
            CreatedAt = _clock.Now,
            CreatedBy = createdBy,
            FirstPublishedAt = null // Nunca publicado
        };

        // Disparar evento de dominio
        listing.AddDomainEvent(new ListingCreatedEto(
            listing.Id,
            architectId,
            listing.Title,
            listing.CreatedAt));

        return listing;
    }

    public async Task PublishAsync(Listing listing, Guid? modifiedBy = null)
    {
        if (!CanChangeStatus(listing.Status, ListingStatus.Published))
        {
            throw new BusinessException(cimaDomainErrorCodes.InvalidStatusTransition)
                .WithData("CurrentStatus", listing.Status)
                .WithData("NewStatus", ListingStatus.Published);
        }

        var oldStatus = listing.Status;
        var isFirstTimePublished = listing.FirstPublishedAt == null;
        
        listing.Status = ListingStatus.Published;
        listing.LastModifiedAt = _clock.Now;
        listing.LastModifiedBy = modifiedBy;
        
        // Marcar fecha de primera publicación solo si nunca se publicó
        if (isFirstTimePublished)
        {
            listing.FirstPublishedAt = _clock.Now;
        }

        // Disparar evento con flag de primera publicación
        listing.AddDomainEvent(new ListingStatusChangedEto(
            listing.Id,
            listing.ArchitectId,
            oldStatus,
            ListingStatus.Published,
            isFirstTimePublished));

        await Task.CompletedTask;
    }

    public async Task UnpublishAsync(Listing listing, Guid? modifiedBy = null)
    {
        if (!CanChangeStatus(listing.Status, ListingStatus.Draft))
        {
            throw new BusinessException(cimaDomainErrorCodes.InvalidStatusTransition)
                .WithData("CurrentStatus", listing.Status)
                .WithData("NewStatus", ListingStatus.Draft);
        }

        var oldStatus = listing.Status;
        listing.Status = ListingStatus.Draft;
        listing.LastModifiedAt = _clock.Now;
        listing.LastModifiedBy = modifiedBy;

        listing.AddDomainEvent(new ListingStatusChangedEto(
            listing.Id,
            listing.ArchitectId,
            oldStatus,
            ListingStatus.Draft));

        await Task.CompletedTask;
    }

    public async Task ArchiveAsync(Listing listing, Guid? modifiedBy = null)
    {
        if (!CanChangeStatus(listing.Status, ListingStatus.Archived))
        {
            throw new BusinessException(cimaDomainErrorCodes.InvalidStatusTransition)
                .WithData("CurrentStatus", listing.Status)
                .WithData("NewStatus", ListingStatus.Archived);
        }

        var oldStatus = listing.Status;
        listing.Status = ListingStatus.Archived;
        listing.LastModifiedAt = _clock.Now;
        listing.LastModifiedBy = modifiedBy;

        listing.AddDomainEvent(new ListingStatusChangedEto(
            listing.Id,
            listing.ArchitectId,
            oldStatus,
            ListingStatus.Archived));

        await Task.CompletedTask;
    }

    public async Task UnarchiveAsync(Listing listing, Guid? modifiedBy = null)
    {
        if (listing.Status != ListingStatus.Archived)
        {
            throw new BusinessException(cimaDomainErrorCodes.ListingNotArchived)
                .WithData("ListingId", listing.Id)
                .WithData("CurrentStatus", listing.Status);
        }

        var oldStatus = listing.Status;
        listing.Status = ListingStatus.Published;
        listing.LastModifiedAt = _clock.Now;
        listing.LastModifiedBy = modifiedBy;

        // No es primera publicación si ya tiene FirstPublishedAt
        listing.AddDomainEvent(new ListingStatusChangedEto(
            listing.Id,
            listing.ArchitectId,
            oldStatus,
            ListingStatus.Published,
            isFirstTimePublished: false));

        await Task.CompletedTask;
    }

    public async Task MoveToPortfolioAsync(Listing listing, Guid? modifiedBy = null)
    {
        if (!CanChangeStatus(listing.Status, ListingStatus.Portfolio))
        {
            throw new BusinessException(cimaDomainErrorCodes.InvalidStatusTransition)
                .WithData("CurrentStatus", listing.Status)
                .WithData("NewStatus", ListingStatus.Portfolio);
        }

        var oldStatus = listing.Status;
        listing.Status = ListingStatus.Portfolio;
        listing.LastModifiedAt = _clock.Now;
        listing.LastModifiedBy = modifiedBy;

        listing.AddDomainEvent(new ListingStatusChangedEto(
            listing.Id,
            listing.ArchitectId,
            oldStatus,
            ListingStatus.Portfolio));

        await Task.CompletedTask;
    }

    public bool CanChangeStatus(ListingStatus currentStatus, ListingStatus newStatus)
    {
        // Máquina de estados para transiciones válidas
        return (currentStatus, newStatus) switch
        {
            // Desde Draft
            (ListingStatus.Draft, ListingStatus.Published) => true,
            
            // Desde Published
            (ListingStatus.Published, ListingStatus.Draft) => true,
            (ListingStatus.Published, ListingStatus.Archived) => true,
            (ListingStatus.Published, ListingStatus.Portfolio) => true,
            
            // Desde Archived
            (ListingStatus.Archived, ListingStatus.Published) => true,
            
            // Desde Portfolio
            (ListingStatus.Portfolio, ListingStatus.Archived) => true,
            (ListingStatus.Portfolio, ListingStatus.Published) => true,
            
            // Cualquier otra transición no es válida
            _ => false
        };
    }

    public void ValidateListingData(
        string title,
        string description,
        decimal price,
        decimal landArea,
        decimal constructionArea)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BusinessException(cimaDomainErrorCodes.ListingTitleRequired);
        }

        if (title.Length > 200)
        {
            throw new BusinessException(cimaDomainErrorCodes.ListingTitleTooLong)
                .WithData("MaxLength", 200);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new BusinessException(cimaDomainErrorCodes.ListingDescriptionRequired);
        }

        if (price <= 0)
        {
            throw new BusinessException(cimaDomainErrorCodes.ListingInvalidPrice)
                .WithData("Price", price);
        }

        if (landArea <= 0)
        {
            throw new BusinessException(cimaDomainErrorCodes.ListingInvalidLandArea)
                .WithData("LandArea", landArea);
        }

        if (constructionArea <= 0)
        {
            throw new BusinessException(cimaDomainErrorCodes.ListingInvalidConstructionArea)
                .WithData("ConstructionArea", constructionArea);
        }

        if (constructionArea > landArea)
        {
            throw new BusinessException(cimaDomainErrorCodes.ConstructionAreaExceedsLandArea)
                .WithData("ConstructionArea", constructionArea)
                .WithData("LandArea", landArea);
        }
    }
}
