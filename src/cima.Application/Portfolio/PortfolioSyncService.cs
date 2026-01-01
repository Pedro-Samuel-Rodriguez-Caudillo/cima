using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using cima.Domain.Entities;
using cima.Domain.Entities.Listings;
using cima.Domain.Entities.Portfolio;
using cima.Listings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace cima.Portfolio;

public interface IPortfolioSyncService
{
    Task<PortfolioProject?> SyncFromListingAsync(
        Listing listing,
        bool setVisible,
        bool requireImages,
        bool allowCreate);
}

public class PortfolioSyncService : IPortfolioSyncService, ITransientDependency
{
    private readonly IRepository<PortfolioProject, Guid> _portfolioRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public PortfolioSyncService(
        IRepository<PortfolioProject, Guid> portfolioRepository,
        IGuidGenerator guidGenerator,
        IClock clock)
    {
        _portfolioRepository = portfolioRepository;
        _guidGenerator = guidGenerator;
        _clock = clock;
    }

    public async Task<PortfolioProject?> SyncFromListingAsync(
        Listing listing,
        bool setVisible,
        bool requireImages,
        bool allowCreate)
    {
        var project = await _portfolioRepository.FirstOrDefaultAsync(
            p => p.ListingId == listing.Id);

        var isNew = project == null;
        if (project == null)
        {
            if (!allowCreate)
            {
                return null;
            }

            project = new PortfolioProject(
                _guidGenerator.Create(),
                listing.Title,
                listing.Description,
                ResolveLocation(listing),
                _clock.Now,
                listing.CategoryId);
        }
        else
        {
            project.UpdateInfo(
                listing.Title,
                listing.Description,
                ResolveLocation(listing),
                project.CompletionDate,
                listing.CategoryId);
        }

        project.SetListingReference(listing.Id);
        project.SetVisibility(setVisible);

        SyncImages(project, listing, requireImages);

        if (isNew)
        {
            await _portfolioRepository.InsertAsync(project);
        }
        else
        {
            await _portfolioRepository.UpdateAsync(project);
        }

        return project;
    }

    private void SyncImages(PortfolioProject project, Listing listing, bool requireImages)
    {
        var orderedImages = listing.Images
            .OrderBy(image => image.SortOrder)
            .ToList();

        if (!orderedImages.Any())
        {
            if (requireImages)
            {
                throw new BusinessException("Listing:NoImages");
            }

            return;
        }

        project.Gallery.Clear();

        foreach (var image in orderedImages)
        {
            project.AddImage(
                _guidGenerator.Create(),
                image.Url,
                string.IsNullOrWhiteSpace(image.ThumbnailUrl) ? image.Url : image.ThumbnailUrl,
                image.AltText ?? string.Empty,
                caption: string.Empty,
                tags: "Listing");
        }

        project.SetCoverImage(orderedImages[0].Url);
    }

    private static string ResolveLocation(Listing listing)
    {
        var raw = listing.Location?.Value;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<LocationDto>(raw);
            return dto?.ToString() ?? raw;
        }
        catch
        {
            return raw;
        }
    }
}
