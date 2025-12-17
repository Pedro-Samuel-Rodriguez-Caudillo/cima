using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace cima.Domain.Entities;

/// <summary>
/// Represents an image associated with a listing.
/// </summary>
public class ListingImage : ValueObject
{
    public Guid ImageId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string ThumbnailUrl { get; private set; } = string.Empty;
    
    /// <summary>
    /// Order to display the image.
    /// </summary>
    public int SortOrder { get; internal set; }
    
    public string AltText { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string ContentType { get; private set; } = "image/jpeg";

    // Constructor privado para EF Core
    private ListingImage()
    {
    }

    // Constructor público para crear instancias
    public ListingImage(
        Guid imageId,
        string url,
        int sortOrder,
        string thumbnailUrl = "",
        string altText = "",
        long fileSize = 0,
        string contentType = "image/jpeg")
    {
        ImageId = imageId;
        Url = url ?? throw new ArgumentNullException(nameof(url));
        SortOrder = sortOrder;
        ThumbnailUrl = string.IsNullOrWhiteSpace(thumbnailUrl) ? url ?? string.Empty : thumbnailUrl;
        AltText = altText ?? string.Empty;
        FileSize = fileSize;
        ContentType = string.IsNullOrWhiteSpace(contentType) ? "image/jpeg" : contentType;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ImageId;
        yield return Url;
        yield return ThumbnailUrl;
        yield return SortOrder;
        yield return AltText;
        yield return FileSize;
        yield return ContentType;
    }
}