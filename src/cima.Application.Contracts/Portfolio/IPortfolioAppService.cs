using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace cima.Portfolio;

public interface IPortfolioAppService : IApplicationService
{
    Task<PortfolioProjectDto> GetAsync(Guid id);
    Task<PagedResultDto<PortfolioProjectDto>> GetListAsync(GetPortfolioListDto input);
    Task<PortfolioProjectDto> CreateAsync(CreateUpdatePortfolioProjectDto input);
    Task<PortfolioProjectDto> UpdateAsync(Guid id, CreateUpdatePortfolioProjectDto input);
    Task DeleteAsync(Guid id);
    
    // Image Management
    Task AddImageAsync(Guid id, CreatePortfolioImageDto input);
    Task RemoveImageAsync(Guid id, Guid imageId);
    Task ReorderImagesAsync(Guid id, List<Guid> orderedIds);
    Task SetCoverImageAsync(Guid id, string url);

    // Migration / Integration
    Task<PortfolioProjectDto> CreateFromListingAsync(Guid listingId);
}
