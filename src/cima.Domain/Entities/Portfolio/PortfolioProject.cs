using System;
using System.Collections.Generic;
using System.Linq;
using cima.Domain.Shared;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace cima.Domain.Entities.Portfolio;

/// <summary>
/// Represents a completed project or showcase item in the portfolio.
/// Focuses on visual storytelling, before/after, and client success.
/// </summary>
public class PortfolioProject : FullAuditedAggregateRoot<Guid>
{
    public const int MaxImages = 20;

    #region Properties
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty; // Rich Text allowed
    public string Location { get; private set; } = string.Empty; // e.g. "Zapopan, Jal.", not full address
    
    public DateTime? CompletionDate { get; private set; }
    public PortfolioCategory Category { get; private set; }
    
    // Testimonial Section
    public string Testimonial { get; private set; } = string.Empty;
    public string TestimonialAuthor { get; private set; } = string.Empty;

    // Visuals
    public string CoverImage { get; private set; } = string.Empty;
    public ICollection<PortfolioImage> Gallery { get; private set; } = new List<PortfolioImage>();

    // Publish Status
    public bool IsVisible { get; private set; }

    #endregion

    #region Constructors
    private PortfolioProject() { }

    public PortfolioProject(
        Guid id,
        string title,
        string description,
        string location,
        DateTime? completionDate,
        PortfolioCategory category)
        : base(id)
    {
        SetTitle(title);
        Description = description;
        Location = location;
        CompletionDate = completionDate;
        Category = category;
        IsVisible = false; // Draft by default
    }
    #endregion

    #region Business Logic

    public void UpdateInfo(
        string title,
        string description,
        string location,
        DateTime? completionDate,
        PortfolioCategory category)
    {
        SetTitle(title);
        Description = description;
        Location = location;
        CompletionDate = completionDate;
        Category = category;
    }

    public void SetTestimonial(string testimonial, string author)
    {
        Testimonial = testimonial ?? string.Empty;
        TestimonialAuthor = author ?? string.Empty;
    }

    public void SetCoverImage(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
             throw new BusinessException("Portfolio:CoverImageRequired");
        }
        CoverImage = url;
    }

    public void AddImage(Guid imageId, string url, string thumbnailUrl, string altText, string caption, string tags)
    {
        if (Gallery.Count >= MaxImages)
        {
            throw new BusinessException("Portfolio:MaxImagesExceeded").WithData("Max", MaxImages);
        }

        var nextOrder = Gallery.Any() ? Gallery.Max(i => i.SortOrder) + 1 : 0;
        
        var newImage = new PortfolioImage(
            imageId,
            url,
            nextOrder,
            thumbnailUrl,
            altText,
            caption,
            0, // FileSize optional for now
            "image/jpeg",
            tags
        );

        Gallery.Add(newImage);
    }

    public void RemoveImage(Guid imageId)
    {
        var image = Gallery.FirstOrDefault(x => x.ImageId == imageId);
        if (image == null) return;

        Gallery.Remove(image);
        ReindexImages();
    }

    public void ReorderImages(List<Guid> orderedIds)
    {
        if (orderedIds.Count != Gallery.Count || !Gallery.All(i => orderedIds.Contains(i.ImageId)))
        {
            return;
        }

        foreach (var img in Gallery)
        {
            var newOrder = orderedIds.IndexOf(img.ImageId);
            img.SortOrder = newOrder;
        }
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BusinessException("Portfolio:TitleRequired");
        }
        Title = title;
    }

    private void ReindexImages()
    {
        var sorted = Gallery.OrderBy(x => x.SortOrder).ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].SortOrder = i;
        }
    }

    #endregion
}
