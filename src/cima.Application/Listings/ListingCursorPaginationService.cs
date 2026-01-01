using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Services;

namespace cima.Listings;

public class ListingCursorPaginationService : DomainService
{
    public IQueryable<Listing> ApplyFilters(IQueryable<Listing> query, PropertySearchDto input)
    {
        if (input.TransactionType.HasValue)
        {
            query = query.Where(x => x.TransactionType == input.TransactionType.Value);
        }

        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == input.CategoryId.Value);
        }

        if (input.TypeId.HasValue)
        {
            query = query.Where(x => x.TypeId == input.TypeId.Value);
        }

        if (input.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= input.MinPrice.Value);
        }

        if (input.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= input.MaxPrice.Value);
        }

        if (input.MinBedrooms.HasValue)
        {
            query = query.Where(x => x.Bedrooms >= input.MinBedrooms.Value);
        }

        if (input.MinBathrooms.HasValue)
        {
            query = query.Where(x => x.Bathrooms >= input.MinBathrooms.Value);
        }

        if (input.MinArea.HasValue)
        {
            query = query.Where(x => x.LandArea >= input.MinArea.Value);
        }

        if (input.MaxArea.HasValue)
        {
            query = query.Where(x => x.LandArea <= input.MaxArea.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Location))
        {
            var loc = input.Location.Trim();
            // Usar .Value para acceder al string del ValueObject
            query = query.Where(x => x.Location != null && x.Location.Value.Contains(loc));
        }

        return query;
    }
}
