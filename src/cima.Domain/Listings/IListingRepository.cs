using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using cima.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Specifications;

namespace cima.Domain.Listings;

public interface IListingRepository : IRepository<Listing, Guid>
{
    Task<List<Listing>> GetDashboardListAsync(
        ISpecification<Listing> spec,
        string? sorting,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default
    );
    
    // Future optimization: dedicated method for Search to avoid loading heavy fields if needed
}
