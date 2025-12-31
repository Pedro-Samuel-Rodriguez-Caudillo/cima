using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Entities.Listings;
using cima.Domain.Shared;
using cima.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace cima.Listings;

public class ListingSaleAppService : ApplicationService, IListingSaleAppService
{
    private readonly IRepository<ListingSale, Guid> _listingSaleRepository;
    private readonly IRepository<Listing, Guid> _listingRepository;
    private readonly IRepository<Architect, Guid> _architectRepository;

    public ListingSaleAppService(
        IRepository<ListingSale, Guid> listingSaleRepository,
        IRepository<Listing, Guid> listingRepository,
        IRepository<Architect, Guid> architectRepository)
    {
        _listingSaleRepository = listingSaleRepository;
        _listingRepository = listingRepository;
        _architectRepository = architectRepository;
    }

    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingSaleDto?> GetByListingIdAsync(Guid listingId)
    {
        var listing = await _listingRepository.GetAsync(listingId);
        await EnsureCanManageSaleAsync(listing);

        var queryable = await _listingSaleRepository.WithDetailsAsync(
            sale => sale.Listing!,
            sale => sale.Architect!);
        var sale = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(s => s.ListingId == listingId));
        return sale == null ? null : MapToDto(sale);
    }

    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ListingSaleDto> CreateAsync(CreateListingSaleDto input)
    {
        var listing = await _listingRepository.GetAsync(input.ListingId);
        await EnsureCanManageSaleAsync(listing);

        if (listing.Status != ListingStatus.Published && listing.Status != ListingStatus.Portfolio)
        {
            throw new BusinessException("ListingSale:ListingNotPublished")
                .WithData("ListingId", listing.Id)
                .WithData("Status", listing.Status);
        }

        var existing = await _listingSaleRepository.AnyAsync(sale => sale.ListingId == input.ListingId);
        if (existing)
        {
            throw new BusinessException("ListingSale:AlreadySold")
                .WithData("ListingId", input.ListingId);
        }

        var architectId = listing.ArchitectId;
        var sale = new ListingSale(
            input.ListingId,
            architectId,
            input.SoldAt,
            input.Amount,
            input.Currency,
            input.Notes);

        await _listingSaleRepository.InsertAsync(sale, autoSave: true);

        var queryable = await _listingSaleRepository.WithDetailsAsync(
            s => s.Listing!,
            s => s.Architect!);
        var created = await AsyncExecuter.FirstAsync(queryable.Where(s => s.Id == sale.Id));
        return MapToDto(created);
    }

    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task DeleteAsync(Guid saleId)
    {
        var sale = await _listingSaleRepository.GetAsync(saleId);
        var listing = await _listingRepository.GetAsync(sale.ListingId);
        await EnsureCanManageSaleAsync(listing);

        await _listingSaleRepository.DeleteAsync(saleId);
    }

    [Authorize]
    public async Task<PagedResultDto<ListingSaleDto>> GetMySalesAsync(PagedAndSortedResultRequestDto input)
    {
        var architect = await GetCurrentArchitectAsync();
        if (!CurrentUser.IsInRole("admin") && architect == null)
        {
            return new PagedResultDto<ListingSaleDto>(0, new List<ListingSaleDto>());
        }

        var queryable = await _listingSaleRepository.WithDetailsAsync(
            sale => sale.Listing!,
            sale => sale.Architect!);
        if (!CurrentUser.IsInRole("admin"))
        {
            queryable = queryable.Where(sale => sale.ArchitectId == architect.Id);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(sale => sale.SoldAt)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<ListingSaleDto>(totalCount, items.Select(MapToDto).ToList());
    }

    [Authorize]
    public async Task<ArchitectSalesSummaryDto> GetMySalesSummaryAsync()
    {
        var architect = await GetCurrentArchitectAsync();
        if (!CurrentUser.IsInRole("admin") && architect == null)
        {
            return new ArchitectSalesSummaryDto
            {
                ArchitectId = Guid.Empty,
                TotalSales = 0,
                TotalAmount = 0,
                LastSaleAt = null
            };
        }

        if (CurrentUser.IsInRole("admin"))
        {
            return await BuildGlobalSalesSummaryAsync();
        }

        return await BuildArchitectSalesSummaryAsync(architect.Id);
    }

    [Authorize(cimaPermissions.Listings.Edit)]
    public async Task<ArchitectSalesSummaryDto> GetSummaryByArchitectIdAsync(Guid architectId)
    {
        if (!CurrentUser.IsInRole("admin"))
        {
            var architect = await GetCurrentArchitectAsync();
            if (architect == null || architect.Id != architectId)
            {
                throw new AbpAuthorizationException("No autorizado para ver ventas de este arquitecto.");
            }
        }

        return await BuildArchitectSalesSummaryAsync(architectId);
    }

    private async Task EnsureCanManageSaleAsync(Listing listing)
    {
        if (CurrentUser.IsInRole("admin"))
        {
            return;
        }

        var architect = await GetCurrentArchitectAsync();
        if (architect == null || listing.ArchitectId != architect.Id)
        {
            throw new AbpAuthorizationException("No autorizado para registrar venta de esta propiedad.");
        }
    }

    private async Task<ArchitectSalesSummaryDto> BuildArchitectSalesSummaryAsync(Guid architectId)
    {
        var queryable = await _listingSaleRepository.GetQueryableAsync();
        queryable = queryable.Where(sale => sale.ArchitectId == architectId);

        var sales = await AsyncExecuter.ToListAsync(queryable);
        var totalAmount = sales.Sum(sale => sale.Amount);
        var lastSaleAt = sales.OrderByDescending(sale => sale.SoldAt).Select(sale => (DateTime?)sale.SoldAt).FirstOrDefault();

        return new ArchitectSalesSummaryDto
        {
            ArchitectId = architectId,
            TotalSales = sales.Count,
            TotalAmount = totalAmount,
            LastSaleAt = lastSaleAt
        };
    }

    private async Task<ArchitectSalesSummaryDto> BuildGlobalSalesSummaryAsync()
    {
        var queryable = await _listingSaleRepository.GetQueryableAsync();
        var sales = await AsyncExecuter.ToListAsync(queryable);
        var totalAmount = sales.Sum(sale => sale.Amount);
        var lastSaleAt = sales.OrderByDescending(sale => sale.SoldAt)
            .Select(sale => (DateTime?)sale.SoldAt)
            .FirstOrDefault();

        return new ArchitectSalesSummaryDto
        {
            ArchitectId = Guid.Empty,
            TotalSales = sales.Count,
            TotalAmount = totalAmount,
            LastSaleAt = lastSaleAt
        };
    }

    private ListingSaleDto MapToDto(ListingSale sale)
    {
        var listingTitle = sale.Listing?.Title ?? string.Empty;
        var architectName = sale.Architect == null
            ? string.Empty
            : $"{sale.Architect.Name} {sale.Architect.Surname}".Trim();

        return new ListingSaleDto
        {
            Id = sale.Id,
            ListingId = sale.ListingId,
            ListingTitle = listingTitle,
            ArchitectId = sale.ArchitectId,
            ArchitectName = architectName,
            SoldAt = sale.SoldAt,
            Amount = sale.Amount,
            Currency = sale.Currency,
            Notes = sale.Notes
        };
    }

    private async Task<Architect?> GetCurrentArchitectAsync()
    {
        if (!CurrentUser.Id.HasValue)
        {
            return null;
        }

        var queryable = await _architectRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(a => a.UserId == CurrentUser.Id.Value));
    }
}
