using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Listings;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Specifications;

namespace cima.EntityFrameworkCore.Listings;

public class ListingRepository : EfCoreRepository<cimaDbContext, Listing, Guid>, IListingRepository
{
    public ListingRepository(IDbContextProvider<cimaDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    public async Task<List<Listing>> GetDashboardListAsync(
        ISpecification<Listing> spec,
        string? sorting,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var query = await GetQueryableAsync();
        
        // Aplicar especificación
        query = query.Where(spec.ToExpression());
        
        // Incluir detalles (se puede optimizar para dashboard si no se necesitan imagenes)
        query = query.Include(x => x.Architect);

        // Ordenamiento
        query = !string.IsNullOrWhiteSpace(sorting) 
            ? query.OrderBy(sorting) 
            : query.OrderByDescending(x => x.CreatedAt);
            
        return await query
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(cancellationToken);
    }
}
