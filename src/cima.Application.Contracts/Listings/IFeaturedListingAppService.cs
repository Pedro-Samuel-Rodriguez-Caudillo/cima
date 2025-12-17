using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using cima.Listings.Outputs;

namespace cima.Listings;

/// <summary>
/// Interfaz para el servicio de propiedades destacadas
/// </summary>
public interface IFeaturedListingAppService : IApplicationService
{
    Task<List<FeaturedListingDto>> GetAllAsync();
    
    Task<PagedResultDto<FeaturedListingDto>> GetPagedAsync(GetFeaturedListingsDto input);
    
    /// <summary>
    /// Obtiene propiedades para el homepage (con caché y aleatorización)
    /// </summary>
    Task<List<ListingSummaryDto>> GetForHomepageAsync(int count = 6);
    
    Task<FeaturedListingDto> AddAsync(CreateFeaturedListingDto input);
    
    Task RemoveAsync(Guid featuredListingId);
    
    Task RemoveByListingIdAsync(Guid listingId);
    
    Task<bool> IsListingFeaturedAsync(Guid listingId);
    
    Task<int> GetCountAsync();
}
