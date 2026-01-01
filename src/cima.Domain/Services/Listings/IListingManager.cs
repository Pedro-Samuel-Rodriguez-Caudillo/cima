using System;
using System.Threading.Tasks;
using cima.Domain.Entities.Listings;
using cima.Domain.Entities;
using cima.Domain.Shared;

namespace cima.Domain.Services.Listings;

public interface IListingManager
{
    Task<Listing> CreateAsync(
        string title,
        string description,
        string? location,
        decimal price,
        decimal landArea,
        decimal constructionArea,
        int bedrooms,
        int bathrooms,
        Guid categoryId,
        Guid typeId,
        TransactionType transactionType,
        Guid architectId,
        Guid? createdBy);

    Task<Listing> UpdateAsync(
        Listing listing,
        string title,
        string description,
        string? location,
        decimal price,
        decimal landArea,
        decimal constructionArea,
        int bedrooms,
        int bathrooms,
        Guid categoryId,
        Guid typeId,
        TransactionType transactionType,
        Guid modifiedBy);
        
    Task PublishAsync(Listing listing, Guid publishedBy);
    
    Task UnpublishAsync(Listing listing, Guid unpublishedBy);
    
    Task ArchiveAsync(Listing listing, Guid archivedBy);
    
    Task UnarchiveAsync(Listing listing, Guid unarchivedBy);
    
    Task MoveToPortfolioAsync(Listing listing, Guid movedBy);
    
    void ValidateListingData(
        string title,
        string description,
        decimal price,
        decimal landArea,
        decimal constructionArea);
}
