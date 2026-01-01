using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Entities.Listings;
using cima.Domain.Entities.Portfolio;
using cima.Domain.Listings;
using cima.Domain.Services.Listings;
using cima.Domain.Shared;
using cima.Images;
using cima.Portfolio; // Contracts namespace
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace cima.Portfolio;

// TODO: Add specific permissions like [Authorize(cimaPermissions.Portfolio.Default)]
[Authorize] 
public class PortfolioAppService : ApplicationService, IPortfolioAppService
{
    private readonly IRepository<PortfolioProject, Guid> _portfolioRepository;
    private readonly IImageStorageService _imageStorageService;
    private readonly IListingRepository _listingRepository;
    private readonly IListingManager _listingManager;
    private readonly IPortfolioSyncService _portfolioSyncService;
    private readonly IRepository<PropertyCategoryEntity, Guid> _categoryRepository;

    public PortfolioAppService(
        IRepository<PortfolioProject, Guid> portfolioRepository,
        IImageStorageService imageStorageService,
        IListingRepository listingRepository,
        IListingManager listingManager,
        IPortfolioSyncService portfolioSyncService,
        IRepository<PropertyCategoryEntity, Guid> categoryRepository)
    {
        _portfolioRepository = portfolioRepository;
        _imageStorageService = imageStorageService;
        _listingRepository = listingRepository;
        _listingManager = listingManager;
        _portfolioSyncService = portfolioSyncService;
        _categoryRepository = categoryRepository;
    }

    public async Task<PortfolioProjectDto> GetAsync(Guid id)
    {
        var project = await _portfolioRepository.GetAsync(id);
        var dto = ObjectMapper.Map<PortfolioProject, PortfolioProjectDto>(project);
        await ApplyCategoryNameAsync(dto);
        return dto;
    }

