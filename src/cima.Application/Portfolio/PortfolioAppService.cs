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

    public PortfolioAppService(
        IRepository<PortfolioProject, Guid> portfolioRepository,
        IImageStorageService imageStorageService,
        IListingRepository listingRepository,
        IListingManager listingManager)
    {
        _portfolioRepository = portfolioRepository;
        _imageStorageService = imageStorageService;
        _listingRepository = listingRepository;
        _listingManager = listingManager;
    }

    public async Task<PortfolioProjectDto> GetAsync(Guid id)
    {
        var project = await _portfolioRepository.GetAsync(id);
        return ObjectMapper.Map<PortfolioProject, PortfolioProjectDto>(project);
    }

    public async Task<PagedResultDto<PortfolioProjectDto>> GetListAsync(GetPortfolioListDto input)
    {
        var queryable = await _portfolioRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            queryable = queryable.Where(x => x.Title.Contains(input.Filter) || x.Description.Contains(input.Filter));
        }

        if (input.Category.HasValue)
        {
            queryable = queryable.Where(x => x.Category == input.Category.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        
        // Sorting default by CreationTime if not specified (Standard Entity)
        queryable = queryable.OrderByDescending(x => x.CreationTime)
                             .Skip(input.SkipCount)
                             .Take(input.MaxResultCount);

        var projects = await AsyncExecuter.ToListAsync(queryable);

        return new PagedResultDto<PortfolioProjectDto>(
            totalCount,
            ObjectMapper.Map<List<PortfolioProject>, List<PortfolioProjectDto>>(projects)
        );
    }

    public async Task<PortfolioProjectDto> CreateAsync(CreateUpdatePortfolioProjectDto input)
    {
        var project = new PortfolioProject(
            GuidGenerator.Create(),
            input.Title,
            input.Description,
            input.Location,
            input.CompletionDate,
            input.Category
        );

        project.SetTestimonial(input.Testimonial, input.TestimonialAuthor);
        project.SetVisibility(input.IsVisible);

        await _portfolioRepository.InsertAsync(project);

        return ObjectMapper.Map<PortfolioProject, PortfolioProjectDto>(project);
    }

    public async Task<PortfolioProjectDto> UpdateAsync(Guid id, CreateUpdatePortfolioProjectDto input)
    {
        var project = await _portfolioRepository.GetAsync(id);

        project.UpdateInfo(
            input.Title,
            input.Description,
            input.Location,
            input.CompletionDate,
            input.Category
        );

        project.SetTestimonial(input.Testimonial, input.TestimonialAuthor);
        project.SetVisibility(input.IsVisible);

        await _portfolioRepository.UpdateAsync(project);

        return ObjectMapper.Map<PortfolioProject, PortfolioProjectDto>(project);
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

        // 1. Create Portfolio Project based on Listing Data
        var portfolioCategory = MapCategory(listing.Category, listing.Type);
        var locationString = listing.Location?.Value ?? string.Empty;

        var project = new PortfolioProject(
            GuidGenerator.Create(),
            listing.Title,
            listing.Description,
            locationString,
            DateTime.UtcNow, // Completion date defaults to now (can be edited later)
            portfolioCategory
        );

        // 2. Copy Images
        // ListingImage -> PortfolioImage
        // We reuse the URLs so we don't need to duplicate blobs, just references.
        foreach (var img in listing.Images.OrderBy(x => x.SortOrder))
        {
            project.AddImage(
                GuidGenerator.Create(),
                img.Url,
                img.ThumbnailUrl,
                img.AltText,
                caption: "", // Default empty
                tags: "Imported" // Tag to know source
            );

            // Set first image as cover by default if not set
            if (string.IsNullOrEmpty(project.CoverImage))
            {
                project.SetCoverImage(img.Url);
            }
        }
        
        // 3. Save Portfolio Project
        await _portfolioRepository.InsertAsync(project);

        // 4. Update Listing Status to Portfolio
        // This removes it from active sales lists
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

        return ObjectMapper.Map<PortfolioProject, PortfolioProjectDto>(project);
    }

    private static PortfolioCategory MapCategory(PropertyCategory category, PropertyType type)
    {
        // Simple mapping logic - can be refined
        return category switch
        {
            PropertyCategory.Residential => PortfolioCategory.ResidentialConstruction,
            PropertyCategory.Commercial => PortfolioCategory.CommercialConstruction,
            PropertyCategory.Industrial => PortfolioCategory.CommercialConstruction,
            PropertyCategory.Land => PortfolioCategory.Other, // Selling land isn't usually a "construction project" unless landscaping
            PropertyCategory.Mixed => PortfolioCategory.ArchitecturalDesign,
            _ => PortfolioCategory.Other
        };
    }
}
