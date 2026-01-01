using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace cima.Domain.Entities.Portfolio;

/// <summary>
/// Represents an image associated with a portfolio project.
/// </summary>
public class PortfolioImage : ValueObject
{
    public Guid ImageId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string ThumbnailUrl { get; private set; } = string.Empty;
    
    /// <summary>
    /// Order to display the image in the gallery.
    /// </summary>
    public int SortOrder { get; internal set; }
    
    public string AltText { get; private set; } = string.Empty;
    public string Caption { get; private set; } = string.Empty; // Added caption for storytelling
    public long FileSize { get; private set; }
    public string ContentType { get; private set; } = "image/jpeg";
    
    /// <summary>
    /// Tags specifically for this image (e.g., "Kitchen", "Before", "After")
    /// </summary>
    public string Tags { get; private set; } = string.Empty;

    // Constructor privado para EF Core
    private PortfolioImage()
    {
    }

    public PortfolioImage(
        Guid imageId,
        string url,
        int sortOrder,
        string thumbnailUrl = "",
        string altText = "",
        string caption = "",
        long fileSize = 0,
        string contentType = "image/jpeg",
        string tags = "")
    {
        ImageId = imageId;
        Url = url ?? throw new ArgumentNullException(nameof(url));
        SortOrder = sortOrder;
        ThumbnailUrl = string.IsNullOrWhiteSpace(thumbnailUrl) ? url ?? string.Empty : thumbnailUrl;
        AltText = altText ?? string.Empty;
        Caption = caption ?? string.Empty;
        FileSize = fileSize;
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType;
        Tags = tags ?? string.Empty;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ImageId;
        yield return Url;
        yield return ThumbnailUrl;
        yield return SortOrder;
        yield return AltText;
        yield return Caption;
        yield return FileSize;
        yield return ContentType;
        yield return Tags;
    }
}