    public async Task<PagedResultDto<PortfolioProjectDto>> GetListAsync(GetPortfolioListDto input)
    {
        var queryable = await _portfolioRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => x.Title.Contains(input.Filter) || x.Description.Contains(input.Filter));
        }

        if (input.CategoryId.HasValue)
        {
            queryable = queryable.Where(x => x.CategoryId == input.CategoryId.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        
        // Sorting default by CreationTime if not specified (Standard Entity)
        queryable = queryable.OrderByDescending(x => x.CreationTime)
                             .Skip(input.SkipCount)
                             .Take(input.MaxResultCount);

        var projects = await AsyncExecuter.ToListAsync(queryable);

        var dtos = ObjectMapper.Map<List<PortfolioProject>, List<PortfolioProjectDto>>(projects);
        await ApplyCategoryNamesAsync(dtos);

        return new PagedResultDto<PortfolioProjectDto>(totalCount, dtos);
    }

    public async Task<PortfolioProjectDto> CreateAsync(CreateUpdatePortfolioProjectDto input)
    {
        var project = new PortfolioProject(
            GuidGenerator.Create(),
            input.Title,
            input.Description,
            input.Location,
            input.CompletionDate,
            input.CategoryId
        );

        project.SetTestimonial(input.Testimonial, input.TestimonialAuthor);
        project.SetVisibility(input.IsVisible);

        await _portfolioRepository.InsertAsync(project);

        var dto = ObjectMapper.Map<PortfolioProject, PortfolioProjectDto>(project);
        await ApplyCategoryNameAsync(dto);
        return dto;
    }

    public async Task<PortfolioProjectDto> UpdateAsync(Guid id, CreateUpdatePortfolioProjectDto input)
    {
        var project = await _portfolioRepository.GetAsync(id);

        project.UpdateInfo(
            input.Title,
            input.Description,
            input.Location,
            input.CompletionDate,
            input.CategoryId
        );

        project.SetTestimonial(input.Testimonial, input.TestimonialAuthor);
        project.SetVisibility(input.IsVisible);

        await _portfolioRepository.UpdateAsync(project);

        var dto = ObjectMapper.Map<PortfolioProject, PortfolioProjectDto>(project);
        await ApplyCategoryNameAsync(dto);
        return dto;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _portfolioRepository.DeleteAsync(id);
    }

    public async Task AddImageAsync(Guid id, CreatePortfolioImageDto input)
    {
        var project = await _portfolioRepository.GetAsync(id);

        // Handle Image Upload Logic here (Simplified compared to Listings)
        // Assuming input.Url is already uploaded or we handle base64 here.
        // For simplicity, let's assume client uploads to API/Blob first and sends URL, 
        // OR we reuse the logic from ListingAppService for base64.
        
        string finalUrl = input.Url;
        string thumbnailUrl = input.Url; 

        // Minimal logic to replicate "StoreImageIfNeededAsync"
        // In a real scenario, refactor StoreImageIfNeededAsync to a shared Domain Service or use ImageStorageService directly.
        // For now, assume URL is valid or simple.
        
        project.AddImage(
            GuidGenerator.Create(),
            finalUrl,
            thumbnailUrl,
            input.AltText,
            input.Caption,
            input.Tags
        );

        await _portfolioRepository.UpdateAsync(project);
    }

    public async Task RemoveImageAsync(Guid id, Guid imageId)
    {
        var project = await _portfolioRepository.GetAsync(id);
        project.RemoveImage(imageId);
        await _portfolioRepository.UpdateAsync(project);
    }

    public async Task ReorderImagesAsync(Guid id, List<Guid> orderedIds)
    {
        var project = await _portfolioRepository.GetAsync(id);
        project.ReorderImages(orderedIds);
        await _portfolioRepository.UpdateAsync(project);
    }

    public async Task SetCoverImageAsync(Guid id, string url)
    {
        var project = await _portfolioRepository.GetAsync(id);
        project.SetCoverImage(url);
        await _portfolioRepository.UpdateAsync(project);
    }

    public async Task<PortfolioProjectDto> CreateFromListingAsync(Guid listingId)
    {
        var queryable = await _listingRepository.WithDetailsAsync(x => x.Images);
        var listing = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(l => l.Id == listingId));
        
        if (listing == null)
        {
            throw new EntityNotFoundException(typeof(Listing), listingId);
        }

        var project = await _portfolioSyncService.SyncFromListingAsync(
            listing,
            setVisible: true,
            requireImages: true,
            allowCreate: true);

        // Update Listing Status to Portfolio
        if (listing.Status != ListingStatus.Portfolio)
        {
            if (CurrentUser.Id.HasValue)
            {
                await _listingManager.MoveToPortfolioAsync(listing, CurrentUser.Id.Value);
            }
            else
            {
                 // Fallback or throw, though [Authorize] ensures user is there mostly.
                 // For safety in weird contexts:
                 throw new AbpAuthorizationException("User must be logged in to promote listing.");
            }
            await _listingRepository.UpdateAsync(listing);
        }

        var dto = ObjectMapper.Map<PortfolioProject, PortfolioProjectDto>(project!);
        await ApplyCategoryNameAsync(dto);
        return dto;
    }

    private async Task<Dictionary<Guid, string>> GetCategoryNamesAsync()
    {
        var categories = await _categoryRepository.GetListAsync();
        return categories.ToDictionary(c => c.Id, c => c.Name);
    }

    private static void ApplyCategoryName(PortfolioProjectDto dto, Dictionary<Guid, string> names)
    {
        dto.CategoryName = names.GetValueOrDefault(dto.CategoryId);
    }

    private async Task ApplyCategoryNameAsync(PortfolioProjectDto dto)
    {
        var names = await GetCategoryNamesAsync();
        ApplyCategoryName(dto, names);
    }

    private async Task ApplyCategoryNamesAsync(List<PortfolioProjectDto> dtos)
    {
        if (dtos.Count == 0)
        {
            return;
        }

        var names = await GetCategoryNamesAsync();
        foreach (var dto in dtos)
        {
            ApplyCategoryName(dto, names);
        }
    }
}
