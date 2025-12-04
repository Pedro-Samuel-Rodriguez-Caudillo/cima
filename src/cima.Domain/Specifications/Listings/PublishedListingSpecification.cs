using System;
using System.Linq.Expressions;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Volo.Abp.Specifications;

namespace cima.Domain.Specifications.Listings;

/// <summary>
/// Specification para filtrar solo Listings publicados.
/// </summary>
public class PublishedListingSpecification : Specification<Listing>
{
    public override Expression<Func<Listing, bool>> ToExpression()
    {
        return listing => listing.Status == ListingStatus.Published;
    }
}
