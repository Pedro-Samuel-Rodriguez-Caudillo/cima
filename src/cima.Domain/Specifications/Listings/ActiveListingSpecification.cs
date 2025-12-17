using System;
using System.Linq.Expressions;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Volo.Abp.Specifications;

namespace cima.Domain.Specifications.Listings;

public class ActiveListingSpecification : Specification<Listing>
{
    public override Expression<Func<Listing, bool>> ToExpression()
    {
        return query => query.Status == ListingStatus.Published;
    }
}
