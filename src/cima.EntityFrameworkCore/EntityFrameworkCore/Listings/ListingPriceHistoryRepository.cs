using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities.Listings;
using cima.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace cima.EntityFrameworkCore.Listings;

public class ListingPriceHistoryRepository : 
    EfCoreRepository<cimaDbContext, ListingPriceHistory, Guid>,
    IListingPriceHistoryRepository
{
    public ListingPriceHistoryRepository(IDbContextProvider<cimaDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    public async Task<List<ListingPriceHistory>> GetByListingIdAsync(Guid listingId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.ListingId == listingId)
            .OrderByDescending(x => x.ChangedAt)
            .ToListAsync();
    }

    public async Task<List<ListingPriceHistory>> GetByUserIdAsync(Guid userId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.ChangedByUserId == userId)
            .OrderByDescending(x => x.ChangedAt)
            .ToListAsync();
    }

    public async Task<List<ListingPriceHistory>> GetByIpAddressAsync(string ipAddress)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.ClientIpAddress == ipAddress)
            .OrderByDescending(x => x.ChangedAt)
            .ToListAsync();
    }
}
