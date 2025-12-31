using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace cima.Listings;

public interface IListingSaleAppService : IApplicationService
{
    Task<ListingSaleDto?> GetByListingIdAsync(Guid listingId);
    Task<decimal?> GetSuggestedAmountAsync(Guid listingId);
    Task<ListingSaleDto> CreateAsync(CreateListingSaleDto input);
    Task DeleteAsync(Guid saleId);
    Task<PagedResultDto<ListingSaleDto>> GetMySalesAsync(PagedAndSortedResultRequestDto input);
    Task<ArchitectSalesSummaryDto> GetMySalesSummaryAsync();
    Task<ArchitectSalesSummaryDto> GetSummaryByArchitectIdAsync(Guid architectId);
}
