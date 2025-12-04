using System;
using System.Linq.Expressions;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Volo.Abp.Specifications;

namespace cima.Domain.Specifications.Listings;

/// <summary>
/// Specification para filtrar Listings visibles públicamente (Published o Portfolio).
/// </summary>
public class PublicVisibleListingSpecification : Specification<Listing>
{
    public override Expression<Func<Listing, bool>> ToExpression()
    {
        return listing =>
            listing.Status == ListingStatus.Published ||
            listing.Status == ListingStatus.Portfolio;
    }
}
