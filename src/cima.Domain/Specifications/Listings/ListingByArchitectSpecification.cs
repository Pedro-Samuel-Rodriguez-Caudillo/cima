using System;
using System.Linq.Expressions;
using cima.Domain.Entities;
using Volo.Abp.Specifications;

namespace cima.Domain.Specifications.Listings;

/// <summary>
/// Specification para filtrar Listings por arquitecto.
/// </summary>
public class ListingByArchitectSpecification : Specification<Listing>
{
    private readonly Guid _architectId;

    public ListingByArchitectSpecification(Guid architectId)
    {
        _architectId = architectId;
    }

    public override Expression<Func<Listing, bool>> ToExpression()
    {
        return listing => listing.ArchitectId == _architectId;
    }
}
