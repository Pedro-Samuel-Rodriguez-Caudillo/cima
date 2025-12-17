using System;
using System.Linq.Expressions;
using cima.Domain.Entities;
using Volo.Abp.Specifications;

namespace cima.Domain.Specifications.Listings;

public class ListingsByArchitectSpecification : Specification<Listing>
{
    private readonly Guid _architectId;

    public ListingsByArchitectSpecification(Guid architectId)
    {
        _architectId = architectId;
    }

    public override Expression<Func<Listing, bool>> ToExpression()
    {
        return query => query.ArchitectId == _architectId;
    }
}
