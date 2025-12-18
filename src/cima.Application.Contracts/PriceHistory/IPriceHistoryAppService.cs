using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace cima.PriceHistory;

public interface IPriceHistoryAppService : IApplicationService
{
    /// <summary>
    /// Obtiene el historial de cambios de precio de un listing
    /// </summary>
    Task<List<ListingPriceHistoryDto>> GetByListingIdAsync(Guid listingId);
    
    /// <summary>
    /// Búsqueda avanzada de historial (para investigación de fraude)
    /// </summary>
    Task<PagedResultDto<ListingPriceHistoryDto>> GetListAsync(GetPriceHistoryInput input);
}
