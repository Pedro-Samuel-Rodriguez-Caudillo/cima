using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Listings;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace cima.PriceHistory;

[Authorize(Roles = "admin")]
public class PriceHistoryAppService : ApplicationService, IPriceHistoryAppService
{
    private readonly IListingPriceHistoryRepository _priceHistoryRepository;

    public PriceHistoryAppService(IListingPriceHistoryRepository priceHistoryRepository)
    {
        _priceHistoryRepository = priceHistoryRepository;
    }

    public async Task<List<ListingPriceHistoryDto>> GetByListingIdAsync(Guid listingId)
    {
        var history = await _priceHistoryRepository.GetByListingIdAsync(listingId);
        
        return history.Select(h => new ListingPriceHistoryDto
        {
            Id = h.Id,
            ListingId = h.ListingId,
            OldPrice = h.OldPrice,
            NewPrice = h.NewPrice,
            ChangedAt = h.ChangedAt,
            ChangedByUserId = h.ChangedByUserId,
            ChangedByUserName = h.ChangedByUserName,
            ClientIpAddress = h.ClientIpAddress,
            UserAgent = h.UserAgent,
            CorrelationId = h.CorrelationId,
            ChangeReason = h.ChangeReason,
            SessionId = h.SessionId,
            AuthenticationMethod = h.AuthenticationMethod
        }).ToList();
    }

    public async Task<PagedResultDto<ListingPriceHistoryDto>> GetListAsync(GetPriceHistoryInput input)
    {
        var queryable = await _priceHistoryRepository.GetQueryableAsync();

        queryable = queryable
            .WhereIf(input.ListingId.HasValue, x => x.ListingId == input.ListingId)
            .WhereIf(input.UserId.HasValue, x => x.ChangedByUserId == input.UserId)
            .WhereIf(!string.IsNullOrWhiteSpace(input.UserName),
                x => x.ChangedByUserName != null && x.ChangedByUserName.Contains(input.UserName))
            .WhereIf(!string.IsNullOrEmpty(input.IpAddress), x => x.ClientIpAddress == input.IpAddress)
            .WhereIf(input.FromDate.HasValue, x => x.ChangedAt >= input.FromDate)
            .WhereIf(input.ToDate.HasValue, x => x.ChangedAt <= input.ToDate);

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var histories = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.ChangedAt)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var items = histories
            .Select(h => new ListingPriceHistoryDto
            {
                Id = h.Id,
                ListingId = h.ListingId,
                OldPrice = h.OldPrice,
                NewPrice = h.NewPrice,
                ChangedAt = h.ChangedAt,
                ChangedByUserId = h.ChangedByUserId,
                ChangedByUserName = h.ChangedByUserName,
                ClientIpAddress = h.ClientIpAddress,
                UserAgent = h.UserAgent,
                CorrelationId = h.CorrelationId,
                ChangeReason = h.ChangeReason,
                SessionId = h.SessionId,
                AuthenticationMethod = h.AuthenticationMethod
            })
            .ToList();

        return new PagedResultDto<ListingPriceHistoryDto>(totalCount, items);
    }
}
