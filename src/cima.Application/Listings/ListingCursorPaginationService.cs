using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Common;
using cima.Domain.Entities;
using cima.Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace cima.Listings;

/// <summary>
/// Servicio para paginación basada en cursor de Listings
/// </summary>
public class ListingCursorPaginationService : ApplicationService
{
    private readonly IRepository<Listing, Guid> _listingRepository;

    public ListingCursorPaginationService(IRepository<Listing, Guid> listingRepository)
    {
        _listingRepository = listingRepository;
    }

    [AllowAnonymous]
    public async Task<CursorPagedResultDto<ListingCursorDto>> GetListingsWithCursorAsync(
        GetListingsCursorInput input)
    {
        var pageSize = Math.Clamp(input.PageSize, 1, 100);
        
        var queryable = await _listingRepository.WithDetailsAsync(
            l => l.Architect!,
            l => l.Images!);

        queryable = ApplyFilters(queryable, input);

        var cursorData = CursorUtils.ParseCursor(input.Cursor);

        if (cursorData.HasValue)
        {
            var (timestamp, id) = cursorData.Value;
            
            if (input.Direction == CursorDirection.Forward)
            {
                if (input.SortDescending)
                {
                    queryable = queryable.Where(l => 
                        l.CreatedAt < timestamp || 
                        (l.CreatedAt == timestamp && l.Id.CompareTo(id) < 0));
                }
                else
                {
                    queryable = queryable.Where(l => 
                        l.CreatedAt > timestamp || 
                        (l.CreatedAt == timestamp && l.Id.CompareTo(id) > 0));
                }
            }
            else
            {
                if (input.SortDescending)
                {
                    queryable = queryable.Where(l => 
                        l.CreatedAt > timestamp || 
                        (l.CreatedAt == timestamp && l.Id.CompareTo(id) > 0));
                }
                else
                {
                    queryable = queryable.Where(l => 
                        l.CreatedAt < timestamp || 
                        (l.CreatedAt == timestamp && l.Id.CompareTo(id) < 0));
                }
            }
        }

        queryable = ApplySorting(queryable, input.SortBy, input.SortDescending);

        var listings = await AsyncExecuter.ToListAsync(
            queryable.Take(pageSize + 1));

        var hasMore = listings.Count > pageSize;
        if (hasMore)
        {
            listings = listings.Take(pageSize).ToList();
        }

        if (input.Direction == CursorDirection.Backward)
        {
            listings.Reverse();
        }

        var items = listings.Select(l => new ListingCursorDto
        {
            Id = l.Id,
            Title = l.Title,
            Location = l.Location,
            Price = l.Price,
            LandArea = l.LandArea,
            Bedrooms = l.Bedrooms,
            Bathrooms = l.Bathrooms,
            Status = l.Status,
            Type = l.Type,
            Category = l.Category,
            TransactionType = l.TransactionType,
            CreatedAt = l.CreatedAt,
            MainImageUrl = l.Images?.FirstOrDefault()?.Url,
            ArchitectName = null // TODO: Add architect name mapping
        }).ToList();

        var result = new CursorPagedResultDto<ListingCursorDto>
        {
            Items = items,
            PageSize = pageSize,
            HasNextPage = input.Direction == CursorDirection.Forward ? hasMore : cursorData.HasValue,
            HasPreviousPage = input.Direction == CursorDirection.Backward ? hasMore : cursorData.HasValue
        };

        if (items.Any())
        {
            var firstItem = items.First();
            var lastItem = items.Last();

            result.PreviousCursor = CursorUtils.CreateCursor(firstItem.Id, firstItem.CreatedAt);
            result.NextCursor = CursorUtils.CreateCursor(lastItem.Id, lastItem.CreatedAt);
        }

        if (input.IncludeTotalCount)
        {
            var countQueryable = await _listingRepository.GetQueryableAsync();
            countQueryable = ApplyFilters(countQueryable, input);
            // Agregar OrderBy antes del Count para evitar el warning de EF Core
            countQueryable = countQueryable.OrderBy(l => l.Id);
            result.TotalCount = await AsyncExecuter.CountAsync(countQueryable);
        }

        return result;
    }

    private IQueryable<Listing> ApplyFilters(IQueryable<Listing> queryable, GetListingsCursorInput input)
    {
        queryable = queryable.Where(l => l.Status == ListingStatus.Published);

        if (!string.IsNullOrWhiteSpace(input.SearchTerm))
        {
            var term = input.SearchTerm.ToLower();
            queryable = queryable.Where(l =>
                l.Title.ToLower().Contains(term) ||
                (l.Location != null && l.Location.ToLower().Contains(term)) ||
                l.Description.ToLower().Contains(term));
        }

        if (input.Status.HasValue)
        {
            queryable = queryable.Where(l => l.Status == input.Status.Value);
        }

        if (input.MinPrice.HasValue)
        {
            queryable = queryable.Where(l => l.Price >= input.MinPrice.Value);
        }

        if (input.MaxPrice.HasValue)
        {
            queryable = queryable.Where(l => l.Price <= input.MaxPrice.Value);
        }

        if (input.MinBedrooms.HasValue)
        {
            queryable = queryable.Where(l => l.Bedrooms >= input.MinBedrooms.Value);
        }

        if (input.MinBathrooms.HasValue)
        {
            queryable = queryable.Where(l => l.Bathrooms >= input.MinBathrooms.Value);
        }

        if (input.ArchitectId.HasValue)
        {
            queryable = queryable.Where(l => l.ArchitectId == input.ArchitectId.Value);
        }

        if (input.PropertyType.HasValue)
        {
            queryable = queryable.Where(l => l.Type == input.PropertyType.Value);
        }

        if (input.Category.HasValue)
        {
            queryable = queryable.Where(l => l.Category == input.Category.Value);
        }

        if (input.TransactionType.HasValue)
        {
            queryable = queryable.Where(l => l.TransactionType == input.TransactionType.Value);
        }

        return queryable;
    }

    private IQueryable<Listing> ApplySorting(
        IQueryable<Listing> queryable, 
        string? sortBy, 
        bool sortDescending)
    {
        return (sortBy?.ToLower(), sortDescending) switch
        {
            ("price", false) => queryable.OrderBy(l => l.Price).ThenBy(l => l.Id),
            ("price", true) => queryable.OrderByDescending(l => l.Price).ThenByDescending(l => l.Id),
            ("landarea", false) => queryable.OrderBy(l => l.LandArea).ThenBy(l => l.Id),
            ("landarea", true) => queryable.OrderByDescending(l => l.LandArea).ThenByDescending(l => l.Id),
            ("title", false) => queryable.OrderBy(l => l.Title).ThenBy(l => l.Id),
            ("title", true) => queryable.OrderByDescending(l => l.Title).ThenByDescending(l => l.Id),
            (_, false) => queryable.OrderBy(l => l.CreatedAt).ThenBy(l => l.Id),
            (_, true) => queryable.OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id)
        };
    }
}
