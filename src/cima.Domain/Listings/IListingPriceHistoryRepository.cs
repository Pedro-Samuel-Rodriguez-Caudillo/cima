using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using cima.Domain.Entities.Listings;

namespace cima.Domain.Listings;

/// <summary>
/// Repositorio para historial de precios de listings
/// </summary>
public interface IListingPriceHistoryRepository : IRepository<ListingPriceHistory, Guid>
{
    /// <summary>
    /// Obtiene el historial de precios de un listing
    /// </summary>
    Task<List<ListingPriceHistory>> GetByListingIdAsync(Guid listingId);
    
    /// <summary>
    /// Obtiene cambios de precio realizados por un usuario específico
    /// </summary>
    Task<List<ListingPriceHistory>> GetByUserIdAsync(Guid userId);
    
    /// <summary>
    /// Obtiene cambios de precio desde una IP específica (para investigación de fraude)
    /// </summary>
    Task<List<ListingPriceHistory>> GetByIpAddressAsync(string ipAddress);
}
